using System.Collections.Generic;
using UnityEngine;

namespace Common.Scripts.Utils
{
    public class PoolManager : Singleton<PoolManager>
    {
        [SerializeField]
        private Transform poolingContainer;
        
        private readonly Dictionary<string, Queue<GameObject>>
            _objectPoolDictionary = new Dictionary<string, Queue<GameObject>>();

        public void CreatePool(GameObject go, int poolSize)
        {
            var poolKey = go.name;

            if (_objectPoolDictionary.TryGetValue(poolKey, out Queue<GameObject> objectPool))
            {
                if (poolSize <= objectPool.Count) return;
                for (var i = 0; i < poolSize - objectPool.Count; i++)
                {
                    objectPool.Enqueue( CreateNewObject(go, false));
                }
            }
            else
            {
                Queue<GameObject> newObjectQueue = new Queue<GameObject>();

                for (var i = 0; i < poolSize; i++)
                {
                    newObjectQueue.Enqueue( CreateNewObject(go, false));
                }

                _objectPoolDictionary.Add(poolKey, newObjectQueue);
            }
        }
        
        public GameObject Get(GameObject go)
        {
            var poolKey = go.name;
            
            if (_objectPoolDictionary.TryGetValue(poolKey, out Queue<GameObject> objectPool))
            {
                if (objectPool.Count == 0)
                {
                    return CreateNewObject(go, true);
                }

                var obj = objectPool.Dequeue();
                obj.SetActive(true);
                return obj;
            }

            return CreateNewObject(go, true);
        }

        public void Return(GameObject go)
        {
            var poolKey = go.name;
            
            go.SetActive(false);
            
            if (_objectPoolDictionary.TryGetValue(poolKey, out Queue<GameObject> objectPool))
            {
                objectPool.Enqueue(go);
            }
            else
            {
                Queue<GameObject> newObjectQueue = new Queue<GameObject>();
                newObjectQueue.Enqueue(go);
                _objectPoolDictionary.Add(poolKey, newObjectQueue);
            }
        }

        public void Return(List<GameObject> gameObjects)
        {
            foreach (var go in gameObjects)
            {
                Return(go);
            }
        }

        private GameObject CreateNewObject(GameObject go, bool active)
        {
            var newGo = Instantiate(go, poolingContainer);
            newGo.SetActive(active);
            newGo.name = go.name;
            return newGo;
        }
    }
}