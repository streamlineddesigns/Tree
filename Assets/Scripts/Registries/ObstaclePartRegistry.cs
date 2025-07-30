using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StudioByStorm.Obstacles;

namespace StudioByStorm.Registries {

    public class ObstaclePartRegistry : Registry<int, List<ObstaclePart>>
    {        
        public void Add(int nodeID, ObstaclePart part)
        {
            if (! _Registry.ContainsKey(nodeID)) {
                List<ObstaclePart> parts = new List<ObstaclePart>();
                _Registry[nodeID] = parts;
                _Registry[nodeID].Add(part);
            } else {
                _Registry[nodeID].Add(part);
            }
        }
    }

}