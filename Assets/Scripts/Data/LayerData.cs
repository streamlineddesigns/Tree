using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StudioByStorm.Data {

    [System.Serializable]
    public class LayerData
    {
        public int nodeCount;
        public List<VectorData> nodePositions;
        public List<NodeColor> nodeColors;
        public List<NodeType> nodeTypes;
    }

}