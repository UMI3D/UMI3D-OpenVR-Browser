using com.inetum.addonEulen.common.dtos;
using com.inetum.eulen.recording.app;
using UnityEngine;

namespace com.inetum.eulen
{
    /// <summary>
    /// Global class to check if user movement is correct.
    /// </summary>
    public class MovementValidator : MonoBehaviour, IMovementValidator
    {
        [SerializeField]
        private Movement movement;

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
            return movement.Validate(rightKneeGizmo, leftKneeGizmo, hipsGizmo, backGizmo, leftElbowGizmo, rightElbowGizmo, isBoxGrabbed, footL, footR, validationDto);
        }
    }

    public interface IMovementValidator
    {
        /// <summary>
        /// Validates global frame for a given frame.
        /// </summary>
        /// <param name="rightKneeGizmo"></param>
        /// <param name="leftKneeGizmo"></param>
        /// <param name="chestAngleGizmo"></param>
        /// <param name="hipsGizmo"></param>
        /// <param name="backBoneAngleGizmo"></param>
        /// <param name="leftElbowGizmo"></param>
        /// <param name="rightElbowGizmo"></param>
        /// <returns></returns>
        public bool Validate(AngleGizmo rightKneeGizmo, AngleGizmo leftKneeGizmo, AngleGizmo hipsGizmo, AngleGizmo backGizmo, AngleGizmo leftElbowGizmo, AngleGizmo rightElbowGizmo, bool isBoxGrabbed, Transform footL, Transform footR, MovementValidationDto validationDto);
    }
}