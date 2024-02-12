using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StudioByStorm.Data.LevelChapters {

    [System.Serializable]
    public struct Chapter
    {
        public string heading;
        public string subHeading;
        public List<CutScene> cutScenes;
        public List<Level> levels;
        public AudioClip music;
    }

}