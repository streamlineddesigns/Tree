using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StudioByStorm.EventPublishers;
using DG.Tweening;

namespace StudioByStorm.UI.Controllers {

    public class GameController : Controller
    {
        [SerializeField] private GameObject[] ResetButtons;
        private GameObject ActiveResetButton;
        public GameObject Centroid;
        protected bool isZooming = false;

        void OnEnable()
        {
            base.OnEnable();
            GameEventPublisher.OnStateChange      += OnStateChange;
            GameEventPublisher.OnPlayerNodeChange += OnPlayerNodeChange;
        }

        void OnDisable()
        {
            base.OnDisable();
            GameEventPublisher.OnStateChange      -= OnStateChange;
            GameEventPublisher.OnPlayerNodeChange -= OnPlayerNodeChange;
        }

        protected void OnPlayerNodeChange(int nodeID)
        {
            StartCoroutine(DelayedOnPlayerNodeChange(nodeID));
        }

        IEnumerator DelayedOnPlayerNodeChange(int nodeID)
        {
            //this is to wait for any connections to be made
            yield return null;
            yield return null;
            yield return null;

            //need the current node
            Node currentNode = GameManager.Singleton.NodeRegistry.TryGetValue(nodeID);
            //need to check if that color has a completed path
            bool isNodePathCompleted = (GameManager.Singleton.LevelManager.parentColorsConnected.ContainsKey(currentNode.NodeColor) && GameManager.Singleton.LevelManager.parentColorsConnected[currentNode.NodeColor]);

            //fetch the action controller
            ActionController actionController = GameManager.Singleton.ControllerRegistry.TryGetValue(ViewName.ActionView) as ActionController;

            //cache the color connection count
            int colorIndex = (int) currentNode.NodeColor;
            int colorConnectionCount = actionController.ActionModel.ColorConnectionsCount[colorIndex];

            //hide the active reset button by default
            if (ActiveResetButton != null) ActiveResetButton.SetActive(false);
            
            //need to check if a current edge exists & the node path is completed & if its color is different than the current nodes color
            if (actionController.ActionModel.CurrentEdge != null && isNodePathCompleted && actionController.ActionModel.CurrentEdge.EdgeColor != currentNode.NodeColor) {
                /*show the reset button for the color that is the current node color*/
                int nodeColorIndex = (int) currentNode.NodeColor;
                ActiveResetButton = ResetButtons[nodeColorIndex];
                ActiveResetButton.SetActive(true);
                //Debug.LogError(1);

            //if there is an edge & its the same as the node & theres no connections
            } else if (actionController.ActionModel.CurrentEdge != null && colorConnectionCount <= 0 && actionController.ActionModel.CurrentEdge.EdgeColor == currentNode.NodeColor) {
                //do nothing here
                //Debug.LogError(2);

            //if there is an edge at least
            } else if (actionController.ActionModel.CurrentEdge != null) {
                /*show the reset button for the color that is the current edge color*/
                int edgeColorIndex = (int) actionController.ActionModel.CurrentEdge.EdgeColor;
                ActiveResetButton = ResetButtons[edgeColorIndex];
                ActiveResetButton.SetActive(true);
                //Debug.LogError(3);

            //might as well show the node color then
            } else if (colorConnectionCount >= 1) {
                /*show the reset button for the color that is the current node color*/
                int nodeColorIndex = (int) currentNode.NodeColor;
                ActiveResetButton = ResetButtons[nodeColorIndex];
                ActiveResetButton.SetActive(true);
                //Debug.LogError(4);
            }
        }

        public void PauseButtonClick()
        {
            AudioManager.Singleton.Play(SoundType.ButtonPress);
            Time.timeScale = 0.0f;
            GameManager.Singleton.UIController.ShowView(ViewName.PauseView);
        }

        public void ZoomButtonClick(float speed = 1.0f)
        {
            AudioManager.Singleton.Play(SoundType.ButtonPress);
            CameraController.centroid = Centroid.transform;
            GameManager.Singleton.CameraController.MoveToCentroid();
            GameManager.Singleton.CameraController.GetComponent<Camera>().DOOrthoSize(45, speed).SetEase(Ease.InQuad);
            GameManager.Singleton.UIController.ShowView(ViewName.ZoomView);
        }

        public void OnStateChange(GameState state)
        {
            switch(state) {
                case GameState.GameStart :
                    GameStart();
                    break;

                case GameState.LevelComplete :
                    break;

                case GameState.LevelExited :
                    LevelExited();
                    break;
            }
        }

        public void ResetBlueButtonClick()
        {
            ResetButtonClick(NodeColor.Blue);
        }

