using com.inetum.addonEulen.common.dtos;
using com.inetum.eulen.recording.app;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

namespace com.inetum.eulen
{
    /// <summary>
    /// Describes a movement.
    /// </summary>
    [CreateAssetMenu(fileName = "MovementToDetect", menuName = "Eulen/Movement to detect")]
    public class Movement : ScriptableObject, IMovementValidator
    {
        /// <summary>
        /// Conditions to consider the movement correct.
        /// </summary>
        public List<MovementRequirement> conditions = new List<MovementRequirement>();

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool Validate(AngleGizmo rightKneeGizmo, AngleGizmo leftKneeGizmo, AngleGizmo hipsGizmo, AngleGizmo backGizmo, AngleGizmo leftElbowGizmo, AngleGizmo rightElbowGizmo, bool isBoxGrabbed, Transform footL, Transform footR, MovementValidationDto validationDto)
        {
            rightKneeGizmo.isError = false;
            leftKneeGizmo.isError = false;
            hipsGizmo.isError = false;
            leftElbowGizmo.isError = false;
            rightElbowGizmo.isError = false;

            bool res = true;

            foreach (var condition in conditions)
            {
                if (!condition.Validate(rightKneeGizmo, leftKneeGizmo, hipsGizmo, backGizmo, leftElbowGizmo, rightElbowGizmo, isBoxGrabbed, footL, footR, validationDto))
                {
                    res = false;
                }
            }

            return res;
        }
    }

    [System.Serializable]
    public class MovementRequirement : IMovementValidator
    {
        /// <summary>
        /// Name
        /// </summary>
        public string ruleName;

        /// <summary>
        /// Error that will displayed on the screen
        /// </summary>
        [Tooltip("The error message that will be displayed on the screen")]
        public string errorLog;

        [Header("Only for Precondition Test")]
        [Tooltip("Check if the box is grabbed")]
        public bool isBoxGrabbed;

        /// <summary>
        /// Enables if we should check the distance between feets and box
        /// </summary>
        [Header("Only for Postcondition Test")]
        [Tooltip("Check the user's feet position")]
        public bool checkFeetPosition;

        /// <summary>
        /// Distance on the X axis to the box
        /// </summary>
        public float xDistanceToBox;

        [Space(16f)]
        /// <summary>
        /// Conditions to check if <see cref="postConditions"/> are checked.
        /// </summary>
        public List<MovementCondition> preConditions = new List<MovementCondition>();

        /// <summary>
        /// Angles to check when <see cref="preConditions"/> are checked.
        /// </summary>
        public List<MovementCondition> postConditions = new List<MovementCondition>();

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="rightKneeGizmo"></param>
        /// <param name="leftKneeGizmo"></param>
        /// <param name="chestAngleGizmo"></param>
        /// <param name="hipsGizmo"></param>
        /// <param name="backBoneAngleGizmo"></param>
        /// <param name="leftElbowGizmo"></param>
        /// <param name="rightElbowGizmo"></param>
        /// <returns></returns>
        public bool Validate(AngleGizmo rightKneeGizmo, AngleGizmo leftKneeGizmo, AngleGizmo hipsGizmo, AngleGizmo backGizmo, AngleGizmo leftElbowGizmo, AngleGizmo rightElbowGizmo, bool isBoxGrabbed, Transform footL, Transform footR, MovementValidationDto validationDto)
        {
            bool preConditionTest = true;

            if (this.isBoxGrabbed != isBoxGrabbed) { return true; }
            // RestartGizmoErrors(rightKneeGizmo, leftKneeGizmo, hipsGizmo, backGizmo, rightElbowGizmo, leftElbowGizmo);

            foreach (var condition in preConditions)
            {
                condition.isPreCondition = true;

                if (!condition.Validate(rightKneeGizmo, leftKneeGizmo, hipsGizmo, backGizmo, leftElbowGizmo, rightElbowGizmo, true, footL, footR, validationDto))
                {
                    preConditionTest = false;
                    return true;
                }
            }
            if (preConditionTest)
            {
                bool postConditionTest = true;

                // Debug.Log($"<color=#77ffaa>Rule: {ruleName}</color>");
                foreach (var condition in postConditions)
                {
                    // If it's the 3rd movement don't take care of the feet position
                    if (validationDto.movementId == 2) checkFeetPosition = false;
                    condition.SetDistanceToBox(checkFeetPosition, xDistanceToBox);

                    if (!condition.Validate(rightKneeGizmo, leftKneeGizmo, hipsGizmo, backGizmo, leftElbowGizmo, rightElbowGizmo, true, footL, footR, validationDto))
                    {
                        postConditionTest = false;

                        if (validationDto != null)
                        {
                            // Check if the error is performed for 60 frames (errorUser), if it is, the validation is wrong and add the respective log
                            if (validationDto.isValid && DrawAvatar.errorUser)
                            {
                                validationDto.isValid = false;
                            }
                            if (!validationDto.logMessages.Contains(errorLog) && DrawAvatar.errorUser)
                                validationDto.logMessages.Add(errorLog);
                        }
                    }
                }

                return postConditionTest;
            }
            else
            {
                return true;
            }
        }
    }

