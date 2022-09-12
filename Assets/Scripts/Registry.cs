using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StudioByStorm {

    public abstract class Registry<T1, T> : MonoBehaviour
    {        
        protected Dictionary<T1, T> _Registry =  new Dictionary<T1, T>();

        public void Add(T1 k, T v)
        {
            if (! _Registry.ContainsKey(k)) {
                _Registry.Add(k, v);
            }
        }

        public void Remove(T1 k)
        {
            if (_Registry.ContainsKey(k)) {
                _Registry.Remove(k);
            }
        }

        public bool Contains(T1 k)
        {
            if (_Registry.ContainsKey(k)) {
                return true;
            }
            return false;
        }

        public int Count()
        {
            return _Registry.Count;
        }

        public T TryGetValue(T1 k)
        {
            if (Contains(k)) {
                return _Registry[k];
            }
            return default(T);
        }

        public void UpdateValue(T1 k, T v)
        {
            if (Contains(k)) {
                _Registry[k] = v;
            }
        }

        public List<T> getAllAsList()
        {
            List<T> registryList = new List<T>();
            foreach (KeyValuePair<T1, T> entry in _Registry) {
                registryList.Add(entry.Value);
            }
            return registryList;
        }

    }

}