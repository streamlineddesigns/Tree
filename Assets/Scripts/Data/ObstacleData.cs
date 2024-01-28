using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace StudioByStorm.Data {

    [System.Serializable]
    public struct ObstacleData
    {
        public string name;
        public AssetReference assetReference;
        public ObstacleType obstacleType;
        public int obstacleCount;
        public float difficultyScore;
    }

}