    [System.Serializable]
    public class MovementCondition : IMovementValidator
    {
        /// <summary>
        /// Angle to check
        /// </summary>
        public AngleType type;

        /// <summary>
        /// How to check angle;
        /// </summary>
        public TestKeyword test;

        /// <summary>
        /// Expected value.
        /// </summary>
        public float angle;

        /// <summary>
        /// The minor value to use for "Between test"
        /// </summary>
        [Tooltip("Smallest angle. Only works with 'Between' test")]
        public float smallAngle;

        private bool checkFeetPosition;
        private float xDistBox = 0;

        // public bool isBoxGrabbed;
        [HideInInspector]
        public bool isPreCondition = false;

        public static int[] wrongGizmos = new int[6];
        public static AngleGizmo[] gizmosAux = new AngleGizmo[6];

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool Validate(AngleGizmo rightKneeGizmo, AngleGizmo leftKneeGizmo, AngleGizmo hipsGizmo, AngleGizmo backGizmo, AngleGizmo leftElbowGizmo, AngleGizmo rightElbowGizmo, bool isBoxGrabbed, Transform footL, Transform footR, MovementValidationDto dto)
        {
            AngleGizmo gizmoToCheck = null;
            bool feetOk = true;
            int aux = 0;

            // Test
            gizmosAux[0] = rightKneeGizmo;
            gizmosAux[1] = leftKneeGizmo;
            gizmosAux[2] = leftElbowGizmo;
            gizmosAux[3] = rightElbowGizmo;
            gizmosAux[4] = backGizmo;
            gizmosAux[5] = hipsGizmo;
            //

            switch (type)
            {
                case AngleType.RightKnee:
                    gizmoToCheck = rightKneeGizmo;
                    break;
                case AngleType.LeftKnee:
                    gizmoToCheck = leftKneeGizmo;
                    break;
                case AngleType.HipsTwisting:
                    gizmoToCheck = hipsGizmo;
                    break;
                case AngleType.BackInclination:
                    gizmoToCheck = backGizmo;
                    break;
                case AngleType.RightElbow:
                    gizmoToCheck = rightElbowGizmo;
                    break;
                case AngleType.LeftElbow:
                    gizmoToCheck = leftElbowGizmo;
                    break;
                default:
                    return false;
            }

            bool res = false;

            switch (test)
            {
                case TestKeyword.Always:
                    return true;
                case TestKeyword.Equal:
                    res = gizmoToCheck.angle == angle;
                    break;
                case TestKeyword.GreaterThan:
                    res = gizmoToCheck.angle > angle;
                    break;
                case TestKeyword.GreaterThanOrEqual:
                    res = gizmoToCheck.angle >= angle;
                    break;
                case TestKeyword.Less:
                    res = gizmoToCheck.angle < angle;
                    break;
                case TestKeyword.LessOrEqual:
                    res = gizmoToCheck.angle <= angle;
                    break;
                case TestKeyword.Between:   //x: 2.6 - 4    3.3 mid
                    res = angle > gizmoToCheck.angle && gizmoToCheck.angle > smallAngle;
                    break;
                default:
                    res = false;
                    break;
            }

            if (!isBoxGrabbed) res = false;


            if (checkFeetPosition)
            {
                aux = ValidateAlignedFeet(footR, footL);
                if (res && aux != 3) feetOk = false;
            }

            // In case the postconditions are wrong
            if (!isPreCondition && (!res || !feetOk))
            {
                // In case we are validating the angles (The angle will change their color to red)
                if (!res)
                {
                    if (test != TestKeyword.Between) Debug.Log(this + " Current value " + gizmoToCheck.angle);
                    else Debug.Log($"<color=#ff9933>[MovementCondition]</color> Check if {type} angle is {test} {angle} and {smallAngle}. <color=#ffbbbb>Current value: </color>" + gizmoToCheck.angle);
                    gizmoToCheck.isError = true;

                    switch (gizmoToCheck.name)
                    {
                        case "RK":
                            wrongGizmos[0]++;
                            break;
                        case "LK":
                            wrongGizmos[1]++;
                            break;
                        case "LE":
                            wrongGizmos[2]++;
                            break;
                        case "RE":
                            wrongGizmos[3]++;
                            break;
                        case "W":
                            wrongGizmos[4]++;
                            break;
                        case "H":
                            wrongGizmos[5]++;
                            break;
                        default: break;
                    }


                }

                // In case we are validating the feet position (There are no visual feedback on the wireframe just on the "Screen Results")
                if (checkFeetPosition && !feetOk)
                {
                    if (aux <= 1) Debug.Log($"<color=#ff9933>[MovementCondition]</color> Check if the <color=#33cccc>feet</color> are on the right X position, current value ->" +
                        $" Right: {footR.position.x} Expected Between: {xDistBox - 0.04f} - {xDistBox + 0.04f}, Left: {footL.position.x} Expected Between: {-xDistBox - 0.04f} - {-xDistBox + 0.04f}");

                    if (aux == 0 || aux == 2) Debug.Log($"<color=#ff9933>[MovementCondition]</color> Check if the <color=#33eeee>feet</color> are aligned, current value ->" +
                        $" Left: {footL.position.z} Expected Between: {footR.position.z + 0.07f} - {footR.position.z - 0.07f}");
                }
            }
            // In case the postconditions are ok
            else
            {
                switch (gizmoToCheck.name)
                {
                    case "RK":
                        // if (!(wrongGizmos[0] >= 60)) wrongGizmos[0] = 0;
                        wrongGizmos[0] = 0;
                        break;
                    case "LK":
                        // if (!(wrongGizmos[0] >= 60)) wrongGizmos[1] = 0;
                        wrongGizmos[1] = 0;
                        break;
                    case "LE":
                        // if (!(wrongGizmos[0] >= 60)) wrongGizmos[2] = 0;
                        wrongGizmos[2] = 0;
                        break;
                    case "RE":
                        // if (!(wrongGizmos[0] >= 60)) wrongGizmos[3] = 0;
                        wrongGizmos[3] = 0;
                        break;
                    case "W":
                        // if (!(wrongGizmos[0] >= 60)) wrongGizmos[4] = 0;
                        wrongGizmos[4] = 0;
                        break;
                    case "H":
                        // if (!(wrongGizmos[0] >= 60)) wrongGizmos[5] = 0;
                        wrongGizmos[5] = 0;
                        break;
                    default: break;
                }

                if (DrawAvatar.isExtraTime)
                {
                    Debug.Log("Nothing to do");

                    for (int i = 0; i < DrawAvatar.isGizmoErrorExtraTime.Length; i++)
                    {
                        if (DrawAvatar.isGizmoErrorExtraTime[i]) gizmosAux[i].isError = true;
                    }
                }
                else
                {
                    gizmoToCheck.isError = false;
                }

                //
                /*if (!DrawAvatar.isExtraTime)
                {
                    Debug.Log($"No es tiempo extra");

                    for(int i = 0; i < gizmosAux.Length; i++)
                    {
                        gizmosAux[i].isError = false;
                    }

                }
                else
                {
                    Debug.Log($"Es tiempo extra");

                    for (int i = 0; i < wrongGizmos.Length; i++)
                    {
                        if (wrongGizmos[i] >= 60) { gizmosAux[i].isError = true; Debug.Log($"Gizmo extra time: {gizmosAux[i].name}"); }
                    }
                }*/
                //
            }

            if (!feetOk) res = false;

            return res;
        }

