using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StudioByStorm.Data;

namespace StudioByStorm.Repositories {
    
    [CreateAssetMenu(menuName = "StudioByStorm/Repositories/ObstacleDataRepository")]
    public class ObstacleDataRepository : ScriptableObject
    {
        public List<ObstacleData> data = new List<ObstacleData>();
    }

}