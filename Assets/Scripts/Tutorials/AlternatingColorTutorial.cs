using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StudioByStorm.EventPublishers;

namespace StudioByStorm.Tutorials {

    public class AlternatingColorTutorial : Tutorial
    {
        private bool didPlayerNodeChange = false;

        protected void OnEnable()
        {
            GameEventPublisher.OnPlayerNodeChange += OnPlayerNodeChange;
        }

        protected void OnDisable()
        {
            base.OnDisable();
            GameEventPublisher.OnPlayerNodeChange -= OnPlayerNodeChange;
        }

        protected void OnPlayerNodeChange(int NodeID)
        {
            didPlayerNodeChange = true;
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
            yield return new WaitUntil(() => didPlayerNodeChange);
            _isFinished = true;
        }

        public override void CleanUp()
        {
            Destroy(gameObject);
        }
    }

}