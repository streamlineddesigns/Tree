using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StudioByStorm.Tutorials {

    public abstract class Tutorial : MonoBehaviour
    {
        public bool isFinished {
            get {
                return _isFinished;
            }
        }

        public bool isAborting {
            get {
                return _isAborting;
            }
        }

        protected bool isRunning;
        protected bool _isFinished;
        protected bool _isAborting;
        
        protected void OnDisable()
        {
            StopAllCoroutines();
        }

        public void Begin() 
        {
            isRunning = true;
            StartCoroutine(TutorialUpdate());
        }

        public void End() 
        {
            isRunning = false;
            StopCoroutine(TutorialUpdate());
        }

        public abstract void Init();

        protected abstract IEnumerator TutorialUpdate();

        public abstract IEnumerator WaitUntilFinished();

        public abstract void CleanUp();

    }

}