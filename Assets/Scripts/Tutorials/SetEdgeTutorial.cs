using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StudioByStorm.Gravity.Player;

namespace StudioByStorm.Tutorials {

    public class SetEdgeTutorial : Tutorial
    {
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
                yield return new WaitForSeconds(0.0333f);
            }
        }

        public override IEnumerator WaitUntilFinished()
        {
            yield return new WaitUntil(() => GameManager.Singleton.LevelManager.currentLevelEdgeCount >= 1 || (actionController.ActionModel.CurrentEdge != null && actionController.ActionModel.CurrentEdge.gameObject.GetInstanceID() != startEdgeID));
            _isFinished = true;
        }

        public override void CleanUp()
        {
            Destroy(gameObject);
        }
    }

}