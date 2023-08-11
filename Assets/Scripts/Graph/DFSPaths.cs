using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StudioByStorm.Graph {

    public class DFSPaths
    {
        protected Dictionary<int, List<int>> Nodes;

        public DFSPaths(AdjacencyList adjacencyList)
        {
            Nodes = adjacencyList.GetRaw();
        }

        public void Search(int start, int end)
        {
            var paths = FindAllPaths(start, end);
            foreach (var path in paths)
            {
                Debug.Log(string.Join(" -> ", path));
            }
        }
        
        private List<List<int>> FindAllPaths(int start, int end)
        {
            List<List<int>> allPaths = new List<List<int>>();
            List<int> currentPath = new List<int>();
            HashSet<int> visited = new HashSet<int>();

            DFS(start, end, visited, currentPath, allPaths);

            return allPaths;
        }

        private void DFS(int current, int end, HashSet<int> visited, List<int> currentPath, List<List<int>> allPaths)
        {
            visited.Add(current);
            currentPath.Add(current);

            if (current == end)
            {
                // Note: This creates a new list with the contents of currentPath
                // This is to ensure each path in allPaths is independent.
                allPaths.Add(new List<int>(currentPath));
            }
            else
            {
                foreach (var neighbor in Nodes[current])
                {
                    if (!visited.Contains(neighbor))
                    {
                        DFS(neighbor, end, visited, currentPath, allPaths);
                    }
                }
            }

            // Backtrack
            visited.Remove(current);
            currentPath.RemoveAt(currentPath.Count - 1);
        }

    }

}