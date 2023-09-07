using inetum.unityUtils;
using System.Collections.Generic;
using System.Linq;

namespace com.inetum.addonEulen.common.dtos
{
    [System.Serializable]
    public class MovementValidationDto
    {
        public int movementId = 0;

        public bool isValid = true;

        public List<string> logMessages = new ();

        public override string ToString()
        {
            return "[MovementValidationDto] Movement id " + movementId + "; is valid " + isValid + "\nLogs : \n" + formatedLogs; 
        }

        public string formatedLogs
        {
            get => logMessages.Count == 0 ? string.Empty : string.Join("\n", logMessages);
        }
    }
}