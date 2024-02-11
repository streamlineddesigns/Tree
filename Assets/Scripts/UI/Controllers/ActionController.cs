using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using StudioByStorm.UI;
using StudioByStorm.ML;
using StudioByStorm.FX;
using StudioByStorm.EventPublishers;
using Lean.Gui;

namespace StudioByStorm {

    public class ActionController : Controller
    {
        public float EdgeDistanceThreshold;
        public ActionView ActionView;
        public ActionModel ActionModel;
        public bool isTravelButtonClicked;
        public bool isJumpIndicatorOn;
        protected bool isTravelAvailable;
        public float thresholdDistanceToBreakOutOfLerp = 0.1f;
        public bool lerping;
        [SerializeField] private Color currentJumpIndicatorColor;
        [SerializeField] private Color originalJumpIndicatorColor;
        protected bool travelIsUp;
        protected bool travelsearching = false;
        protected bool jumpIsUp;
        protected bool jumpIsUpSafetySwitch;
        protected Vector2 DownPoint;
        protected Vector2 UpPoint;
        private Vector2 jumpJoystickDownPoint;
        private Vector2 jumpJoystickUpPoint;
        public Material glowingMaterial;
        public Material originalMaterial;
        protected Edge currentSelectedEdge;
        public LeanJoystick JumpJoyStick;
        public LeanJoystick TravelJoyStick;
        public float jumpScaling = 0.75f;
        private bool canParentNodesConnect = true;
        private const float JUMPTHRESHOLD = 0.1f;
        private bool isJumpLocked;

        void Update()
        {
            if (GameManager.Singleton.UIController.CurrentViewScreen.ViewName != ViewName.GameView) {
                return;
            }


            if (! jumpIsUp) {
                ActionView.OnJumpJoyStickDown();
            }

            if (travelsearching && ActionModel.CurrentNode !=  null && GameManager.Singleton.PlayerController.isPlayerInAtmosphere()) {
                edgeSelectionCheck();
            } else {
                if (currentSelectedEdge != null) {
                    //set the current selected back to original material
                    for (int i = 0; i < currentSelectedEdge.LinkSpriteRenderers.Length; i++) {
                        currentSelectedEdge.LinkSpriteRenderers[i].material = originalMaterial;
                    }

                    currentSelectedEdge = null;
                }
            }

            if (isJumpIndicatorOn) {
                // Calculate the angle between the direction and the X axis
                float angle = Mathf.Atan2(JumpJoyStick.ScaledValue.y, JumpJoyStick.ScaledValue.x) * Mathf.Rad2Deg;

                // Rotate the object around the Z axis to match the direction
                GameManager.Singleton.PlayerController.JumpIndicator.transform.rotation = Quaternion.Euler(0, 0, angle + 90.0f);

                GameManager.Singleton.PlayerController.JumpIndicator.transform.position = GameManager.Singleton.PlayerController.gameObject.transform.position;

                Vector3 magnitude = new Vector3 (JumpJoyStick.ScaledValue.magnitude * jumpScaling, JumpJoyStick.ScaledValue.magnitude * jumpScaling, JumpJoyStick.ScaledValue.magnitude * jumpScaling);
                GameManager.Singleton.PlayerController.JumpIndicator.transform.localScale = magnitude;
            }

            EdgeButtonClickListener();
        }

