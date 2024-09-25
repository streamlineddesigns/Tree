using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace StudioByStorm.Data {

    [System.Serializable]
    public struct VideoHintData {
        public int levelID;
        public int nodeID;
        public VideoClip clip;
        public VideoHintName name;
        public string message;
    }

}