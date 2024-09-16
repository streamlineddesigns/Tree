using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StudioByStorm.Gravity.Player;
using StudioByStorm.FX;

namespace StudioByStorm.Tutorials {

    public class SetEdgeTutorial : Tutorial
    {
        [SerializeField] private int[] startNodeIDToEndNodeIDForTutorials;
        [SerializeField] private bool[] isTutorialForNodeIDStarted;
        [SerializeField] private bool[] isTutorialForNodeIDAvailable;

        private ActionController actionController;
        private PlayerController playerController;
        private int startEdgeID = -1;

        public override void Init()
        {
            actionController = GameManager.Singleton.ControllerRegistry.TryGetValue(ViewName.ActionView) as ActionController;
            playerController = GameManager.Singleton.PlayerController;
        }

        protected override IEnumerator TutorialUpdate()
        {
            while(isRunning) {
                if (startEdgeID == -1) {
                    if (actionController.ActionModel.CurrentEdge != null) {
                        startEdgeID = actionController.ActionModel.CurrentEdge.gameObject.GetInstanceID();
                    }
                }

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
            yield return new WaitUntil(() => GameManager.Singleton.LevelManager.currentLevelEdgeCount >= 1);
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