        protected void edgeSelectionCheck()
        {
            Vector2 tempUpPoint = Input.mousePosition;
            //check the direction of the joystick
            //Vector2 joystickDir = tempUpPoint - DownPoint;
            Vector2 joystickDir = -TravelJoyStick.ScaledValue;
            //get the full adjacency list
            List<int> levelAdjacentNodeIDS = GameManager.Singleton.FullAdjacencyList.Get(ActionModel.CurrentNode.ID);
            //get a list of the ids the current node is connected to from the adjacency list
            List<int> adjacentNodeIDS = GameManager.Singleton.AdjacencyList.Get(ActionModel.CurrentNode.ID);

            if (lerping || adjacentNodeIDS == null || levelAdjacentNodeIDS == null || adjacentNodeIDS.Count == 0) {
                return;
            }
            //calculate the directions from the current node to the connected node
            //do a 1KNN on the list and the directions of the joystick direction
            int index = -1;
            float minimumDistance = 1000.0f;
            float thresholdDistance = 0.4f;
            Node targetNode = null;

            for (int i = 0; i < levelAdjacentNodeIDS.Count; i++) {
                int nodeID = levelAdjacentNodeIDS[i];
                Node connectedNode = GameManager.Singleton.NodeRegistry.TryGetValue(nodeID);
                Vector2 directionToConnectedNode = (connectedNode.gameObject.transform.position - ActionModel.CurrentNode.gameObject.transform.position).normalized;

                float connectedNodeDistance = ML.Math.GetDistance(joystickDir, directionToConnectedNode);

                if (connectedNodeDistance < minimumDistance && connectedNodeDistance <= thresholdDistance) {
                    minimumDistance = connectedNodeDistance;
                    index = nodeID;
                    targetNode = connectedNode;
                }
            }

            Edge tempEdge = ActionModel.CurrentNode.currentEdge;

            //now we need to find the list of edges which are connected to current and target
            if (index != -1) {
                Edge currentEdge = ActionModel.CurrentNode.currentEdge;
                Edge targetNodeEdge = targetNode.currentEdge;

                //which edge connects both together?
                if (targetNodeEdge.childID == ActionModel.CurrentNode.ID) {
                    tempEdge = targetNodeEdge;
                } else if (currentEdge.childID == targetNode.ID) {
                    tempEdge = currentEdge;
                }
            }

            //if nothing was found
            if (index == -1 || ! adjacentNodeIDS.Contains(targetNode.ID)) {

                if (currentSelectedEdge != null) {
                    //set the current selected back to original material
                    for (int i = 0; i < currentSelectedEdge.LinkSpriteRenderers.Length; i++) {
                        currentSelectedEdge.LinkSpriteRenderers[i].material = originalMaterial;
                    }
                    currentSelectedEdge = null;
                }

                //update jump indicator
                GameManager.Singleton.PlayerController.JumpIndicator.SetActive(true);
                /*if (currentJumpIndicatorColor != originalJumpIndicatorColor) {
                    currentJumpIndicatorColor = originalJumpIndicatorColor;
                    GameManager.Singleton.PlayerController.JumpIndicator.transform.GetComponentsInChildren<SpriteRenderer>().ToList().ForEach(x => {x.color = currentJumpIndicatorColor;});
                    Debug.Log("HERE1");
                }*/
                
                return;
            }

            if (tempEdge.childID != -1 && currentSelectedEdge != tempEdge) {

                //if there was all ready a selected edge
                if (currentSelectedEdge != null) {
                    //set the current selected back to original material
                    for (int i = 0; i < currentSelectedEdge.LinkSpriteRenderers.Length; i++) {
                        currentSelectedEdge.LinkSpriteRenderers[i].material = originalMaterial;
                    }
                }

                //set the new selected edge
                currentSelectedEdge = tempEdge;

                //set the new selected to glowing material
                for (int i = 0; i < currentSelectedEdge.LinkSpriteRenderers.Length; i++) {
                    currentSelectedEdge.LinkSpriteRenderers[i].material = glowingMaterial;
                }

                AudioManager.Singleton.Play(SoundType.TravelEdgeIndicator);


                //update jump indicator
                GameManager.Singleton.PlayerController.JumpIndicator.SetActive(false);
                /*Color nc = GameManager.Singleton.ColorModel.lightColor[(int) targetNode.NodeColor];
                if (currentJumpIndicatorColor != nc) {
                    currentJumpIndicatorColor = nc;
                    GameManager.Singleton.PlayerController.JumpIndicator.transform.GetComponentsInChildren<SpriteRenderer>().ToList().ForEach(x => {x.color = currentJumpIndicatorColor;});
                    Debug.Log("HERE2");
                }*/
                
            }
        }
        
        public void OnTravelJoyStickDown()
        {
            travelsearching = true;
            travelIsUp = false;
            DownPoint = Input.mousePosition;
        }

        public void OnTravelJoyStickUp()
        {
            travelsearching = false;
            travelIsUp = true;
            UpPoint = Input.mousePosition;

            OnTravelJoystickDirectionChange(Vector2.zero);
        }
        
