using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StudioByStorm.Gravity.Player;

namespace StudioByStorm.Tutorials {

    public class GetEdgeTutorial : Tutorial
    {
        private ActionController actionController;
        private PlayerController playerController;
        private bool isJumpLocked;

        public override void Init()
        {
            actionController = GameManager.Singleton.ControllerRegistry.TryGetValue(ViewName.ActionView) as ActionController;
            playerController = GameManager.Singleton.PlayerController;

            isJumpLocked = true;
            actionController.LockJump(isJumpLocked);
        }

        protected override IEnumerator TutorialUpdate()
        {
            while(isRunning) {
                if (isJumpLocked && playerController.JumpIndicator.activeSelf) playerController.JumpIndicator.SetActive(false);
                yield return new WaitForSeconds(0.0333f);
            }
        }

        public override IEnumerator WaitUntilFinished()
        {
            yield return new WaitUntil(() => actionController.ActionModel.CurrentEdge != null);
            isJumpLocked = false;
            actionController.LockJump(isJumpLocked);
            _isFinished = true;
        }

        public override void CleanUp()
        {
            isJumpLocked = false;
            actionController.LockJump(isJumpLocked);
            Destroy(gameObject);
        }
    }

}