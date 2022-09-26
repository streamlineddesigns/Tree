using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StudioByStorm.Data {

    [System.Serializable]
    public struct LevelData
    {
        public List<LayerData> Layers;
        public List<List<int>> AdjacencyListData;
    }

}