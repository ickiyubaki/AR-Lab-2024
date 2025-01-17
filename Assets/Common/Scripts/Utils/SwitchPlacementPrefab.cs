using Common.Scripts.AR;
using UnityEngine;

namespace Common.Scripts.Utils
{
    [RequireComponent(typeof(ARPlacementInteractableMultiple))]
    public class SwitchPlacementPrefab : MonoBehaviour
    {
        private ARPlacementInteractableMultiple _arPlacementInteractableMultiple;

        protected void Awake()
        {
            _arPlacementInteractableMultiple = GetComponent<ARPlacementInteractableMultiple>();
        }

        public void SwapPlacementObject(GameObject modelPrefab)
        {
            if (modelPrefab != null)
            {
                _arPlacementInteractableMultiple.PlacementPrefab = modelPrefab;
            }
        }
    }
}