using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.IK;
using DG.Tweening;
using StudioByStorm.EventPublishers;
using StudioByStorm.UI.Controllers;
using StudioByStorm.ML.Clustering;
using StudioByStorm.Optimizations;

namespace StudioByStorm.Tutorials {

    public class TravelTutorial : Tutorial
    {
        [SerializeField] private int[] startNodeIDToEndNodeIDForTutorials;
        [SerializeField] private bool[] isTutorialForNodeIDStarted;
        [SerializeField] private bool[] isTutorialForNodeIDAvailable;
        private GameObject startNode;
        private GameObject endNode;
        private bool didTeleportToSafeNode = false;
        
        private int travelCount;
        private bool didPlayerTravel = false;
        private bool isLevelComplete = false;
        private GameController gameController;
        private ActionController actionController;
        private bool isJumpLocked;
        private Edge secondaryEdge;
        private int targetNodeID;

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
            travelCount++;
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
            gameController = GameManager.Singleton.ControllerRegistry.TryGetValue(ViewName.GameView) as GameController;

            int nodeCount = GameManager.Singleton.LevelManager.currentLevelNodeCount;

            startNodeIDToEndNodeIDForTutorials = new int[nodeCount];
            isTutorialForNodeIDStarted = new bool[nodeCount];
            isTutorialForNodeIDAvailable = new bool[nodeCount];

            int startIndex = GameManager.Singleton.LevelManager.CurrentLevelData.safePath[0];
            int endIndex = GameManager.Singleton.LevelManager.CurrentLevelData.safePath[1];

            startNodeIDToEndNodeIDForTutorials[startIndex] = endIndex;
            isTutorialForNodeIDAvailable[startIndex] = true;

            isJumpLocked = true;
            actionController.LockJump(isJumpLocked);
            //"start position" for tutorial to occur
            startNode = GameManager.Singleton.NodeRegistry.TryGetValue(startIndex).gameObject;
            //"end position" where player travels to
            endNode = GameManager.Singleton.NodeRegistry.TryGetValue(endIndex).gameObject;

            StartCoroutine(CreateEdgeAnimation());
        }

        protected IEnumerator CreateEdgeAnimation()
        {
            //go to the safe node
            if (! didTeleportToSafeNode && startNode != null) {
                didTeleportToSafeNode = true;
                GameManager.Singleton.player.transform.DOMove(startNode.transform.position, 0.1f).OnComplete(() => {
                    GameManager.Singleton.player.transform.position = startNode.transform.position;
                    //reset every color
                    gameController.ResetBlueButtonClick();
                    gameController.ResetGreenButtonClick();
                    gameController.ResetPurpleButtonClick();
                    gameController.ResetWhiteButtonClick();
                    gameController.ResetOrangeButtonClick();
                    gameController.ResetYellowButtonClick();
                });
            }

            yield return new WaitForSeconds(0.25f);

            //prevent "floaty issue"
            if (didTeleportToSafeNode) {
                GameManager.Singleton.player.transform.position = startNode.transform.position;
            }

            Node currentNode = actionController.ActionModel.CurrentNode;
            int currentNodeID = currentNode.ID;
            Node targetNode = endNode.GetComponent<Node>();
            
            //the animation steps
            if (targetNode != null) {
                targetNodeID = targetNode.ID;

                actionController.GetEdgeButtonClick();

                Edge currentEdge = actionController.ActionModel.CurrentEdge;
                FabrikSolver2D fabrikSolver2D = currentEdge.FabrikSolver2D;
                Transform emptyTarget = currentEdge.emptyTarget.transform;
                emptyTarget.transform.position = currentNode.gameObject.transform.position;
                fabrikSolver2D.GetChain(fabrikSolver2D.chainCount).target = emptyTarget;

                Vector2 targetPosition = (Vector2) targetNode.transform.position;

                //emptyTarget.transform.DOMove(targetPosition, 1.0f).SetEase(Ease.InQuad);
                emptyTarget.transform.position = targetPosition;
                yield return null;

                actionController.ActionModel.CurrentNode = targetNode;
                actionController.SetEdgeButtonClick();
                
                targetNode.LightColored.color = GameManager.Singleton.ColorModel.lightColor[(int) currentNode.NodeColor];
                targetNode.DarkColored.color = GameManager.Singleton.ColorModel.darkColor[(int) currentNode.NodeColor];
                targetNode.DisplayColor();

                actionController.ActionModel.CurrentNode = currentNode;

                //get the updated edge which gets created in "SetEdgeButtonClick"
                secondaryEdge = actionController.ActionModel.CurrentEdge;
                if (secondaryEdge != null) secondaryEdge.gameObject.SetActive(false);

                actionController.isJumpIndicatorOn = true;
            } else {
                _isAborting = true;
            }

            yield return null;
        }

