using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StudioByStorm.Graph;

namespace StudioByStorm.PCG {

    public class ProceduralLevelGenerator : MonoBehaviour
    {
        [SerializeField] private GraphConstructionManager GraphConstructionManager;
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

        public void Search()
        {
            int numberOfSolutions = 0;

            while (numberOfSolutions == 0) {

                bool isThereAPathForEachColor = false;

                while (! isThereAPathForEachColor) {
                    PlaceRandomColors();
                    FindAllColorsInGraphConstructionManager();
                    FindAllColorPaths();
                    isThereAPathForEachColor = IsThereAPathForEachColor();
                }
                
                GetCartesianProductOfAllPathsIntoPathIndexCombinations();
                CreateAllPotentialSolutionsFromPathIndexCombinations();
                FindSolutions();
                numberOfSolutions = workingSolutions.Count;
            }

            DisplayWorkingSolutions();
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
            int requiredNumberOfColorsToPlace = (nodeCount >= 12) ? 4 : (nodeCount >= 9) ? 3 : (nodeCount >= 6) ? 2 : (nodeCount >= 3) ? 1 : 0;
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

        private void FindAllColorPaths () {
            bool isDebugging = true;
            NodeColorToAllPaths = new Dictionary<NodeColor, List<List<int>>>();
            DFSPaths DFSPaths = new DFSPaths(GraphConstructionManager.AdjacencyList, isDebugging);

            for (int j = 0; j < allColors.Count; j++) {
                //get the current color to operate on
                NodeColor colorKey = allColors[j];
                //get the node ID's of the current color
                List<int> NodeIDs = NodeColorToNodeIDs[colorKey];
                if (NodeIDs.Count != 2) {
                    Debug.LogError("There should be 2 of each color on the graph!!");
                    return;
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
            }
        }

        private bool IsThereAPathForEachColor()
        {
            bool isThereAPathForEachColor = true;
            List<bool> longEnoughChecks = new List<bool>{false, false, false, false};
            bool isThereALongEnoughPathForEachColor = true;

            for (int i = 0; i < allColors.Count; i++) {
                NodeColor colorKey = allColors[i];
                if (NodeColorToAllPaths[colorKey].Count <= 0) {
                    isThereAPathForEachColor = false;
                }

                for (int j = 0; j < NodeColorToAllPaths[colorKey].Count; j++) {
                    if (NodeColorToAllPaths[colorKey][j].Count >= 3) {
                        longEnoughChecks[i] = true;
                    }
                }
                
            }

            isThereALongEnoughPathForEachColor = (longEnoughChecks.Where(x => x == true).ToList().Count == allColors.Count);

            Debug.Log("Is There A Path For Each Color: " + isThereAPathForEachColor);
            Debug.Log("Is There A Long Enough Path For Each Color: " + isThereALongEnoughPathForEachColor);

            return (isThereAPathForEachColor && isThereALongEnoughPathForEachColor);
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
                    indexs.Add(j);
                }

                listOfColorToAllPathsIndexs.Add(indexs);
            }
            
            //matches the path indexs from NodeColorToAllPaths to lists of combinations of paths
            bool isDebugging = false;
            Cartesian Cartesian = new Cartesian(isDebugging);
            cartesianPathIndexCombinations = Cartesian.GetCartesianProduct(listOfColorToAllPathsIndexs).Select(x => x.ToList()).ToList();
        }

        private void CreateAllPotentialSolutionsFromPathIndexCombinations()
        {
            allPotentialSolutions = new List<List<List<int>>>();

            //iterate over all possible combinations of solutions
            for (int i = 0; i < cartesianPathIndexCombinations.Count; i++) {

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

                allPotentialSolutions.Add(potentialSolution);
            }
        }

        private void FindSolutions()
        {
            workingSolutions = new List<List<List<int>>>();

            for (int k = 0; k < allPotentialSolutions.Count; k++) {

                Dictionary<int, int> usedNodeIds = new Dictionary<int, int>();
                bool workingSolution = true;

                for (int l = 0; l < allPotentialSolutions[k].Count; l++) {
                    
                    //the number of nodes in all paths for a solution needs to equal the number of existing nodes in the level i.e. all nodes must be used
                    //the number of nodes per path must also be greater than 2
                    if (allPotentialSolutions[k].Select(x=>x.Count).Sum() == GraphConstructionManager.nodeColors.Count && allPotentialSolutions[k][l].Count > 2) {
                        
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

                    } else {
                        workingSolution = false;
                        break;
                    }
                }

                if (workingSolution) {
                    workingSolutions.Add(allPotentialSolutions[k]);
                }
            }
        }

        private void DisplayWorkingSolutions()
        {
            Debug.Log("Total Solutions: " + workingSolutions.Count);

            for (int k = 0; k < workingSolutions.Count; k++) {
                for (int l = 0; l < workingSolutions[k].Count; l++) {
                    Debug.Log(string.Join(" -> ", workingSolutions[k][l]));
                }
            }
        }
    }

}