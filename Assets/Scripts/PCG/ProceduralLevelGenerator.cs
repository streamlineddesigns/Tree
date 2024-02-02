using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using StudioByStorm.Graph;

namespace StudioByStorm.PCG {

    public class ProceduralLevelGenerator : MonoBehaviour
    {
        public int attempts = 0;
        private GraphConstructionManager GraphConstructionManager;
        //stores color: node ids that are the input color
        private Dictionary<NodeColor, List<int>> NodeColorToNodeIDs;
        //stores all colors used in graph construction
        private List<NodeColor> allColors;
        //stores color: all paths
        private Dictionary<NodeColor, List<List<int>>> NodeColorToAllPaths;
        //stores all path combinations. index refers to inner list inside NodeColorToAllPaths, not NodeColor
        private List<List<int>> cartesianPathIndexCombinations;
        //stores the path combinations of potential solutions built from the cartesian path index combinations
        private List<List<List<int>>> allPotentialSolutions;
        //stores the path combinations that actually solve the level
        private List<List<List<int>>> workingSolutions;
        private List<List<List<int>>> nearSolutions;
        [SerializeField] private bool isUsingMaxDistanceBetweenNodes = false;

        public void DependencyInjection(GraphConstructionManager gcm)
        {
            GraphConstructionManager = gcm;
        }

        public IEnumerator CreateLevel()
        {
            int numberOfSolutions = 0;

            while (numberOfSolutions == 0) {
                yield return null;
                createAdjacencyList();
                bool foundColorPaths = false;

                while (! foundColorPaths) {
                    yield return null;
                    attempts++;
                    PlaceRandomColors();
                    FindAllColorsInGraphConstructionManager();
                    foundColorPaths = FindAllColorPaths();
                }
                
                GetCartesianProductOfAllPathsIntoPathIndexCombinations();
                CreateAllPotentialSolutionsFromPathIndexCombinations();
                FindSolutions();
                numberOfSolutions = workingSolutions.Count;
            }

            if (numberOfSolutions != 0) {
                //re-create the adjacency list
                createAdjacencyList(1.5f);
                DisplayWorkingSolutions();
            }

            GraphConstructionManager.CreateLevelData();
        }

        private void createAdjacencyList(float maxDistanceBetweenNodesToCreateEdge = 1.2f)
        {
            maxDistanceBetweenNodesToCreateEdge = (isUsingMaxDistanceBetweenNodes && maxDistanceBetweenNodesToCreateEdge == 1.2f) ? 1.5f : 1.2f;

            int nodeCount = GraphConstructionManager.nodePositions.Count;

            for (int i = 0; i < GraphConstructionManager.nodePositions.Count; i++) {

                for (int j = 0; j < GraphConstructionManager.nodePositions.Count; j++) {
                    if (i == j) {
                        continue;
                    }

                    //remove all edges before adding any
                    GraphConstructionManager.AdjacencyList.Remove(i, j);

                    if (Vector3.Distance(GraphConstructionManager.nodePositions[i], GraphConstructionManager.nodePositions[j]) <= maxDistanceBetweenNodesToCreateEdge) {
                        GraphConstructionManager.addToAdjacencyList(i, j);
                    }
                }
            }
        }

