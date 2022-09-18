using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using StudioByStorm.UI;
using StudioByStorm.ML;
using StudioByStorm.EventPublishers;

namespace StudioByStorm {

    public class ActionController : MonoBehaviour
    {
        public float EdgeDistanceThreshold;
        public ActionView ActionView;
        public ActionModel ActionModel;
        public bool isTravelButtonClicked;
        protected bool isTravelAvailable;
        public float thresholdDistanceToBreakOutOfLerp = 0.1f;
        public bool lerping;
        public bool isUp;
        public Vector2 DownPoint;
        public Vector2 UpPoint;

        void Update()
        {
            
        }
        
        public void OnDown()
        {
            isUp = false;
           DownPoint = Input.mousePosition;
        }

        public void OnUp()
        {
            isUp = true;
            UpPoint = Input.mousePosition;

            OnTravelJoystickDirectionChange(Vector2.zero);
        }
        
        void OnTravelJoystickDirectionChange(Vector2 Direction)
        {
            if (!isUp) {
                return;
            }
            //check the direction of the joystick
            Vector2 joystickDir = UpPoint - DownPoint;
            //get a list of the ids the current node is connected to from the adjacency list
            List<int> adjacentNodeIDS = GameManager.Singleton.AdjacencyList.Get(ActionModel.CurrentNode.ID);
            if (lerping || adjacentNodeIDS == null) {
                return;
            }
            //calculate the directions from the current node to the connected node
            //do a 1KNN on the list and the directions of the joystick direction
            int index = -1;
            float minimumDistance = 1000.0f;
            Node targetNode = null;

            for (int i = 0; i < adjacentNodeIDS.Count; i++) {
                int nodeID = adjacentNodeIDS[i];
                Node connectedNode = GameManager.Singleton.NodeRegistry.TryGetValue(nodeID);
                Vector2 directionToConnectedNode = connectedNode.gameObject.transform.position - ActionModel.CurrentNode.gameObject.transform.position;

                float connectedNodeDistance = ML.Math.GetDistance(joystickDir, directionToConnectedNode);

                if (connectedNodeDistance < minimumDistance) {
                    minimumDistance = connectedNodeDistance;
                    index = nodeID;
                    targetNode = connectedNode;
                }
            }

            //now we need to find the list of edges which are connected to current and target
            if (index != -1) {
                Edge currentEdge = ActionModel.CurrentNode.currentEdge;
                Edge targetNodeEdge = targetNode.currentEdge;

                //which edge connects both together?
                if (targetNodeEdge.childID == ActionModel.CurrentNode.ID) {
                    StartCoroutine(Lerp(targetNodeEdge));
                } else if (currentEdge.childID == targetNode.ID) {
                    StartCoroutine(Lerp(currentEdge));

                }
            }
            
        }

        IEnumerator Lerp(Edge edge)
        {
            Debug.Log("Lerping");
            lerping = true;
            Vector3[] waypoints = edge.LinkSpriteRenderers.Select(x => x.gameObject.transform.position).ToArray();
            Vector3 waypointTarget = Vector3.zero;

            if (ML.Math.GetDistance(waypoints[0], GameManager.Singleton.player.transform.position) < ML.Math.GetDistance(waypoints[waypoints.Length - 1], GameManager.Singleton.player.transform.position)) {
                waypointTarget = waypoints[0];
                GameManager.Singleton.PlayerController.DoPathMovement(waypoints);
            } else {
                waypointTarget = waypoints[waypoints.Length - 1];
                GameManager.Singleton.PlayerController.DoPathMovement(waypoints.Reverse().ToArray());
            }
            
            yield return 0;
            lerping = false;
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
                //ActionView.DisableTravelButton();//$$EXPERMENTAL
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
        }

        /*public void TravelEdgeButtonDeselect()
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
            Vector2 swipeDir = GameManager.Singleton.MobileInput.BackupEndTouch - GameManager.Singleton.MobileInput.BackupStartTouch;
            for (int i = 0; i < adjacentNodes.Count; i++) {
                //calculate the direction to the currentnode
                Vector2 currentnodeDir = GameManager.Singleton.NodeRegistry.TryGetValue(adjacentNodes[i]).gameObject.transform.position - ActionModel.CurrentNode.gameObject.transform.position;
                float currentDissimilarity = ML.Math.GetDistance(currentnodeDir.normalized, swipeDir.normalized);
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
        }*/

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
                //ActionView.EnableTravelButton();
                //$$EXPERMENTAL
            }

            if (ActionModel.colorMaxConnections.Sum() == ActionModel.ColorConnectionsCount.Sum()) {
                GameManager.Singleton.LevelComplete();
            }
        }
    }

}