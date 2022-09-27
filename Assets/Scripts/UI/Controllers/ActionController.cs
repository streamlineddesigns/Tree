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

    public class ActionController : Controller
    {
        public float EdgeDistanceThreshold;
        public ActionView ActionView;
        public ActionModel ActionModel;
        public bool isTravelButtonClicked;
        protected bool isTravelAvailable;
        public float thresholdDistanceToBreakOutOfLerp = 0.1f;
        public bool lerping;
        protected bool travelIsUp;
        protected bool jumpIsUp;
        protected bool jumpIsUpSafetySwitch;
        protected Vector2 DownPoint;
        protected Vector2 UpPoint;
        private Vector2 jumpJoystickDownPoint;
        private Vector2 jumpJoystickUpPoint;

        void Update()
        {
            if (! jumpIsUp) {
                ActionView.OnJumpJoyStickDown();
            }
        }
        
        public void OnTravelJoyStickDown()
        {
            travelIsUp = false;
            DownPoint = Input.mousePosition;
        }

        public void OnTravelJoyStickUp()
        {
            travelIsUp = true;
            UpPoint = Input.mousePosition;

            OnTravelJoystickDirectionChange(Vector2.zero);
        }
        
        void OnTravelJoystickDirectionChange(Vector2 Direction)
        {
            if (!travelIsUp) {
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
            //Debug.Log("Lerping");
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

        public void ManualOnTriggerEnter2D(Node node)
        {
            ActionModel.CurrentNode = node;
            EnableActionButtons();
        }

        public void ManualOnTriggerExit2D(Node node)
        {
            ActionView.DisableGetEdgeButton();
            ActionView.DisableSetEdgeButton();
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

        public void OnJumpJoyStickDown()
        {
            jumpIsUpSafetySwitch = true;
            jumpIsUp = false;
            jumpJoystickDownPoint = ActionView.JumpJoyStick.ScaledValue;
        }

        public void OnJumpJoyStickUp()
        {
            jumpIsUpSafetySwitch = false;
            StartCoroutine(DeplayedJumpIsUp());
            jumpJoystickUpPoint = ActionView.JumpJoyStick.ScaledValue;
            Vector2 dir = (jumpJoystickDownPoint - jumpJoystickUpPoint);
            GameManager.Singleton.PlayerController.JumpOverride(dir);
            ActionView.OnJumpJoyStickUp();
        }

        //this just allows our animations to fade out for a quater of an extra second
        IEnumerator DeplayedJumpIsUp()
        {
            yield return new WaitForSeconds(0.25f);
            if (! jumpIsUpSafetySwitch) {
                jumpIsUp = true;
            }
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
            GameManager.Singleton.LevelManager.currentLevelEdgeCount++;
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
            } else {
                GameManager.Singleton.LevelManager.parentColorsConnected[ActionModel.CurrentNode.NodeColor] = true;
            }
            //}$$Experimental: allow players to use more than what we know is the max number of edges a color will need

            if ( (ActionModel.CurrentEdge != null && ActionModel.CurrentEdge.parentID != ActionModel.CurrentNode.ID && ActionModel.CurrentEdge.EdgeColor == ActionModel.CurrentNode.NodeColor) || (ActionModel.CurrentEdge != null && ActionModel.CurrentEdge.EdgeColor == ActionModel.CurrentNode.NodeColor && ActionModel.CurrentNode.NumOfConnections > 1)  || (ActionModel.CurrentEdge == null && ActionModel.CurrentNode.NumOfConnections > 0) ){
                //ActionView.EnableTravelButton();
                //$$EXPERMENTAL
            }

            if (GameManager.Singleton.LevelManager.parentColorsConnected.Where(x => x.Value == true).Count() >= (GameManager.Singleton.LevelManager.currentLevelParentCount / 2) && GameManager.Singleton.LevelManager.currentLevelEdgeCount >= (GameManager.Singleton.LevelManager.currentLevelNodeCount - (GameManager.Singleton.LevelManager.currentLevelParentCount / 2))) {
                GameManager.Singleton.LevelComplete();
            }
        }
    }

}