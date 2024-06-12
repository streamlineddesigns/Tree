using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using StudioByStorm.FX;
using StudioByStorm.EventPublishers;
using StudioByStorm.UI.Controllers;

namespace StudioByStorm.Helpers {

    [System.Serializable]
    public struct Path {
        public List<int> ids;
        public NodeColor color;
    }

    public class EdgePlacementHelper : MonoBehaviour
    {
        public List<Path> paths;
        public bool isCheckingLevelCompletion = false;
        private ActionController actionController;
        private GameController gameController;

        protected void Start()
        {
            StartCoroutine(DelayedStart());
        }
        
        IEnumerator DelayedStart()
        {
            yield return null;
            actionController = GameManager.Singleton.ControllerRegistry.TryGetValue(ViewName.ActionView) as ActionController;
            gameController = GameManager.Singleton.ControllerRegistry.TryGetValue(ViewName.GameView) as GameController;
        }

        public void LinearPlacement()
        {
            for (int i = 0; i < paths.Count; i++) {
                StartCoroutine(SetPath(paths[i].ids));
            }
        }

        public void IterativePlacement()
        {
            for (int i = 0; i < paths.Count; i++) {
                StartCoroutine(SetPathsOneAtATime(paths[i].ids, paths[i].color));
            }
        }

        IEnumerator SetPathsOneAtATime(List<int> nodeIDs, NodeColor color)
        {
            List<int> tempNodeIDs = new List<int>();

            tempNodeIDs.Add(nodeIDs[0]);

            for (int i = 0; i < nodeIDs.Count; i++) {
                if (i + 1 <= nodeIDs.Count - 1) {
                    tempNodeIDs.Add(nodeIDs[i + 1]);
                    yield return StartCoroutine(SetPath(tempNodeIDs));
                }

                if (tempNodeIDs.Count < nodeIDs.Count) {
                    switch(color) {
                        case NodeColor.Blue:
                            gameController.ResetBlueButtonClick();
                            break;
                        case NodeColor.Green:
                            gameController.ResetGreenButtonClick();
                            break;
                        case NodeColor.Purple:
                            gameController.ResetPurpleButtonClick();
                            break;
                        case NodeColor.Orange:
                            gameController.ResetOrangeButtonClick();
                            break;
                        case NodeColor.Yellow:
                            gameController.ResetYellowButtonClick();
                            break;
                    }

                    yield return new WaitForSeconds(0.1f);
                }
            }
        }

        IEnumerator SetPath(List<int> nodeIDs)
        {
            for (int i = 0; i < nodeIDs.Count; i++) {
                if (i + 1 <= nodeIDs.Count - 1) {
                    yield return StartCoroutine(SetEdge(nodeIDs[i], nodeIDs[i + 1]));
                }
            }

            if (isCheckingLevelCompletion) LevelCompleteCheck();
        }

