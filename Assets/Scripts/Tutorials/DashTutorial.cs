using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StudioByStorm.EventPublishers;

namespace StudioByStorm.Tutorials {

    public class DashTutorial : Tutorial
    {
        private bool didPlayerDash = false;

        protected void OnEnable()
        {
            GameEventPublisher.OnPlayerDash += OnPlayerDash;
        }

        protected void OnDisable()
        {
            base.OnDisable();
            GameEventPublisher.OnPlayerDash -= OnPlayerDash;
        }

        protected void OnPlayerDash()
        {
            didPlayerDash = true;
        }

        public override void Init()
        {
            
        }

        protected override IEnumerator TutorialUpdate()
        {
            while(isRunning) {
                yield return new WaitForSeconds(0.0333f);
            }
        }

        public override IEnumerator WaitUntilFinished()
        {
            yield return new WaitUntil(() => didPlayerDash);
            _isFinished = true;
        }

        public override void CleanUp()
        {
            Destroy(gameObject);
        }
    }

}