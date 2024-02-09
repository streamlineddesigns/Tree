using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StudioByStorm.EventPublishers;

namespace StudioByStorm.Tutorials {

    public class JumpTutorial : Tutorial
    {
        private bool didPlayerJump = false;

        protected void OnEnable()
        {
            GameEventPublisher.OnPlayerJump += OnPlayerJump;
        }

        protected void OnDisable()
        {
            base.OnDisable();
            GameEventPublisher.OnPlayerJump -= OnPlayerJump;
        }

        protected void OnPlayerJump()
        {
            didPlayerJump = true;
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
            yield return new WaitUntil(() => didPlayerJump);
            _isFinished = true;
        }

        public override void CleanUp()
        {
            Destroy(gameObject);
        }
    }

}