        void OnTravelJoystickDirectionChange(Vector2 Direction)
        {
            if (!travelIsUp || !GameManager.Singleton.PlayerController.isPlayerInAtmosphere()) {
                return;
            }
            //check the direction of the joystick
            //Vector2 joystickDir = UpPoint - DownPoint;
            Vector2 joystickDir = -TravelJoyStick.ScaledValue;
            //get a list of the ids the current node is connected to from the adjacency list
            List<int> levelAdjacentNodeIDS = GameManager.Singleton.FullAdjacencyList.Get(ActionModel.CurrentNode.ID);

            List<int> adjacentNodeIDS = GameManager.Singleton.AdjacencyList.Get(ActionModel.CurrentNode.ID);

            if (lerping || adjacentNodeIDS == null || levelAdjacentNodeIDS == null) {
                return;
            }
            //calculate the directions from the current node to the connected node
            //do a 1KNN on the list and the directions of the joystick direction
            int index = -1;
            float minimumDistance = 1000.0f;
            float thresholdDistance = 0.4f;
            Node targetNode = null;

            for (int i = 0; i < levelAdjacentNodeIDS.Count; i++) {
                int nodeID = levelAdjacentNodeIDS[i];
                Node connectedNode = GameManager.Singleton.NodeRegistry.TryGetValue(nodeID);
                Vector2 directionToConnectedNode = (connectedNode.gameObject.transform.position - ActionModel.CurrentNode.gameObject.transform.position).normalized;

                float connectedNodeDistance = ML.Math.GetDistance(joystickDir, directionToConnectedNode);

                if (connectedNodeDistance < minimumDistance && connectedNodeDistance <= thresholdDistance) {
                    minimumDistance = connectedNodeDistance;
                    index = nodeID;
                    targetNode = connectedNode;
                }
            }

            //now we need to find the list of edges which are connected to current and target
            if (index != -1) {

                if (! adjacentNodeIDS.Contains(targetNode.ID)) {
                    return;
                }

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
            Vector3[] waypoints = edge.LinkSpriteRenderers.Select(x => x.gameObject.transform.position).Take(edge.activeLinkIndex).ToArray();
            Vector3 waypointTarget = Vector3.zero;

            //activate node ripple FX
            GameObject NodeRippleInStart = GameManager.Singleton.FXManager.NodeRippleInPool.Get();
            GameObject NodeRippleInEnd = GameManager.Singleton.FXManager.NodeRippleInPool.Get();
            
            Node StartNode = (edge.parentID == ActionModel.CurrentNode.ID) ? GameManager.Singleton.NodeRegistry.TryGetValue(edge.parentID) : GameManager.Singleton.NodeRegistry.TryGetValue(edge.childID);
            Vector3 StartNodeTargetRotation = StartNode.InnerGraphic.gameObject.transform.localEulerAngles;
            StartNodeTargetRotation.z -= 720.0f;
            StartNode.InnerGraphic.gameObject.transform.DORotate(StartNodeTargetRotation, 1.0f, RotateMode.LocalAxisAdd);
            NodeRippleInStart.transform.position = StartNode.gameObject.transform.position;
            NodeRippleInStart.SetActive(true);

            //if the starting waypoint is closer to the player than the last one, then use the current order
            if (ML.Math.GetDistance(waypoints[0], GameManager.Singleton.player.transform.position) < ML.Math.GetDistance(waypoints[waypoints.Length - 1], GameManager.Singleton.player.transform.position)) {
                waypointTarget = waypoints[0];
                GameManager.Singleton.PlayerController.DoPathMovement(waypoints);

            //otherwise, if the last waypoint is closer than the first, use the reverse order
            } else {
                waypointTarget = waypoints[waypoints.Length - 1];
                GameManager.Singleton.PlayerController.DoPathMovement(waypoints.Reverse().ToArray());
            }

            Node EndNode = (edge.parentID == ActionModel.CurrentNode.ID) ? GameManager.Singleton.NodeRegistry.TryGetValue(edge.childID) : GameManager.Singleton.NodeRegistry.TryGetValue(edge.parentID);
            yield return new WaitForSeconds(0.75f);
            
            Vector3 EndNodeTargetRotation = EndNode.InnerGraphic.gameObject.transform.localEulerAngles;
            EndNodeTargetRotation.z += 720.0f;
            EndNode.InnerGraphic.gameObject.transform.DORotate(EndNodeTargetRotation, 1.0f, RotateMode.LocalAxisAdd);
            NodeRippleInEnd.transform.position = EndNode.gameObject.transform.position;
            NodeRippleInEnd.SetActive(true);
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

        protected void EdgeButtonClickListener()
        {
            if (GameManager.Singleton.MobileInput.DoubleTap) {
                if (ActionView.GetEdgeButton.interactable) {
                    GetEdgeButtonClick();
                } else if (ActionView.SetEdgeButton.interactable) {
                    SetEdgeButtonClick();
                }
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

            bool canParentNodesConnectRule = (canParentNodesConnect) ? true : (ActionModel.CurrentEdge.parentNode.NodeType != NodeType.Parent || ActionModel.CurrentNode.NodeType != NodeType.Parent);
            if ( ActionModel.CurrentEdge != null && canParentNodesConnectRule && ActionModel.CurrentEdge.parentID != ActionModel.CurrentNode.ID && ML.Math.GetDistance(ActionModel.CurrentEdge.parentNode.gameObject.transform.position, ActionModel.CurrentNode.gameObject.transform.position) < EdgeDistanceThreshold && (ActionModel.CurrentNode.NodeType == NodeType.Parent && ActionModel.CurrentEdge.EdgeColor == ActionModel.CurrentNode.NodeColor && ActionModel.CurrentNode.NumOfConnections == 0 || (ActionModel.CurrentNode.NodeType == NodeType.Child || ActionModel.CurrentNode.NodeType == NodeType.Disjoint) && ActionModel.CurrentNode.NumOfConnections <= 1)) {
                ActionView.EnableSetEdgeButton();
            } else {
                ActionView.DisableSetEdgeButton();
            }
        }

        public void LockJump(bool isLocked)
        {
            isJumpLocked = isLocked;
        }

        public void OnJumpJoyStickDown()
        {
            if (isJumpLocked) {
                return;
            }

            //$$HERE
            jumpIsUpSafetySwitch = true;
            jumpIsUp = false;
            jumpJoystickDownPoint = ActionView.JumpJoyStick.ScaledValue;

            isJumpIndicatorOn = true;
            GameManager.Singleton.PlayerController.JumpIndicator.SetActive(true);
        }

        public void OnJumpJoyStickUp()
        {
            if (isJumpLocked) {
                return;
            }

            jumpIsUpSafetySwitch = false;
            StartCoroutine(DeplayedJumpIsUp());
            jumpJoystickUpPoint = ActionView.JumpJoyStick.ScaledValue;
            if (jumpJoystickUpPoint.magnitude < JUMPTHRESHOLD) {
                return;
            }
            Vector2 dir = (jumpJoystickDownPoint - jumpJoystickUpPoint);
            GameManager.Singleton.PlayerController.JumpOverride(-jumpJoystickUpPoint);
            ActionView.OnJumpJoyStickUp();

            isJumpIndicatorOn = false;
            GameManager.Singleton.PlayerController.JumpIndicator.SetActive(false);
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
            AudioManager.Singleton.Play(SoundType.GetEdge);
        }

        protected IEnumerator EdgeLightFXTravel(Edge currentEdge)
        {
            //wait to make sure the edge has had the time to shutdown properly
            yield return new WaitForSeconds(0.35f);

            Vector3[] waypoints = currentEdge.LinkSpriteRenderers.Select(x => x.gameObject.transform.position).Take(currentEdge.activeLinkIndex).ToArray();
            int lightsToTravel = 3;

            for (int k = 0; k < lightsToTravel; k++) {
                GameObject edgeLightFX = GameManager.Singleton.FXManager.EdgeLightPool.Get();
                edgeLightFX.SetActive(true);
                edgeLightFX.GetComponent<EdgeLight>().SetColor(GameManager.Singleton.ColorModel.lightColor[(int)currentEdge.EdgeColor]);
                AudioManager.Singleton.Play(SoundType.EnergyTravel);
                //send light along path :)
                for (int i = 0; i < waypoints.Length; i++) {
                    edgeLightFX.transform.DOMove(waypoints[i], 0.03f, false);
                    yield return new WaitForSeconds(0.03f);
                }
                
                edgeLightFX.transform.DOMove(GameManager.Singleton.nearbyNode.GetPosition(), 0.03f, false);
                edgeLightFX.SetActive(false);

                //edgeTarget = Vector3.zero;
                //edgeTarget.x += 0.1f;
                //currentEdge.gameObject.transform.DOPunchPosition(edgeTarget, 0.2f, 1, 0.1f, false);
            }
            
        }

        public void SetEdgeButtonClick()
        {
            AudioManager.Singleton.Play(SoundType.SetEdge);

            ActionView.DisableSetEdgeButton();
            GameManager.Singleton.LevelManager.currentLevelEdgeCount++;
            GameManager.Singleton.AdjacencyList.Add(ActionModel.CurrentEdge.parentID, ActionModel.CurrentNode.ID);
            GameManager.Singleton.AdjacencyList.Add(ActionModel.CurrentNode.ID, ActionModel.CurrentEdge.parentID);
            //Display logging
            //GameManager.Singleton.AdjacencyList.Log();

            //if the current node was disjoint, then make it a child, otherwise, leave it whatever it was
            ActionModel.CurrentNode.NodeType = (ActionModel.CurrentNode.NodeType == NodeType.Disjoint) ? NodeType.Child : ActionModel.CurrentNode.NodeType;
            //make the current nodes color the same as the current edges parent nodes color
            ActionModel.CurrentNode.NodeColor = ActionModel.CurrentEdge.parentNode.NodeColor;
            ActionModel.CurrentNode.DisplayColor();
            ActionModel.CurrentNode.LightColored.color = GameManager.Singleton.ColorModel.lightColor[(int) ActionModel.CurrentNode.NodeColor];
            ActionModel.CurrentNode.DarkColored.color = GameManager.Singleton.ColorModel.darkColor[(int) ActionModel.CurrentNode.NodeColor];
            ActionModel.CurrentNode.DisplayHairColor();
            ActionModel.CurrentNode.NumOfConnections++;
            ActionModel.CurrentEdge.childID = ActionModel.CurrentNode.ID;
            ActionModel.CurrentEdge.turnFabrikOff();
            
            StartCoroutine(EdgeLightFXTravel(ActionModel.CurrentEdge));

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
                Node[] nodes = GameManager.Singleton.ColorNodeRegistry.TryGetValue(ActionModel.CurrentNode.NodeColor).Where(x => x.NodeType != NodeType.Parent).ToArray();
                for (int i = 0; i < nodes.Length; i++) {
                    nodes[i].AddColorRing(true);
                }
                AudioManager.Singleton.Play(SoundType.ColoredRingsAdded);
            }
            //}$$Experimental: allow players to use more than what we know is the max number of edges a color will need

            if ( (ActionModel.CurrentEdge != null && ActionModel.CurrentEdge.parentID != ActionModel.CurrentNode.ID && ActionModel.CurrentEdge.EdgeColor == ActionModel.CurrentNode.NodeColor) || (ActionModel.CurrentEdge != null && ActionModel.CurrentEdge.EdgeColor == ActionModel.CurrentNode.NodeColor && ActionModel.CurrentNode.NumOfConnections > 1)  || (ActionModel.CurrentEdge == null && ActionModel.CurrentNode.NumOfConnections > 0) ){
                //ActionView.EnableTravelButton();
                //$$EXPERMENTAL
            }

            /*
             * All nodes might not be used but still allow level completion
             */
            if (GameManager.Singleton.LevelManager.parentColorsConnected.Where(x => x.Value == true).Count() >= (GameManager.Singleton.LevelManager.currentLevelParentCount / 2) ) {
                GameEventPublisher.PublishGameStateChange(GameState.LevelComplete);
            }

            /*
             * All nodes have to be used to allow level completion
            if (GameManager.Singleton.LevelManager.parentColorsConnected.Where(x => x.Value == true).Count() >= (GameManager.Singleton.LevelManager.currentLevelParentCount / 2) && GameManager.Singleton.LevelManager.currentLevelEdgeCount >= (GameManager.Singleton.LevelManager.currentLevelNodeCount - (GameManager.Singleton.LevelManager.currentLevelParentCount / 2))) {
                GameEventPublisher.PublishGameStateChange(GameState.LevelComplete);
            }*/
        }
    }

}