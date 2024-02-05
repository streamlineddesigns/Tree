using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StudioByStorm.EventPublishers;
using DG.Tweening;

namespace StudioByStorm.UI.Controllers {

    public class GameController : Controller
    {
        public GameObject Centroid;
        protected bool isZooming = false;

        void OnEnable()
        {
            base.OnEnable();
            GameEventPublisher.OnStateChange += OnStateChange;
        }

        void OnDisable()
        {
            base.OnDisable();
            GameEventPublisher.OnStateChange -= OnStateChange;
        }

        public void PauseButtonClick()
        {
            Time.timeScale = 0.0f;
            GameManager.Singleton.UIController.ShowView(ViewName.PauseView);
        }

        public void ZoomButtonClick(float speed = 1.0f)
        {
            CameraController.centroid = Centroid.transform;
            GameManager.Singleton.CameraController.MoveToCentroid();
            GameManager.Singleton.CameraController.GetComponent<Camera>().DOOrthoSize(20, speed).SetEase(Ease.InQuad);
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

        protected void ResetButtonClick(NodeColor inputColor)
        {
            //Get all the blue nodes
            List<Node> blueNodes = GameManager.Singleton.ColorNodeRegistry.TryGetValue(inputColor);
            //Get all the blue edges
            List<Edge> blueEdges = GameManager.Singleton.ColorEdgeRegistry.TryGetValue(inputColor);
            
            
            //remove nodes and edges from registries
            GameManager.Singleton.ColorNodeRegistry.Remove(inputColor);
            GameManager.Singleton.ColorEdgeRegistry.Remove(inputColor);

            //reset action controller state
            ActionController ActionController = GameManager.Singleton.ControllerRegistry.TryGetValue(ViewName.ActionView) as ActionController;
            if (ActionController.ActionModel.CurrentEdge != null && ActionController.ActionModel.CurrentEdge.EdgeColor == inputColor) {
                ActionController.ActionModel.CurrentEdge.gameObject.SetActive(false);
                ActionController.ActionModel.CurrentEdge.parentNode.NumOfConnections--;
                ActionController.ActionModel.CurrentEdge = null;
                ActionController.ActionView.DisableSetEdgeButton();
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
                }
            }
            //make sure the tracked edge count goes back down 
            if (blueEdges != null) {
                GameManager.Singleton.LevelManager.currentLevelEdgeCount -= blueEdges.Count;
                for (int i = 0; i < blueEdges.Count; i++) {
                    blueEdges[i].gameObject.SetActive(false);
                }
            }

            //when someone resets the current color, check if they are on a node of the same color and it's a parent. If so, enable the get edge button again!
            //this is an edge case; no pun intended lol
            Node nearbyNode = GameManager.Singleton.nearbyNode.GetData<Node>();
            if (nearbyNode.NodeColor == inputColor && nearbyNode.NodeType == NodeType.Parent && Vector3.Distance(nearbyNode.gameObject.transform.position, GameManager.Singleton.player.transform.position) <= 0.25f) {
                ActionView actionView = GameManager.Singleton.ViewRegistry.TryGetValue(ViewName.ActionView) as ActionView;
                actionView.EnableGetEdgeButton();
            }
            
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
        }
    }

}