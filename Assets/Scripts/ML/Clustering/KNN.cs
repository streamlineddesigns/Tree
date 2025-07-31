using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StudioByStorm.Optimizations;

namespace StudioByStorm.ML.Clustering {

    /*
     * A class for finding the nearest neighbors in a given dataset
     */
    public class KNN
    {
        /*
         * @param  neighbors        : The entire dataset to search
         * @param  searchIndex : The index in the dataset, whose neighbors, will be returned
         * @param  k           : The number of neighbors to return
         * @return int[]       : The index's of the k nearest neighbors in the dataset
         */
        public static List<GameObject> GetKNearestNeighbors(GameObject searchAgainstObject, List<GameObject> neighbors, int k = 1)
        {
            Dictionary<int, float> Distances = new Dictionary<int, float>();
            List<int> nearestNeighborIndexs = new List<int>();
            List<GameObject> nearestNeighbors = new List<GameObject>();

            for (int i = 0; i < neighbors.Count; i++) {
                if (neighbors[i].GetInstanceID() == searchAgainstObject.GetInstanceID()) {
                    continue;
                }

                Distances.Add(i, Math.GetDistance(searchAgainstObject.transform.position, neighbors[i].transform.position));
            }

            nearestNeighborIndexs = Distances.OrderBy(x => x.Value).Select(x => x.Key).Take(k).ToList();

            for (int j = 0; j < nearestNeighborIndexs.Count; j++) {

                int index = nearestNeighborIndexs[j];
                nearestNeighbors.Add(neighbors[index]);   

            }


            return nearestNeighbors;
        }

        public static List<HashData> GetKNearestNeighbors(HashData searchAgainstObject, List<HashData> neighbors, int k)
        {
            Dictionary<int, float> Distances = new Dictionary<int, float>();
            List<int> nearestNeighborIndexs = new List<int>();
            List<HashData> nearestNeighbors = new List<HashData>();

            if (neighbors == null) {
                return null;
            }

            for (int i = 0; i < neighbors.Count; i++) {

                if (neighbors[i].gameObject.GetInstanceID() == searchAgainstObject.gameObject.GetInstanceID()) {
                    continue;
                }

                Distances.Add(i, Math.GetDistance(searchAgainstObject.GetPosition(), neighbors[i].GetPosition()));
            }

            nearestNeighborIndexs = Distances.OrderBy(x => x.Value).Select(x => x.Key).Take(k).ToList();

            for (int j = 0; j < nearestNeighborIndexs.Count; j++) {

                int index = nearestNeighborIndexs[j];
                nearestNeighbors.Add(neighbors[index]);   

            }


            return nearestNeighbors;
        }

        public static List<HashData> GetKNearestNeighbors(GameObject searchAgainstObject, List<HashData> neighbors, int k)
        {
            Dictionary<int, float> Distances = new Dictionary<int, float>();
            List<int> nearestNeighborIndexs = new List<int>();
            List<HashData> nearestNeighbors = new List<HashData>();

            if (neighbors == null) {
                return null;
            }

            for (int i = 0; i < neighbors.Count; i++) {

                if (neighbors[i].gameObject.GetInstanceID() == searchAgainstObject.GetInstanceID()) {
                    continue;
                }

                Distances.Add(i, Math.GetDistance(searchAgainstObject.transform.position, neighbors[i].GetPosition()));
            }

            nearestNeighborIndexs = Distances.OrderBy(x => x.Value).Select(x => x.Key).Take(k).ToList();

            for (int j = 0; j < nearestNeighborIndexs.Count; j++) {

                int index = nearestNeighborIndexs[j];
                nearestNeighbors.Add(neighbors[index]);   

            }


            return nearestNeighbors;
        }

        public static List<HashData> GetKNearestNeighbors(Vector2 position, List<HashData> neighbors, int k)
        {
            Dictionary<int, float> Distances = new Dictionary<int, float>();
            List<int> nearestNeighborIndexs = new List<int>();
            List<HashData> nearestNeighbors = new List<HashData>();

            if (neighbors == null) {
                return null;
            }

            for (int i = 0; i < neighbors.Count; i++) {

                Distances.Add(i, Math.GetDistance(position, neighbors[i].GetPosition()));
            }

            nearestNeighborIndexs = Distances.OrderBy(x => x.Value).Select(x => x.Key).Take(k).ToList();

            for (int j = 0; j < nearestNeighborIndexs.Count; j++) {

                int index = nearestNeighborIndexs[j];
                nearestNeighbors.Add(neighbors[index]);   

            }


            return nearestNeighbors;
        }
    }
    
}