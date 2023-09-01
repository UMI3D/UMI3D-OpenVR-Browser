using System.Collections.Generic;

namespace com.inetum.addonEulen.common.dtos
{
    [System.Serializable]
    public class MovementValidationDto
    {
        public int movementId = 0;

        public bool isValid = true;

        public List<string> logMessages = new();
    }
}