        public void ResetGreenButtonClick()
        {
            ResetButtonClick(NodeColor.Green);
        }

        public void ResetPurpleButtonClick()
        {
            ResetButtonClick(NodeColor.Purple);
        }

        public void ResetWhiteButtonClick()
        {
            ResetButtonClick(NodeColor.White);
        }

        public void ResetOrangeButtonClick()
        {
            ResetButtonClick(NodeColor.Orange);
        }

        public void ResetYellowButtonClick()
        {
            ResetButtonClick(NodeColor.Yellow);
        }

        protected void ResetButtonClick(NodeColor inputColor)
        {
            //if (ActiveResetButton != null) ActiveResetButton.SetActive(false);

            ActionController ActionController = GameManager.Singleton.ControllerRegistry.TryGetValue(ViewName.ActionView) as ActionController;
            Edge activeEdgeDuringReset = ActionController.ActionModel.CurrentEdge;

            if ((activeEdgeDuringReset != null && activeEdgeDuringReset.EdgeColor == inputColor) || GameManager.Singleton.ColorEdgeRegistry.TryGetValue(inputColor) != null) {

            } else {
                return;
            }

            AudioManager.Singleton.Play(SoundType.Scissors);
            //Get all the blue nodes
            List<Node> blueNodes = GameManager.Singleton.ColorNodeRegistry.TryGetValue(inputColor);
            //Get all the blue edges
            List<Edge> blueEdges = GameManager.Singleton.ColorEdgeRegistry.TryGetValue(inputColor);
            
            
            //remove nodes and edges from registries
            GameManager.Singleton.ColorNodeRegistry.Remove(inputColor);
            GameManager.Singleton.ColorEdgeRegistry.Remove(inputColor);            

            //when someone resets a color, & they have an edge in use
            //we need to know if the nearby node is a parent. If so, enable the get edge button again!
            //this is an edge case; no pun intended lol
            bool isEdgeCase = false;
            Node nearbyNode = GameManager.Singleton.nearbyNode.GetData<Node>();
            if (activeEdgeDuringReset != null && nearbyNode.NodeType == NodeType.Parent && !GameManager.Singleton.LevelManager.parentColorsConnected[nearbyNode.NodeColor] && Vector3.Distance(nearbyNode.gameObject.transform.position, GameManager.Singleton.player.transform.position) <= 0.25f) {
                isEdgeCase = true;
                ActionController.ActionView.EnableGetEdgeButton();
            }

            if (ActionController.ActionModel.CurrentEdge != null && ActionController.ActionModel.CurrentEdge.EdgeColor == inputColor) {
                ActionController.ActionModel.CurrentEdge.gameObject.SetActive(false);
                ActionController.ActionModel.CurrentEdge.parentNode.NumOfConnections--;
                ActionController.ActionModel.CurrentEdge = null;
                if (!isEdgeCase) {
                    GameEventPublisher.PublishPlayerEdgeChange(-1);
                    ActionController.ActionView.DisableSetEdgeButton();
                } else {
                    //GameEventPublisher.PublishPlayerEdgeChange(nearbyNode.ID);
                }
            }
            int colorIndex = (int) inputColor;
            ActionController.ActionModel.ColorConnectionsCount[colorIndex] = 0;
            

            //reset tracked connected parents
            GameManager.Singleton.LevelManager.parentColorsConnected[inputColor] = false;

            //Set the nodes back to their original states??? just disable and enable??
            if (blueNodes != null) {
                for (int i = 0; i < blueNodes.Count; i++) {
                    blueNodes[i].gameObject.SetActive(false);
                    blueNodes[i].gameObject.SetActive(true);
                    blueNodes[i].ResetToFactorySettings();
                }
            }
            //make sure the tracked edge count goes back down 
            if (blueEdges != null) {
                GameManager.Singleton.LevelManager.currentLevelEdgeCount -= blueEdges.Count;
                for (int i = 0; i < blueEdges.Count; i++) {
                    blueEdges[i].gameObject.SetActive(false);
                }
            }

            //fetch the action controller
            ActionController actionController = GameManager.Singleton.ControllerRegistry.TryGetValue(ViewName.ActionView) as ActionController;
            OnPlayerNodeChange(actionController.ActionModel.CurrentNode.ID);
            
        }
        
        protected void GameStart()
        {
            Centroid.transform.position = GameManager.Singleton.LevelManager.CurrentLevelData.Centroid;
        }

        protected void LevelExited()
        {
            ResetBlueButtonClick();
            ResetGreenButtonClick();
            ResetPurpleButtonClick();
            ResetWhiteButtonClick();
            ResetOrangeButtonClick();
            ResetYellowButtonClick();
        }
    }

}