        protected override IEnumerator TutorialUpdate()
        {
            while(isRunning) {
                //for the swipe tutorial
                int nodeID = actionController.ActionModel.CurrentNode.ID;
                if (isTutorialForNodeIDAvailable[nodeID] && !isTutorialForNodeIDStarted[nodeID]) {
                    isTutorialForNodeIDStarted[nodeID] = true;
                    GameManager.Singleton.FXManager.fingerSlingShotAnimation.Stop();
                    GameManager.Singleton.FXManager.fingerSlingShotAnimation.gameObject.SetActive(false);
                    GameObject startGO = GameManager.Singleton.NodeRegistry.TryGetValue(nodeID).gameObject;
                    GameObject endGO = GameManager.Singleton.NodeRegistry.TryGetValue(startNodeIDToEndNodeIDForTutorials[nodeID]).gameObject;
                    GameManager.Singleton.FXManager.fingerSlingShotAnimation.SetPositions(startGO, endGO);

                    if (!GameManager.Singleton.FXManager.fingerSlingShotAnimation.isAnimating) {
                        GameManager.Singleton.FXManager.fingerSlingShotAnimation.gameObject.SetActive(true);
                        GameManager.Singleton.FXManager.fingerSlingShotAnimation.Animate();
                    }
                }

                yield return new WaitForSeconds(0.0333f);
            }
        }

        protected void Update()
        {
            if (Input.GetMouseButton(0)) {
                Vector2 clickPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2 cellID = GameManager.Singleton.SpatialHashManager.SpatialHash.GetCellIDForPos(clickPos);
                List<HashData> nearbyNodes = GameManager.Singleton.SpatialHashManager.SpatialHash.GetNearby(cellID);
                List<HashData> nearestNeighbor = KNN.GetKNearestNeighbors(clickPos, nearbyNodes, 1);
                HashData nearbyNode = (nearestNeighbor.Count > 0 && nearestNeighbor[0] != null) ? nearestNeighbor[0] : null;
                Node selectedNode = nearbyNode.GetData<Node>();
                bool isNearbyNodeWithinClickDistance = (nearbyNode != null && selectedNode != null && selectedNode.ID == targetNodeID && ML.Math.GetDistance(nearbyNode.gameObject.transform.position, clickPos) <= 3.0f);

                if (isNearbyNodeWithinClickDistance) {
                    isJumpLocked = false;
                    actionController.LockJump(isJumpLocked);
                } else {
                    isJumpLocked = true;
                    actionController.LockJump(isJumpLocked);
                }
            }
        }

        public override IEnumerator WaitUntilFinished()
        {
            yield return new WaitUntil(() => didPlayerTravel);
            TravelCleanUp();
            _isFinished = true;
        }

        public override void CleanUp()
        {
            TravelCleanUp();

            GameManager.Singleton.FXManager.fingerSlingShotAnimation.Stop();
            GameManager.Singleton.FXManager.fingerSlingShotAnimation.gameObject.SetActive(false);

            Destroy(gameObject);
        }

        private void TravelCleanUp()
        {
            isJumpLocked = false;
            actionController.LockJump(isJumpLocked);
            actionController.isJumpIndicatorOn = false;

            if (secondaryEdge != null) secondaryEdge.gameObject.SetActive(true);
        }
    }

}