        private bool PlaceRandomColors()
        {
            //reset all nodes to grey
            for (int i = 0; i < GraphConstructionManager.nodeColors.Count; i++) {
                int nodeIndex = i;
                GraphConstructionManager.nodeColors[nodeIndex] = NodeColor.GrayScale;
                GraphConstructionManager.nodeGameObjectReferences[nodeIndex].GetComponent<SpriteRenderer>().color = GraphConstructionManager.ColorModel.lightColor[(int)NodeColor.GrayScale];
            }

            
            //retrieve all available NodeColors
            List<NodeColor> availableColors = new List<NodeColor>();
            foreach (NodeColor nc in Enum.GetValues(typeof(NodeColor))) {
                if (nc != NodeColor.GrayScale) {
                    availableColors.Add(nc);
                }
            }

            //color shuffling
            int nodeCount = GraphConstructionManager.nodeColors.Count;
            int minimumNodesPerPath = 3;
            int colorsToPlaceOptionOne = (nodeCount >= 12) ? 4 : (nodeCount >= 9) ? 3 : (nodeCount >= 6) ? 2 : 1;
            int colorsToPlaceOptionTwo = (nodeCount >= 15) ? 4 : (nodeCount >= 12) ? 3 : (nodeCount >= 9) ? 2 : 1;
            int requiredNumberOfColorsToPlace = (UnityEngine.Random.Range(1, 11) <= 5) ? colorsToPlaceOptionOne : colorsToPlaceOptionTwo;
            if (requiredNumberOfColorsToPlace <= 0) {
                Debug.LogError("You need to place more nodes!! Can't make a level without at least " + minimumNodesPerPath + " nodes!");
                return false;
            }
            List<NodeColor> colorsBeingUsed = new List<NodeColor>();
            //shuffle available colors
            List<NodeColor> shuffledColors = new Shuffle().FisherYates(availableColors);
            //add the required number of colors to colorsBeingUsed list
            for (int j = 0; j < requiredNumberOfColorsToPlace; j++) {
                //add 2 of each color
                colorsBeingUsed.Add(shuffledColors[j]);
                colorsBeingUsed.Add(shuffledColors[j]);
            }

            int colorsPlaced = 0;              //requiredNumberOfColorsToPlace * 2 because 2 of each color gets placed
            int totalColorNodesRequiredToPlace = requiredNumberOfColorsToPlace * 2;

            for (int k = 0; k < colorsBeingUsed.Count; k++) {
                //Debug.Log(colorsBeingUsed[k].ToString());
            }

            while(colorsPlaced < totalColorNodesRequiredToPlace) {
                int randomNodeID = UnityEngine.Random.Range(0, GraphConstructionManager.nodeColors.Count);
                if (GraphConstructionManager.nodeColors[randomNodeID] == NodeColor.GrayScale) {
                    NodeColor currentColor = colorsBeingUsed[colorsPlaced];
                    GraphConstructionManager.nodeColors[randomNodeID] = currentColor;
                    GraphConstructionManager.nodeGameObjectReferences[randomNodeID].GetComponent<SpriteRenderer>().color = GraphConstructionManager.ColorModel.lightColor[(int)currentColor];
                    colorsPlaced++;
                }
            }

            Debug.Log("PlaceRandomColors COMPLETED");
            return true;
        }

        private void FindAllColorsInGraphConstructionManager()
        {
            NodeColorToNodeIDs = new Dictionary<NodeColor, List<int>>();

            //iterate over all of nodeColors.. these are the nodes placed on the level creator
            for (int i = 0; i < GraphConstructionManager.nodeColors.Count; i++) {

                //not only is this the node index, but it's also the node ID
                int nodeIndex = i;
                NodeColor currentColor = GraphConstructionManager.nodeColors[nodeIndex];

                //skip child nodes
                if (currentColor == NodeColor.GrayScale) {
                    continue;
                }

                //Add key/value pair of NodeColor:List of NodeIDs of that color
                if (NodeColorToNodeIDs.ContainsKey(currentColor)) {
                    NodeColorToNodeIDs[currentColor].Add(nodeIndex);
                } else {
                    NodeColorToNodeIDs.Add(currentColor, new List<int>{nodeIndex});
                }
            }

            //this is all the colors in the NodeColorToNodeIDs dictionary
            allColors = NodeColorToNodeIDs.Keys.ToList();

            Debug.Log("All Colors Count: " + allColors.Count);
        }

        private bool FindAllColorPaths () {
            bool isDebugging = false;
            NodeColorToAllPaths = new Dictionary<NodeColor, List<List<int>>>();
            int minPathLength = 2;
            DFSPaths DFSPaths = new DFSPaths(GraphConstructionManager.AdjacencyList, minPathLength, isDebugging);
            int pathCount = 0;
            bool isThereAPathForEachColor = true;
            bool isCTooLarge = false;
            int C = 1;
            int mostPathsOfAnyColor = 1;

            for (int j = 0; j < allColors.Count; j++) {
                //get the current color to operate on
                NodeColor colorKey = allColors[j];
                //get the node ID's of the current color
                List<int> NodeIDs = NodeColorToNodeIDs[colorKey];
                if (NodeIDs.Count != 2) {
                    Debug.LogError("There should be 2 of each color on the graph!!");
                    return false;
                }

                //create a list of node IDS to exlude from search ie if they are not grayscale && not the same color
                List<int> excludedNodeIds = new List<int>();

                for (int k = 0; k < GraphConstructionManager.nodeColors.Count; k++) {
                    if (GraphConstructionManager.nodeColors[k] != NodeColor.GrayScale && GraphConstructionManager.nodeColors[k] != colorKey) {
                        excludedNodeIds.Add(k);
                        //Debug.Log(k);
                    }
                }

                //search
                List<List<int>> paths = DFSPaths.Search(NodeIDs[0], NodeIDs[1], excludedNodeIds);
                NodeColorToAllPaths.Add(colorKey, paths);
                Debug.Log(colorKey.ToString() + " Count : " + paths.Count);
                pathCount += paths.Count;

                if (paths.Count == 0) {
                    isThereAPathForEachColor = false;
                    break;
                }

                mostPathsOfAnyColor = (paths.Count > mostPathsOfAnyColor) ? paths.Count : mostPathsOfAnyColor;

                C *= paths.Count;
                if (C >= 100000 || ( (C * mostPathsOfAnyColor) >= 100000 && j < (allColors.Count - 1) )) {
                    isCTooLarge = true;
                    break;
                }
            }

            Debug.Log("FindAllColorPaths - pathCount: " + pathCount);
            return (isThereAPathForEachColor && !isCTooLarge);
        }

