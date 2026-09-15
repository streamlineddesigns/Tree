using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace StudioByStorm.Data.LevelPacks {

    [System.Serializable]
    public struct LevelPackProgressionData
    {
        public LevelPackName name;
        public GameObject lockedButton;
        public GameObject playButton;
        public GameObject lockedBanner;
        public Slider progressBar;
        public TMP_Text progressPercentText;
        public int starsToUnlock;
        public int chaptersCount;
        public int levelPerChapterCount;
    }

}