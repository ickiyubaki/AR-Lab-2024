using System.Linq;
using UnityEngine;

namespace Common.Scripts.Extensions
{
    public static class ChildWithTagFinder
    {
        public static T[] FindComponentsInChildrenWithTag<T>(this GameObject parent, string tag,
            bool forceActive = false) where T : Component
        {
            if (parent == null || string.IsNullOrEmpty(tag))
            {
                throw new System.ArgumentNullException();
            }

            return parent.GetComponentsInChildren<T>(forceActive).Where(component => component.CompareTag(tag))
                .ToArray();
        }

        public static GameObject[] FindComponentsInChildrenWithTag(this GameObject parent, string tag,
            bool forceActive = false)
        {
            if (parent == null || string.IsNullOrEmpty(tag))
            {
                throw new System.ArgumentNullException();
            }

            return parent.GetComponentsInChildren<GameObject>(forceActive).Where(component => component.CompareTag(tag))
                .ToArray();
        }

        public static T FindComponentInChildWithTag<T>(this GameObject parent, string tag, bool forceActive = false)
            where T : Component
        {
            if (parent == null || string.IsNullOrEmpty(tag))
            {
                throw new System.ArgumentNullException();
            }

            return parent.GetComponentsInChildren<T>(forceActive)
                .FirstOrDefault(component => component.CompareTag(tag));
        }

        public static GameObject FindComponentInChildWithTag(this GameObject parent, string tag,
            bool forceActive = false)
        {
            if (parent == null || string.IsNullOrEmpty(tag))
            {
                throw new System.ArgumentNullException();
            }

            return parent.GetComponentsInChildren<GameObject>(forceActive)
                .FirstOrDefault(component => component.CompareTag(tag));
        }
    }
}