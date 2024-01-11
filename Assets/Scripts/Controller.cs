using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StudioByStorm {

    public class Controller : MonoBehaviour
    {        
        public ViewName ViewName;

        public void OnEnable()
        {
            StartCoroutine(DelayedEnable());
        }

        IEnumerator DelayedEnable()
        {
            yield return 0;
            GameManager.Singleton.ControllerRegistry.Add(ViewName, this);
        }

        public void OnDisable()
        {
            GameManager.Singleton.ControllerRegistry.Remove(ViewName);
        }
    }

}