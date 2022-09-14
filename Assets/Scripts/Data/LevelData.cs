using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StudioByStorm.Data {

    [System.Serializable]
    public class LevelData : ScriptableObject
    {
        public List<LayerData> Layers;
    }

}