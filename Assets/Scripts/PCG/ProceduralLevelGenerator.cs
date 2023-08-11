using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StudioByStorm.Graph;

namespace StudioByStorm.PCG {

    public class ProceduralLevelGenerator : MonoBehaviour
    {
        [SerializeField] private GraphConstructionManager GraphConstructionManager;

        private Dictionary<NodeColor, List<int>> NodeColorToNodeIDs = new Dictionary<NodeColor, List<int>>();
        private List<NodeColor> allColors;

        public void Search()
        {
            FindAllColorsInGraphConstructionManager();

            DFSPaths DFSPaths = new DFSPaths(GraphConstructionManager.AdjacencyList);

            for (int j = 0; j < allColors.Count; j++) {
                //get the current color to operate on
                NodeColor colorKey = allColors[j];
                //get the node ID's of the current color
                List<int> NodeIDs = NodeColorToNodeIDs[colorKey];

                //create a list of node IDS to exlude from search ie if they are not grayscale && not the same color
                List<int> excludedNodeIds = new List<int>();

                for (int k = 0; k < GraphConstructionManager.nodeColors.Count; k++) {
                    if (GraphConstructionManager.nodeColors[k] != NodeColor.GrayScale && GraphConstructionManager.nodeColors[k] != colorKey) {
                        excludedNodeIds.Add(k);
                        //Debug.Log(k);
                    }
                }

                //search
                DFSPaths.Search(NodeIDs[0], NodeIDs[1], excludedNodeIds);
            }
        }

        private void FindAllColorsInGraphConstructionManager()
        {
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
    }

}