using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StudioByStorm.Data {

    /*
     * Used for organizing quantitative data about the obstacle's parts
     */
    [System.Serializable]
    public class ObstaclePartData
    {
        //ColorType : Max length/distance/perimeter between consecutive ObstacleParts of the same ColorType
        public Dictionary<NodeColor, float> maxColorDistance = new Dictionary<NodeColor, float>();
        //ColorType : Max count of consecutive ObstacleParts of the same ColorType
        public Dictionary<NodeColor, int> maxConsecutiveColorTypeCount = new Dictionary<NodeColor, int>();
    }

}