        public void SetDistanceToBox(bool check, float xDistanceToBox)
        {
            checkFeetPosition = check;
            xDistBox = xDistanceToBox;
        }

        /// <summary>
        /// Validate the feet alignment and distance to the box
        /// </summary>
        /// <param name="rFoot"></param>
        /// <param name="lFoot"></param>
        /// <returns>Are the feet well positioned?</returns>
        private int ValidateAlignedFeet(Transform rFoot, Transform lFoot)
        {
            float offset = 0.1f;
            float offsetB = 0.1f;
            int res = 0;

            // Each foot between the "offsets" (First two conditions are so that the Right foot must remain in that interval, the other two for the left)
            if (xDistBox - 0.04f < rFoot.position.x && rFoot.position.x < xDistBox + 0.04f && -xDistBox - 0.04f < lFoot.position.x && lFoot.position.x < -xDistBox + 0.04f) res++;

            // Not sure what are we trying to validate here ?????
            if (rFoot.position.x > xDistBox && lFoot.position.x < -xDistBox)
            {
                if (-rFoot.position.x - offsetB < lFoot.position.x && lFoot.position.x < -rFoot.position.x + offset) res++;
            }
            //

            // Aligned feet (Foot ahead or behind the other)
            if (rFoot.position.z + offset > lFoot.position.z && lFoot.position.z > rFoot.position.z - offset) res += 2;

            return res;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override string ToString()
        {
            return $"<color=#ff9933>[MovementCondition]</color> Check if {type} angle is {test} than {angle}.";
        }
    }

    public enum TestKeyword
    {
        Always, Equal, GreaterThan, GreaterThanOrEqual, Less, LessOrEqual, Between
    }

    public enum AngleType
    {
        RightKnee, LeftKnee, HipsTwisting, BackInclination, RightElbow, LeftElbow, RightFoot, LeftFoot
    }
}