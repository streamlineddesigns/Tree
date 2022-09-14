using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using StudioByStorm.UI;
using StudioByStorm.ML;

namespace StudioByStorm {

    public class ActionController : MonoBehaviour
    {
        public float EdgeDistanceThreshold;
        public ActionView ActionView;
        public ActionModel ActionModel;
        public bool isTravelButtonClicked;
        public float thresholdDistanceToBreakOutOfLerp = 0.1f;

        public void TravelEdgeButtonDrag()
        {
            if (! isTravelButtonClicked || ! ActionView.TravelEdgeButton.interactable) {
                return;
            }
            float targetX = DOVirtual.EasedValue(GameManager.Singleton.PlayerController.GetSwipeRecognizer().startPoint.x, GameManager.Singleton.PlayerController.GetSwipeRecognizer().endPoint.x, 0.5f, Ease.Linear);
            float targetY = DOVirtual.EasedValue(GameManager.Singleton.PlayerController.GetSwipeRecognizer().startPoint.y, GameManager.Singleton.PlayerController.GetSwipeRecognizer().endPoint.y, 0.5f, Ease.Linear);
            ActionView.TravelButtonImage.transform.position = new Vector2(targetX, targetY);
        }

        public void TravelEdgeButtonDragEnd()
        {
            if (! isTravelButtonClicked || ! ActionView.TravelEdgeButton.interactable) {
                return;
            }
            ActionView.TravelButtonImage.transform.position = ActionView.DragTravelButtonImage.transform.position;
        }

        void OnTriggerEnter2D(Collider2D obj)
        {
            if (obj.TryGetComponent<Node>(out Node Node)) {
                ActionModel.CurrentNode = Node;
                EnableActionButtons();
            }
        }

        void OnTriggerExit2D(Collider2D obj)
        {
            if (obj.TryGetComponent<Node>(out Node Node)) {
                ActionView.DisableGetEdgeButton();
                ActionView.DisableSetEdgeButton();
                ActionView.DisableTravelButton();
            }
        }

        void EnableActionButtons()
        {
            int colorIndex = (int) ActionModel.CurrentNode.NodeColor;
            //$$Experimental: inifinite edge tests
            if (/*(ActionModel.ColorConnectionsCount[colorIndex] < ActionModel.colorMaxConnections[colorIndex]) && */ActionModel.CurrentEdge == null && (ActionModel.CurrentNode.NodeType == NodeType.Parent && ActionModel.CurrentNode.NumOfConnections == 0 || ActionModel.CurrentNode.NodeType == NodeType.Child && ActionModel.CurrentNode.NumOfConnections == 1))  {
                ActionView.EnableGetEdgeButton();
            } else {
                ActionView.DisableGetEdgeButton();
            }

            
            if ( ActionModel.CurrentEdge != null && (ActionModel.CurrentEdge.parentNode.NodeType != NodeType.Parent || ActionModel.CurrentNode.NodeType != NodeType.Parent) && ActionModel.CurrentEdge.parentID != ActionModel.CurrentNode.ID && ML.Math.GetDistance(ActionModel.CurrentEdge.parentNode.gameObject.transform.position, ActionModel.CurrentNode.gameObject.transform.position) < EdgeDistanceThreshold && (ActionModel.CurrentNode.NodeType == NodeType.Parent && ActionModel.CurrentEdge.EdgeColor == ActionModel.CurrentNode.NodeColor && ActionModel.CurrentNode.NumOfConnections == 0 || (ActionModel.CurrentNode.NodeType == NodeType.Child || ActionModel.CurrentNode.NodeType == NodeType.Disjoint) && ActionModel.CurrentNode.NumOfConnections <= 1)) {
                ActionView.EnableSetEdgeButton();
            } else {
                ActionView.DisableSetEdgeButton();
            }

            if ( (ActionModel.CurrentEdge != null && ActionModel.CurrentEdge.EdgeColor == ActionModel.CurrentNode.NodeColor && ActionModel.CurrentEdge.parentID != ActionModel.CurrentNode.ID && ActionModel.CurrentNode.NumOfConnections > 0) || (ActionModel.CurrentEdge != null && ActionModel.CurrentEdge.EdgeColor == ActionModel.CurrentNode.NodeColor && ActionModel.CurrentNode.NumOfConnections > 1)  || (ActionModel.CurrentEdge == null && ActionModel.CurrentNode.NumOfConnections > 0) ){
                ActionView.EnableTravelButton();
            } else {
                ActionView.DisableTravelButton();
            }
        }

        public void TravelEdgeButtonClick()
        {
            isTravelButtonClicked = true;
            ActionView.TravelButtonClick();
            //GameManager.Singleton.PlayerController.LockMovement(true);
        }


