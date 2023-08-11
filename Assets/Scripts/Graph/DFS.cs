using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StudioByStorm.Graph {

    public class DFS
    {
        private AdjacencyList AdjacencyList;
        private List<int> visited;
        private Stack<int> nodeIDs;

        public DFS(AdjacencyList adjacencyList)
        {
            AdjacencyList = adjacencyList;
            visited = new List<int>();
            nodeIDs = new Stack<int>();
        }

        public void Search(int startNodeID)
        {
            nodeIDs.Push(startNodeID);
            visited.Add(startNodeID);

            while(nodeIDs.Count > 0) {

                int currentID = nodeIDs.Pop();
                Debug.Log(currentID);

                List<int> connectedNodes = AdjacencyList.Get(currentID);

                for (int i = 0; i < connectedNodes.Count; i++) {
                    int searchID = connectedNodes[i];
                    if (! visited.Contains(searchID)) {
                        nodeIDs.Push(searchID);
                        visited.Add(searchID);
                    }
                }

            }

        }

    }

}