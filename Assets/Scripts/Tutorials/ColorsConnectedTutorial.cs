using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StudioByStorm.EventPublishers;

namespace StudioByStorm.Tutorials {

    public class ColorsConnectedTutorial : Tutorial
    {
        [SerializeField] private int[] startNodeIDToEndNodeIDForTutorials;
        [SerializeField] private bool[] isTutorialForNodeIDStarted;
        [SerializeField] private bool[] isTutorialForNodeIDAvailable;

        private ActionController actionController;
        private bool isLevelComplete = false;

        protected void OnEnable()
        {
            GameEventPublisher.OnStateChange += OnStateChange;
        }

        protected void OnDisable()
        {
            base.OnDisable();
            GameEventPublisher.OnStateChange -= OnStateChange;
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
            actionController = GameManager.Singleton.ControllerRegistry.TryGetValue(ViewName.ActionView) as ActionController;
        }

        protected override IEnumerator TutorialUpdate()
        {
            while(isRunning) {
                //for the swipe tutorial
                int nodeID = actionController.ActionModel.CurrentNode.ID;
                if (isTutorialForNodeIDAvailable[nodeID] && !isTutorialForNodeIDStarted[nodeID]) {
                    isTutorialForNodeIDStarted[nodeID] = true;
                    GameObject startGO = GameManager.Singleton.NodeRegistry.TryGetValue(nodeID).gameObject;
                    GameObject endGO = GameManager.Singleton.NodeRegistry.TryGetValue(startNodeIDToEndNodeIDForTutorials[nodeID]).gameObject;
                    GameManager.Singleton.FXManager.fingerSlingShotAnimation.gameObject.SetActive(true);
                    GameManager.Singleton.FXManager.fingerSlingShotAnimation.SetPositions(startGO, endGO);
                    GameManager.Singleton.FXManager.fingerSlingShotAnimation.Reset();
                    if (!GameManager.Singleton.FXManager.fingerSlingShotAnimation.isAnimating) {
                        GameManager.Singleton.FXManager.fingerSlingShotAnimation.Animate();
                    }
                }
                yield return new WaitForSeconds(0.0333f);
            }
        }

        public override IEnumerator WaitUntilFinished()
        {
            yield return new WaitUntil(() => isLevelComplete == true);
            _isFinished = true;
        }

        public override void CleanUp()
        {
            Destroy(gameObject);
            GameManager.Singleton.FXManager.fingerSlingShotAnimation.Stop();
            GameManager.Singleton.FXManager.fingerSlingShotAnimation.gameObject.SetActive(false);
        }
    }

}