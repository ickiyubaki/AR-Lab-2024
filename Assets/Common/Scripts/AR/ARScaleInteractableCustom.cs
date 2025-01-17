using Common.Scripts.Extensions;
using UnityEngine.XR.Interaction.Toolkit.AR;

namespace Common.Scripts.AR
{
    public class ARScaleInteractableCustom : ARScaleInteractable
    {
        /// <inheritdoc />
        protected override bool CanStartManipulationForGesture(PinchGesture gesture)
        {
            if (gesture.startPosition1.IsPointerOverUIObject() || gesture.startPosition2.IsPointerOverUIObject())
            {
                return false;
            }

            return base.CanStartManipulationForGesture(gesture);
        }
    }
}