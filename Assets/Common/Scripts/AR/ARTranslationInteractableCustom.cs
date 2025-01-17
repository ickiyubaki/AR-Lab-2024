using Common.Scripts.Extensions;
using UnityEngine.XR.Interaction.Toolkit.AR;

namespace Common.Scripts.AR
{
    public class ARTranslationInteractableCustom : ARTranslationInteractable
    {
        /// <inheritdoc />
        protected override bool CanStartManipulationForGesture(DragGesture gesture)
        {
            return !gesture.startPosition.IsPointerOverUIObject() && base.CanStartManipulationForGesture(gesture);
        }
    }
}