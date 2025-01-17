using UnityEngine;

namespace Common.Scripts.Utils
{
    public class Singleton<T> : MonoBehaviour where T : Component
    {
        private static T instance;
        public static T Instance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<T> ();
                    if (instance == null) {
                        GameObject obj = new GameObject
                        {
                            name = typeof(T).Name,
                            hideFlags = HideFlags.HideAndDontSave
                        };
                        instance = obj.AddComponent<T>();
                    }
                }
                return instance;
            }
        }
 
        public virtual void Awake ()
        {
            if (instance == null) {
                instance = this as T;
                DontDestroyOnLoad (gameObject.transform.root);
            } else {
                Destroy (gameObject);
            }
        }
    }
}
