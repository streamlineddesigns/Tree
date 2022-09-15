using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StudioByStorm.Optimizations {
    
    public class Pool : ScriptableObject
    {
        public List<GameObject> pool;
        protected Transform Parent;
        protected GameObject ObjectToPool;
        
        public void DependencyInjection(GameObject objectToPool, Transform parent, int initialSize)
        {
            ObjectToPool = objectToPool;
            Parent = parent;

            pool = new List<GameObject>(initialSize);
            for (int i = 0; i < initialSize; i++) {
                pool.Add(Instantiate(objectToPool, parent));
            }
        }

        public GameObject Get()
        {
            GameObject first = null;// = pool.First(x => ! x.activeSelf);
            bool foundActive = false;
            for (int i = 0; i < pool.Count; i++) {
                if (! pool[i].activeSelf) {
                    first = pool[i];
                    foundActive = true;
                }
            }
            return (foundActive) ? first : IncrementPool();
        }

        protected GameObject IncrementPool()
        {
            GameObject go = Instantiate(ObjectToPool, Parent);
            pool.Add(go);
            return go;
        }
    }
}