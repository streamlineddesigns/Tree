using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StudioByStorm.Tutorials {

    public class GetEdgeTutorial : Tutorial
    {
        private ActionController actionController;

        public override void Init()
        {
            actionController = GameManager.Singleton.ControllerRegistry.TryGetValue(ViewName.ActionView) as ActionController;
        }

        protected override IEnumerator TutorialUpdate()
        {
            while(isRunning) {
                yield return new WaitForSeconds(0.0333f);
            }
        }

        public override IEnumerator WaitUntilFinished()
        {
            yield return new WaitUntil(() => actionController.ActionModel.CurrentEdge != null);
            _isFinished = true;
        }

        public override void CleanUp()
        {
            Destroy(gameObject);
        }
    }

}