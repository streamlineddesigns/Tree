using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StudioByStorm.Data {

    [System.Serializable]
    public struct VectorData
    {
        public float x;
        public float y;
        public float z;

        public VectorData (Vector3 position) {
            x = position.x;
            y = position.y;
            z = position.z;
        }
    }

}