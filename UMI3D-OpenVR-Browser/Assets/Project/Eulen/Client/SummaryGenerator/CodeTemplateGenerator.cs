namespace com.inetum.addonEulen.summary
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using UnityEngine;

    public class CodeTemplateGenerator
    {
        public const string startVariableTag = "{{";
        public const string endVariableTag = "}}";
        public const string conditionTag = "!";
        public const string includeTag = "%";

        private Dictionary<string, object> variables = new();
        private Dictionary<string, bool> conditions = new();
        private Dictionary<string, string> includes = new();

        string sourceTemplate = string.Empty;

        public CodeTemplateGenerator(string sourceTemplate)
        {
            this.sourceTemplate = sourceTemplate;
        }

        public CodeTemplateGenerator SetVariables(Dictionary<string, object> variables)
        {
            this.variables = new(variables);

            return this;
        }

        public CodeTemplateGenerator SetConditions(Dictionary<string, bool> conditions)
        {
            this.conditions = new(conditions);

            return this;
        }

        public CodeTemplateGenerator SetIncludes(Dictionary<string, string> includes)
        {
            this.includes = new(includes);

            return this;
        }

        public string Generate()
        {
            string res = sourceTemplate;

            Regex regexVariableTag = new Regex(@"(?s){{.+?}}");
            Regex regexConditionTag = new Regex(@"(?s){{" + conditionTag + ".+?" + conditionTag + "}}");
            Regex regexIncludeTag = new Regex(@"(?s){{" + includeTag + ".+?" + includeTag + "}}");

            MatchCollection matches = regexVariableTag.Matches(res);

            foreach (Match match in matches)
            {
                try
                {
                    if (regexConditionTag.IsMatch(match.Value))
                    {
                        string condition = match.Value.Substring(3, match.Value.Length - 6).Trim();
                        var parameters = condition.Split("?");

                        string key = parameters[0].Trim();
                        string valueTrue = parameters[1].Trim();
                        string valueFalse = parameters[2].Trim();

                        if (conditions.ContainsKey(key))
                        {
                            res = res.Replace(match.Value, conditions[key] ? valueTrue : valueFalse);
                        }
                        else
                        {
                            res = res.Replace(match.Value, string.Empty);
                            Debug.LogError("No conditions value for key " + key);
                        }
                    }
                    else if (regexIncludeTag.IsMatch(match.Value))
                    {
                        string key = match.Value.Substring(3, match.Value.Length - 6).Trim();

                        if (includes.ContainsKey(key))
                        {
                            res = res.Replace(match.Value, includes[key]?.ToString());
                        }
                        else
                        {
                            res = res.Replace(match.Value, string.Empty);
                            Debug.LogError("No include value for key " + key);
                        }
                    }
                    else
                    {
                        string key = match.Value.Substring(2, match.Value.Length - 4).Trim();

                        if (variables.ContainsKey(key))
                        {
                            res = res.Replace(match.Value, variables[key]?.ToString());
                        }
                        else
                        {
                            res = res.Replace(match.Value, string.Empty);
                            Debug.LogError("No variable value for key " + key + " " + variables.FirstOrDefault().Key);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }

            }

            return res;
        }
    }
}