        private void GetCartesianProductOfAllPathsIntoPathIndexCombinations()
        {
            List<List<int>> listOfColorToAllPathsIndexs = new List<List<int>>();
            
            for (int i = 0; i < allColors.Count; i++) {
                //get a color
                NodeColor colorKey = allColors[i];
                //get the paths from that color
                List<List<int>> paths = NodeColorToAllPaths[colorKey];
                List<int> indexs = new List<int>();
                //add the index of the paths
                for (int j = 0; j < paths.Count; j++) {
                    //only include paths longer than 2 nodes each
                    //if (paths[j].Count > 2) {
                        indexs.Add(j);
                    //}
                }

                listOfColorToAllPathsIndexs.Add(indexs);
            }
            
            //matches the path indexs from NodeColorToAllPaths to lists of combinations of paths
            bool isDebugging = false;
            Cartesian Cartesian = new Cartesian(isDebugging);
            cartesianPathIndexCombinations = Cartesian.GetCartesianProduct(listOfColorToAllPathsIndexs).Select(x => x.ToList()).ToList();

            Debug.Log("listOfColorToAllPathsIndexs count: " + listOfColorToAllPathsIndexs.Count);
            Debug.Log("cartesianPathIndexCombinations count: " + cartesianPathIndexCombinations.Count);
        }

        private void CreateAllPotentialSolutionsFromPathIndexCombinations()
        {
            allPotentialSolutions = new List<List<List<int>>>(100000);

            //iterate over all possible combinations of solutions
            Parallel.For(0, cartesianPathIndexCombinations.Count, (i, state) => {
            //for (int i = 0; i < cartesianPathIndexCombinations.Count; i++) {

                //a set of paths i.e. potential solution to level
                List<List<int>> potentialSolution = new List<List<int>>();

                for (int j = 0; j < cartesianPathIndexCombinations[i].Count; j++) {
                    //one path per color will go into potential solution
                    NodeColor colorKey = allColors[j];
                    int PathIndex = cartesianPathIndexCombinations[i][j];
                    //a list of connected nodes by color i.e a path
                    List<int> partialSolution = NodeColorToAllPaths[colorKey][PathIndex];
                    potentialSolution.Add(partialSolution);
                }

                //only include a potential solution if the sum of the node's in all paths equals the total number of nodes. ie. all nodes are used in level
                //if (potentialSolution.Select(x=>x.Count).Sum() == GraphConstructionManager.nodeColors.Count) {
                allPotentialSolutions.Add(potentialSolution);
                //}
            });

            Debug.Log("allPotentialSolutions count: " + allPotentialSolutions.Count);
        }

        private void FindSolutions()
        {
            bool foundASolution = false;
            workingSolutions = new List<List<List<int>>>();
            nearSolutions = new List<List<List<int>>>();

            Parallel.For(0, allPotentialSolutions.Count, (k, state) => {

                Dictionary<int, int> usedNodeIds = new Dictionary<int, int>();
                bool workingSolution = true;

                if (allPotentialSolutions[k] != null) {
                    for (int l = 0; l < allPotentialSolutions[k].Count; l++) {
                            
                        //iterate over all nodes
                        for (int m = 0; m < allPotentialSolutions[k][l].Count; m++) {
                            //get the node id
                            int NodeID = allPotentialSolutions[k][l][m];
                            //working solutions have paths where nodes are ONLY used once
                            if (usedNodeIds.ContainsKey(NodeID)) {
                                workingSolution = false;
                                break;
                            } else {
                                //keep track of the Node ID to ensure it's only used once
                                usedNodeIds.Add(NodeID, NodeID);
                            }
                        }

                        if (! workingSolution) {
                            break;
                        }
                    }
                } else {
                    workingSolution = false;
                }
                

            
                if (workingSolution) {
                    if (allPotentialSolutions[k].Select(x=>x.Count).Sum() == GraphConstructionManager.nodeColors.Count) {
                        workingSolutions.Add(allPotentialSolutions[k]);
                        foundASolution = true;
                    } else {
                        nearSolutions.Add(allPotentialSolutions[k]);
                    }
                }
            });
        }

        private void DisplayWorkingSolutions()
        {
            Debug.Log("Total Solutions: " + workingSolutions.Count);
            Debug.Log("Near Solutions: " + nearSolutions.Count);

            for (int k = 0; k < workingSolutions.Count; k++) {
                for (int l = 0; l < workingSolutions[k].Count; l++) {
                    Debug.Log(string.Join(" -> ", workingSolutions[k][l]));
                }
            }
        }
    }

}