using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace StudioByStorm.Data {

    [System.Serializable]
    public struct VideoHintData {
        public VideoClip clip;
        public VideoHintName name;
        public string[] messageTranslations;
    }

}