        public void TravelEdgeButtonDeselect()
        {
            if (! isTravelButtonClicked) {
                return;
            }

            isTravelButtonClicked = false;
            ActionView.TravelButtonImage.transform.position = ActionView.DragTravelButtonImage.transform.position;
            //get the current nodes neighbors
            List<int> adjacentNodes = GameManager.Singleton.AdjacencyList.Get(ActionModel.CurrentNode.ID);
            int targetIndex = -1;
            float minDissimiliarity = 1000000.0f;
            //calculate the swipe direction
            Vector2 swipeDir = GameManager.Singleton.PlayerController.GetSwipeRecognizer().endPoint - GameManager.Singleton.PlayerController.GetSwipeRecognizer().startPoint;
            for (int i = 0; i < adjacentNodes.Count; i++) {
                //calculate the direction to the currentnode
                Vector2 currentnodeDir = GameManager.Singleton.NodeRegistry.TryGetValue(adjacentNodes[i]).gameObject.transform.position - ActionModel.CurrentNode.gameObject.transform.position;
                float currentDissimilarity = ML.Math.GetDistance(currentnodeDir, swipeDir);
                //find the node whose direction from the current node has the least dissimilarity to the swipe direction
                if (currentDissimilarity < minDissimiliarity) {
                    targetIndex = i;
                    minDissimiliarity = currentDissimilarity;
                }
            }
            //once that is complete, we need to now determine which edge it is that connects them
            Edge edge = (GameManager.Singleton.EdgeRegistry.TryGetValue(ActionModel.CurrentNode.ID).childID == GameManager.Singleton.NodeRegistry.TryGetValue(adjacentNodes[targetIndex]).ID) ? GameManager.Singleton.EdgeRegistry.TryGetValue(ActionModel.CurrentNode.ID) : GameManager.Singleton.EdgeRegistry.TryGetValue(adjacentNodes[targetIndex]);
            //send player along edges path
            GameManager.Singleton.PlayerController.DoPathMovement(edge.LinkSpriteRenderers.Select(x => x.gameObject.transform.position).ToArray());
            //GameManager.Singleton.player.transform.position = GameManager.Singleton.NodeRegistry.TryGetValue(adjacentNodes[targetIndex]).gameObject.transform.position;
        }

        public void GetEdgeButtonClick()
        {
            ActionModel.CurrentNode.NumOfConnections++;
            ActionView.DisableGetEdgeButton();
            ActionView.GetEdgeButtonClick();
        }

        public void SetEdgeButtonClick()
        {
            ActionView.DisableSetEdgeButton();
            GameManager.Singleton.AdjacencyList.Add(ActionModel.CurrentEdge.parentID, ActionModel.CurrentNode.ID);
            GameManager.Singleton.AdjacencyList.Add(ActionModel.CurrentNode.ID, ActionModel.CurrentEdge.parentID);
            GameManager.Singleton.AdjacencyList.Log();

            //if the current node was disjoint, then make it a child, otherwise, leave it whatever it was
            ActionModel.CurrentNode.NodeType = (ActionModel.CurrentNode.NodeType == NodeType.Disjoint) ? NodeType.Child : ActionModel.CurrentNode.NodeType;
            //make the current nodes color the same as the current edges parent nodes color
            ActionModel.CurrentNode.NodeColor = ActionModel.CurrentEdge.parentNode.NodeColor;
            ActionModel.CurrentNode.DisplayColor();
            ActionModel.CurrentNode.LightColored.color = GameManager.Singleton.ColorModel.lightColor[(int) ActionModel.CurrentNode.NodeColor];
            ActionModel.CurrentNode.DarkColored.color = GameManager.Singleton.ColorModel.darkColor[(int) ActionModel.CurrentNode.NodeColor];
            ActionModel.CurrentNode.NumOfConnections++;
            ActionModel.CurrentEdge.childID = ActionModel.CurrentNode.ID;
            ActionModel.CurrentEdge.turnFabrikOff();

            GameManager.Singleton.ColorNodeRegistry.Add(ActionModel.CurrentNode.NodeColor, GameManager.Singleton.NodeRegistry.TryGetValue(ActionModel.CurrentEdge.parentID));
            GameManager.Singleton.ColorNodeRegistry.Add(ActionModel.CurrentNode.NodeColor, GameManager.Singleton.NodeRegistry.TryGetValue(ActionModel.CurrentEdge.childID));
            GameManager.Singleton.ColorEdgeRegistry.Add(ActionModel.CurrentNode.NodeColor, ActionModel.CurrentEdge);


            ActionView.SetEdgeButtonClick(ActionModel.CurrentEdge);
            

            int colorIndex = (int) ActionModel.CurrentNode.NodeColor;
            ActionModel.ColorConnectionsCount[colorIndex]++;

            //if (ActionModel.ColorConnectionsCount[colorIndex] < ActionModel.colorMaxConnections[colorIndex]) {
            if (GameManager.Singleton.ColorNodeRegistry.TryGetValue(ActionModel.CurrentNode.NodeColor).Where(x => x.NodeType == NodeType.Parent).ToArray().Length < 2) {
                //get new edge
                GetEdgeButtonClick();
            }
            //}$$Experimental: inifinite edge test

            if ( (ActionModel.CurrentEdge != null && ActionModel.CurrentEdge.parentID != ActionModel.CurrentNode.ID && ActionModel.CurrentEdge.EdgeColor == ActionModel.CurrentNode.NodeColor) || (ActionModel.CurrentEdge != null && ActionModel.CurrentEdge.EdgeColor == ActionModel.CurrentNode.NodeColor && ActionModel.CurrentNode.NumOfConnections > 1)  || (ActionModel.CurrentEdge == null && ActionModel.CurrentNode.NumOfConnections > 0) ){
                ActionView.EnableTravelButton();
            }

            if (ActionModel.colorMaxConnections.Sum() == ActionModel.ColorConnectionsCount.Sum()) {
                GameManager.Singleton.LevelComplete();
            }
        }
    }

}