        IEnumerator SetEdge(int parentNodeID, int childNodeID)
        {
            Node parentNode = GameManager.Singleton.NodeRegistry.TryGetValue(parentNodeID);
            Node childNode = GameManager.Singleton.NodeRegistry.TryGetValue(childNodeID);

            Edge edge = GameManager.Singleton.EdgeRegistry.TryGetValue(parentNodeID);
            edge.gameObject.SetActive(true);

            UnityEngine.U2D.IK.FabrikSolver2D fabrikSolver2D = edge.FabrikSolver2D;
            Transform emptyTarget = edge.emptyTarget.transform;
            emptyTarget.transform.position = parentNode.gameObject.transform.position;
            fabrikSolver2D.GetChain(fabrikSolver2D.chainCount).target = emptyTarget;

            Vector2 targetPosition = (Vector2) childNode.transform.position;
            emptyTarget.transform.position = targetPosition;
            yield return null;

            GameManager.Singleton.LevelManager.currentLevelEdgeCount++;
            GameManager.Singleton.AdjacencyList.Add(parentNodeID, childNodeID);
            GameManager.Singleton.AdjacencyList.Add(childNodeID, parentNodeID);
            //Display logging
            //GameManager.Singleton.AdjacencyList.Log();

            //if the current node was disjoint, then make it a child, otherwise, leave it whatever it was
            childNode.NodeType = (childNode.NodeType == NodeType.Disjoint) ? NodeType.Child : childNode.NodeType;
            //make the current nodes color the same as the current edges parent nodes color
            if (childNode.NodeType == NodeType.Child) {
                childNode.NodeColor = parentNode.NodeColor;
                childNode.DisplayColor();
                childNode.LightColored.color = GameManager.Singleton.ColorModel.lightColor[(int) childNode.NodeColor];
                childNode.DarkColored.color = GameManager.Singleton.ColorModel.darkColor[(int) childNode.NodeColor];
                //v1.13childNode.DisplayHairColor();
            }
            
            childNode.NumOfConnections++;
            edge.childID = childNode.ID;
            edge.turnFabrikOff();
            edge.DisplayLineRendererFX(0.3f);
            
            StartCoroutine(EdgeLightFXTravel(edge));

            GameManager.Singleton.ColorNodeRegistry.Add(childNode.NodeColor, GameManager.Singleton.NodeRegistry.TryGetValue(parentNodeID));
            GameManager.Singleton.ColorNodeRegistry.Add(childNode.NodeColor, GameManager.Singleton.NodeRegistry.TryGetValue(childNodeID));
            GameManager.Singleton.ColorEdgeRegistry.Add(childNode.NodeColor, edge);

            int colorIndex = (int) childNode.NodeColor;
            actionController.ActionModel.ColorConnectionsCount[colorIndex]++;

            if (GameManager.Singleton.ColorNodeRegistry.TryGetValue(childNode.NodeColor).Where(x => x.NodeType == NodeType.Parent).ToArray().Length < 2) {
                
            } else {
                GameManager.Singleton.LevelManager.parentColorsConnected[childNode.NodeColor] = true;
                Node[] nodes = GameManager.Singleton.ColorNodeRegistry.TryGetValue(childNode.NodeColor).Where(x => x.NodeType != NodeType.Parent).ToArray();
                for (int i = 0; i < nodes.Length; i++) {
                    nodes[i].AddColorRing(true);
                    //nodes[i].ActivateHairs();
                    //nodes[i].DisplayHairColor();
                }
                AudioManager.Singleton.Play(SoundType.ColoredRingsAdded);

                List<int> connectedNodeIDs = new List<int>();
                Node startNode = GameManager.Singleton.ColorNodeRegistry.TryGetValue(childNode.NodeColor).Where(x => x.NodeType == NodeType.Parent).First();
                StartCoroutine(CompleteEdgeLightFXTravel(startNode));
            }

            yield return new WaitForSeconds(0.5f);
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
            }
            
        }

        protected IEnumerator CompleteEdgeLightFXTravel(Node startNode)
        {
            List<Vector3> waypoints = new List<Vector3>();

            Stack<int> nodes = new Stack<int>();
            List<int> visitedNodeIDs = new List<int>();
            nodes.Push(startNode.ID);
            
            while(nodes.Count > 0) {
                int ID = nodes.Pop();
                List<int> connectedNodes = GameManager.Singleton.AdjacencyList.Get(ID);

                visitedNodeIDs.Add(ID);
                waypoints.Add(GameManager.Singleton.NodeRegistry.TryGetValue(ID).gameObject.transform.position);

                connectedNodes.ForEach(x => {
                    int currentNodeID = x;
                    if (! visitedNodeIDs.Contains(currentNodeID)) {
                        nodes.Push(currentNodeID);
                    }
                });
            }

            GameObject edgeLightFX = GameManager.Singleton.FXManager.EdgeLightPool.Get();
            edgeLightFX.gameObject.transform.position = waypoints[0];
            edgeLightFX.SetActive(true);
            edgeLightFX.GetComponent<EdgeLight>().SetColor(GameManager.Singleton.ColorModel.lightColor[(int)startNode.NodeColor]);

            for (int i = 0; i < waypoints.Count; i++) {
                edgeLightFX.transform.DOMove(waypoints[i], 0.25f, false).SetEase(Ease.Linear);
                yield return new WaitForSeconds(0.25f);
            }
        }


        protected void LevelCompleteCheck()
        {
            if (GameManager.Singleton.LevelManager.parentColorsConnected.Where(x => x.Value == true).Count() >= (GameManager.Singleton.LevelManager.currentLevelParentCount / 2) ) {
                GameEventPublisher.PublishGameStateChange(GameState.LevelComplete);
            }
        }
    }

}