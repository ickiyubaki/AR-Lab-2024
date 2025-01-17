using Common.Scripts.Extensions;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.AR;

namespace Common.Scripts.AR
{
    public class ARSelectionInteractableCustom : ARSelectionInteractable
    {
        /// <inheritdoc />
        protected override void OnEndManipulation(TapGesture gesture)
        {
            if (!gesture.startPosition.IsPointerOverUIObject())
            {
                base.OnEndManipulation(gesture);
            }
        }

        /// <inheritdoc />
        protected override bool CanStartManipulationForGesture(TapGesture gesture)
        {
            return gameObject.activeSelf && base.CanStartManipulationForGesture(gesture);
        }

        /// <inheritdoc />
        protected override void OnStartManipulation(TapGesture gesture)
        {
            if (!gesture.startPosition.IsPointerOverUIObject() && gesture.targetObject == gameObject)
            {
                Models.Instance.SelectedModel = gesture.targetObject;
            }
            
            base.OnStartManipulation(gesture);
        }

        /// <inheritdoc />
        protected  override void OnSelectExiting(SelectExitEventArgs args)
        {
            if (gameObject == Models.Instance.SelectedModel)
            {
                Models.Instance.SelectedModel = null;
            }
            base.OnSelectExiting(args);
        }
    }
}