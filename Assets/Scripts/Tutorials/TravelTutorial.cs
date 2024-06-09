using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.IK;
using DG.Tweening;
using StudioByStorm.EventPublishers;
using StudioByStorm.ML.Clustering;

namespace StudioByStorm.Tutorials {

    public class TravelTutorial : Tutorial
    {
        private bool didPlayerTravel = false;
        private bool isLevelComplete = false;
        private ActionController actionController;
        private bool isJumpLocked;
        private Edge secondaryEdge;

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
            actionController = GameManager.Singleton.ControllerRegistry.TryGetValue(ViewName.ActionView) as ActionController;
            isJumpLocked = true;
            actionController.LockJump(isJumpLocked);
            StartCoroutine(CreateEdgeAnimation());
        }

        protected IEnumerator CreateEdgeAnimation()
        {
            Node currentNode = actionController.ActionModel.CurrentNode;
            int currentNodeID = currentNode.ID;

            List<int> nearbyNodeIDs = GameManager.Singleton.FullAdjacencyList.Get(currentNodeID);
            List<GameObject> nearbyNodes = new List<GameObject>();

            nearbyNodeIDs.ForEach(x => {
                Node currentNode = GameManager.Singleton.NodeRegistry.TryGetValue(x);
                if (currentNode.NodeColor == NodeColor.GrayScale) {
                    nearbyNodes.Add(currentNode.gameObject);
                } 
            });

            List<GameObject> nearestNodes = KNN.GetKNearestNeighbors(currentNode.gameObject, nearbyNodes, 1);
            GameObject nearestNode;
            Node nearbyNode;
            
            if (nearestNodes.Count > 0) {
                nearestNode = nearestNodes[0];
                nearbyNode = nearestNode.GetComponent<Node>();

                actionController.GetEdgeButtonClick();

                Edge currentEdge = actionController.ActionModel.CurrentEdge;
                FabrikSolver2D fabrikSolver2D = currentEdge.FabrikSolver2D;
                Transform emptyTarget = currentEdge.emptyTarget.transform;
                emptyTarget.transform.position = currentNode.gameObject.transform.position;
                fabrikSolver2D.GetChain(fabrikSolver2D.chainCount).target = emptyTarget;

                Vector2 targetPosition = (Vector2) nearestNode.transform.position;

                //emptyTarget.transform.DOMove(targetPosition, 1.0f).SetEase(Ease.InQuad);
                emptyTarget.transform.position = targetPosition;
                yield return null;

                actionController.ActionModel.CurrentNode = nearestNode.GetComponent<Node>();
                actionController.SetEdgeButtonClick();
                
                nearbyNode.LightColored.color = GameManager.Singleton.ColorModel.lightColor[(int) currentNode.NodeColor];
                nearbyNode.DarkColored.color = GameManager.Singleton.ColorModel.darkColor[(int) currentNode.NodeColor];
                nearbyNode.DisplayColor();

                actionController.ActionModel.CurrentNode = currentNode;

                //get the updated edge which gets created in "SetEdgeButtonClick"
                secondaryEdge = actionController.ActionModel.CurrentEdge;
                secondaryEdge.gameObject.SetActive(false);

                actionController.isJumpIndicatorOn = true;
            } else {
                _isAborting = true;
            }

            yield return null;
        }

        protected override IEnumerator TutorialUpdate()
        {
            while(isRunning) {
                yield return new WaitForSeconds(0.0333f);
            }
        }

        public override IEnumerator WaitUntilFinished()
        {
            yield return new WaitUntil(() => didPlayerTravel);
            isJumpLocked = false;
            actionController.LockJump(isJumpLocked);
            actionController.isJumpIndicatorOn = false;
            secondaryEdge.gameObject.SetActive(true);
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