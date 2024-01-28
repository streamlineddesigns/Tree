using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StudioByStorm.Data {
    
    [CreateAssetMenu(menuName = "StudioByStorm/RegistryData/ObstacleRegistryData")]
    public class ObstacleRegistryData : ScriptableObject
    {
        public List<ObstacleData> data = new List<ObstacleData>();
    }

}