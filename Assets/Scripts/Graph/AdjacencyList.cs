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

        public int Count(int nodeID)
        {
            if (Nodes.ContainsKey(nodeID)) {
                return Nodes[nodeID].Count;
            }

            return 0;
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