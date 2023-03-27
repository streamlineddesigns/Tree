using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StudioByStorm.Data {

    [System.Serializable]
    public struct LevelData
    {
        public List<LayerData> Layers;
        public List<List<int>> AdjacencyListData;
        public Vector3 PlayerStartPosition;
        public Vector3 Centroid;
    }

}