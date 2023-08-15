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
            FindAllColorsInGraphConstructionManager();
            FindAllColorPaths();
            GetCartesianProductOfAllPathsIntoPathIndexCombinations();
            CreateAllPotentialSolutionsFromPathIndexCombinations();
            FindSolutions();
            DisplayWorkingSolutions();
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
        }

        private void FindAllColorPaths () {
            NodeColorToAllPaths = new Dictionary<NodeColor, List<List<int>>>();
            DFSPaths DFSPaths = new DFSPaths(GraphConstructionManager.AdjacencyList, false);

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
            Cartesian Cartesian = new Cartesian(false);
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