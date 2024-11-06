using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StudioByStorm.Data.LevelChapters {

    [System.Serializable]
    public struct Chapter
    {
        public string[] headingTranslations;
        public string[] subHeadingTranslations;
        public List<CutScene> cutScenes;
        public List<Level> levels;
        public List<AudioClip> narration;
        public AudioClip music;
    }

}