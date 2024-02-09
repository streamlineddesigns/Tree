using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StudioByStorm.EventPublishers;

namespace StudioByStorm.Tutorials {

    public class TravelTutorial : Tutorial
    {
        private bool didPlayerTravel = false;
        private bool isLevelComplete = false;

        protected void OnEnable()
        {
            GameEventPublisher.OnPlayerTravel += OnPlayerTravel;
            GameEventPublisher.OnStateChange  += OnStateChange;
        }

        protected void OnDisable()
        {
            base.OnDisable();
            GameEventPublisher.OnPlayerTravel -= OnPlayerTravel;
            GameEventPublisher.OnStateChange  -= OnStateChange;
        }

        protected void OnPlayerTravel()
        {
            didPlayerTravel = true;
        }

        protected void OnStateChange(GameState state)
        {
            switch(state) {
                case GameState.LevelComplete :
                    isLevelComplete = true;
                    break;
            }
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
            yield return new WaitUntil(() => didPlayerTravel || isLevelComplete);
            _isFinished = true;
        }

        public override void CleanUp()
        {
            Destroy(gameObject);
        }
    }

}