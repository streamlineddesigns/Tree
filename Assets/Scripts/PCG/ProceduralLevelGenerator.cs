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
        //stores all path combinations
        private List<List<int>> cartesianPathIndexCombinations;

        public List<List<List<int>>> allSolutions;

        public void Search()
        {
            FindAllColorsInGraphConstructionManager();
            FindAllColorPaths();
            GetCartesianProductOfAllPaths();
            CreateAllSolutionsFromPathIndexCombinations();
            FindSolutions();
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

        private void GetCartesianProductOfAllPaths()
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

        private void CreateAllSolutionsFromPathIndexCombinations()
        {
            allSolutions = new List<List<List<int>>>();

            //iterate over all possible combinations of solutions
            for (int i = 0; i < cartesianPathIndexCombinations.Count; i++) {

                //a set of paths i.e. potential solution to level
                List<List<int>> potentialSolution = new List<List<int>>();

                for (int j = 0; j < cartesianPathIndexCombinations[i].Count; j++) {

                    NodeColor colorKey = allColors[j];
                    int PathIndex = cartesianPathIndexCombinations[i][j];
                    //a list of connected nodes by color i.e a path
                    List<int> partialSolution = NodeColorToAllPaths[colorKey][PathIndex];
                    potentialSolution.Add(partialSolution);
                }

                allSolutions.Add(potentialSolution);
            }
        }

        private void FindSolutions()
        {
            for (int k = 0; k < allSolutions.Count; k++) {
                for (int l = 0; l < allSolutions[k].Count; l++) {
                    
                    if (allSolutions[k].Select(x=>x.Count).Sum() == GraphConstructionManager.nodeColors.Count) {
                        Debug.Log(string.Join(" -> ", allSolutions[k][l]));
                    }
                    
                }
            }
        }
    }

}