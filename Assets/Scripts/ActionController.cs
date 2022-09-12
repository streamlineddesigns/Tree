using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StudioByStorm.UI;
using StudioByStorm.ML;

namespace StudioByStorm {

    public class ActionController : MonoBehaviour
    {
        public float EdgeDistanceThreshold;
        public ActionView ActionView;
        public ActionModel ActionModel;
        
        public void TravelEdgeButtonDrag()
        {
            Debug.Log("test");
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
            if ((ActionModel.ColorConnectionsCount[colorIndex] < ActionModel.colorMaxConnections[colorIndex]) && ActionModel.CurrentEdge == null && (ActionModel.CurrentNode.NodeType == NodeType.Parent && ActionModel.CurrentNode.NumOfConnections == 0 || ActionModel.CurrentNode.NodeType == NodeType.Child && ActionModel.CurrentNode.NumOfConnections == 1))  {
                ActionView.EnableGetEdgeButton();
            } else {
                ActionView.DisableGetEdgeButton();
            }

            
            if (ActionModel.CurrentEdge != null && ActionModel.CurrentEdge.parentID != ActionModel.CurrentNode.ID && ML.Math.GetDistance(ActionModel.CurrentEdge.parentNode.gameObject.transform.position, ActionModel.CurrentNode.gameObject.transform.position) < EdgeDistanceThreshold && (ActionModel.CurrentNode.NodeType == NodeType.Parent && ActionModel.CurrentEdge.EdgeColor == ActionModel.CurrentNode.NodeColor && ActionModel.CurrentNode.NumOfConnections == 0 || (ActionModel.CurrentNode.NodeType == NodeType.Child || ActionModel.CurrentNode.NodeType == NodeType.Disjoint) && ActionModel.CurrentNode.NumOfConnections <= 1)) {
                ActionView.EnableSetEdgeButton();
            } else {
                ActionView.DisableSetEdgeButton();
            }

            if ( (ActionModel.CurrentEdge != null && ActionModel.CurrentEdge.parentID != ActionModel.CurrentNode.ID && ActionModel.CurrentNode.NumOfConnections > 0) || (ActionModel.CurrentEdge != null && ActionModel.CurrentNode.NumOfConnections > 1)  || (ActionModel.CurrentEdge == null && ActionModel.CurrentNode.NumOfConnections > 0) ){
                ActionView.EnableTravelButton();
            } else {
                ActionView.DisableTravelButton();
            }
        }

        public void TravelEdgeButtonClick()
        {
            ActionView.TravelButtonClick();
            Vector2 travelDir = GameManager.Singleton.PlayerController.GetSwipeDirection();
            GameManager.Singleton.PlayerController.LockMovement(true);
            
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

            //if the current node was disjoint, then make it a child, otherwise, leave it whatever it was
            ActionModel.CurrentNode.NodeType = (ActionModel.CurrentNode.NodeType == NodeType.Disjoint) ? NodeType.Child : ActionModel.CurrentNode.NodeType;
            //make the current nodes color the same as the current edges parent nodes color
            ActionModel.CurrentNode.NodeColor = ActionModel.CurrentEdge.parentNode.NodeColor;
            ActionModel.CurrentNode.DisplayColor();
            ActionModel.CurrentNode.LightColored.color = GameManager.Singleton.ColorModel.lightColor[(int) ActionModel.CurrentNode.NodeColor];
            ActionModel.CurrentNode.DarkColored.color = GameManager.Singleton.ColorModel.darkColor[(int) ActionModel.CurrentNode.NodeColor];
            ActionModel.CurrentNode.NumOfConnections++;
            ActionModel.CurrentEdge.childID = ActionModel.CurrentNode.ID;
            ActionView.SetEdgeButtonClick(ActionModel.CurrentEdge);
            

            int colorIndex = (int) ActionModel.CurrentNode.NodeColor;
            ActionModel.ColorConnectionsCount[colorIndex]++;

            if (ActionModel.ColorConnectionsCount[colorIndex] < ActionModel.colorMaxConnections[colorIndex]) {
                //get new edge
                GetEdgeButtonClick();
            }

            if ( (ActionModel.CurrentEdge != null && ActionModel.CurrentEdge.parentID != ActionModel.CurrentNode.ID) || (ActionModel.CurrentEdge != null && ActionModel.CurrentNode.NumOfConnections > 1)  || (ActionModel.CurrentEdge == null && ActionModel.CurrentNode.NumOfConnections > 0) ){
                ActionView.EnableTravelButton();
            }
            
        }
    }

}