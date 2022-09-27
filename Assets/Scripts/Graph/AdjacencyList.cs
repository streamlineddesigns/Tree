using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StudioByStorm.Graph {

    public class AdjacencyList
    {
        protected Dictionary<int, List<int>> Nodes = new Dictionary<int, List<int>>();

        public bool Contains(int nodeID, int edgeID)
        {
            if (Nodes.ContainsKey(nodeID) && Nodes[nodeID].Contains(edgeID)) {
                return true;
            } else {
                return false;
            }
        }

        public void Add(int nodeID)
        {
            if (! Nodes.ContainsKey(nodeID)) {
                Nodes.Add(nodeID, new List<int>());
            }
        }

        public void Add(int nodeID, int edgeID)
        {
            if (Nodes.ContainsKey(nodeID)) {
                Nodes[nodeID].Add(edgeID);
            } else {
                Nodes.Add(nodeID, new List<int>(){edgeID});
            }
        }

        public void Remove(int nodeID)
        {
            if (Nodes.ContainsKey(nodeID)) {
                Nodes[nodeID] = new List<int>();
            }
        }

        public void Remove(int nodeID, int edgeID)
        {
            if (Nodes.ContainsKey(nodeID)) {
                Nodes[nodeID].Remove(edgeID);
            }
        }

        public List<int> Get(int nodeID)
        {
            if (Nodes.ContainsKey(nodeID)) {
                return Nodes[nodeID];
            }

            return null;
        }

        public List<List<int>> GetAll()
        {
            return (Nodes != null && Nodes.Count > 0) ? Nodes.Select(x => x.Value).ToList() : null;
        }

        public List<int> GetKeys()
        {
            return (Nodes != null && Nodes.Count > 0) ? Nodes.Select(x => x.Key).ToList() : null;
        }

        public int Count(int nodeID)
        {
            if (Nodes.ContainsKey(nodeID)) {
                return Nodes[nodeID].Count;
            }

            return 0;
        }

        /*
         * Going to return new instance that only has one edge id per node connection without duplicates
         */
        public AdjacencyList GetWithoutDuplicateEdges()
        {
            AdjacencyList AdjacencyListWithoutDuplicates = new AdjacencyList();

            List<int> keys = GetKeys();

            for (int i = 0; i < keys.Count; i++) {

                int currentNodeID = keys[i];
                List<int> connectedNodeIDs = Get(keys[i]);
                AdjacencyListWithoutDuplicates.Add(currentNodeID);

                for (int j = 0; j < connectedNodeIDs.Count; j++) {
                    if (AdjacencyListWithoutDuplicates.Contains(currentNodeID, connectedNodeIDs[j]) || AdjacencyListWithoutDuplicates.Contains(connectedNodeIDs[j], currentNodeID)) {
                        //do nothing for now
                    } else {
                        AdjacencyListWithoutDuplicates.Add(currentNodeID, connectedNodeIDs[j]);
                    }
                    
                }
            }

            return AdjacencyListWithoutDuplicates;
        }

        public void Clear()
        {
            Nodes = new Dictionary<int, List<int>>();
        }

        public void Log()
        {
            foreach (KeyValuePair<int, List<int>> entry in Nodes) {
                string data = "ID: " + entry.Key;
                for (int i = 0; i < Nodes[entry.Key].Count; i++) {
                    data += ", " + Nodes[entry.Key][i];
                }
                Debug.Log(data);
            }
            
        }
    }

}