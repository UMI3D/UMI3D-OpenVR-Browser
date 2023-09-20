using com.inetum.addonEulen.summary;
using System;
using System.Collections.Generic;
using System.Linq;
using umi3d.common;
using UnityEngine;

public static class SummaryGenerator
{
    public static void Generate(ByteContainer container)
    {
        try
        {
            string summaryTemplate = Resources.Load("eulen-summary-template").ToString();

            CodeTemplateGenerator generator = new CodeTemplateGenerator(summaryTemplate);

            string date = UMI3DSerializer.Read<string>(container);
            string sessionStart = UMI3DSerializer.Read<string>(container);
            string sessionEnd = UMI3DSerializer.Read<string>(container);
            string sessionDuration = UMI3DSerializer.Read<string>(container);
            int sumMediaSeen = UMI3DSerializer.Read<int>(container);
            int[] mediaSeen = UMI3DSerializer.ReadArray<int>(container);
            int[] examplesSeen = UMI3DSerializer.ReadArray<int>(container);
            bool[] success = UMI3DSerializer.ReadArray<bool>(container);
            int[] tryDone = UMI3DSerializer.ReadArray<int>(container);
            string[] logs = UMI3DSerializer.ReadArray<string>(container);

            generator.SetVariables(new Dictionary<string, object>()
            {
                {"date", date },
                {"sessionStart", sessionStart },
                {"sessionEnd", sessionEnd },
                {"sessionDuration", sessionDuration },
                {"sumMediaSeen", sumMediaSeen},
                {"numberMedia1Seen", mediaSeen[0] },
                {"numberMedia2Seen", mediaSeen[1] },
                {"numberMedia3Seen", mediaSeen[2] },
                {"nbProSeen1", examplesSeen[0] },
                {"nbProSeen2", examplesSeen[1] },
                {"nbProSeen3", examplesSeen[2] },
                {"nbProSeenTotal", examplesSeen[0] + examplesSeen[1] + examplesSeen[2]},
                {"nbTryTotal", tryDone[0] + tryDone[1] + tryDone[2] },
                {"nbTry1", tryDone[0] },
                {"nbTry2", tryDone[1] },
                {"nbTry3", tryDone[2] },
                {"success1", success[0] ? "Si" : "No" },
                {"success2", success[1] ? "Si" : "No" },
                {"success3", success[2] ? "Si" : "No" },
            }).SetIncludes(new()
            {
                {"logsEx1", logs[0] },
                {"logsEx2", logs[1] },
                {"logsEx3", logs[2] },
            }).SetConditions(new Dictionary<string, bool>()
            {
                {"ex1Success", success[0] },
                {"ex2Success", success[1] },
                {"ex3Success", success[2] },
            });

            string path = Application.persistentDataPath + "/Eulen";

            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }

            string summaryPath = path + "/eulen-summary-" + System.DateTime.Now.ToString("dd-MM-yyyy--hh-mm-ss") + ".html";

            System.IO.File.WriteAllText(summaryPath, generator.Generate());

            Application.OpenURL("file:///" + summaryPath);
        } catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }
}
