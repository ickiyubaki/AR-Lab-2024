using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Common.Scripts.Extensions
{
    public static class PointerOverUI
    {
        /// <summary>
        /// Blocking raycast on plane when UI is hit.
        /// </summary>
        /// <param name="touchPosition">touch position</param>
        /// <returns>Returns if UI was hit</returns>
        public static bool IsPointerOverUIObject(this Vector2 touchPosition)
        {
            PointerEventData pointerEventData = new PointerEventData(EventSystem.current)
            {
                position =  new Vector2(touchPosition.x, touchPosition.y)
            };
            List<RaycastResult> raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerEventData, raycastResults);

            return raycastResults.Count > 0;
        }
    }
}
