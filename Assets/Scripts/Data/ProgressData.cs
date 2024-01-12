using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StudioByStorm.Data {

    [System.Serializable]
    public class ProgressData
    {
        /*
         * Key is string(ChapterID + "-" + LevelID)
         * Value is how many stars were received
         */
        public Dictionary<string, int> levelProgress = new Dictionary<string, int>();

        /*
         * Key is string(ChapterID + "-" + CutSceneID)
         * Value is just a boolean for if the cut scene was watched or not
         */
        public Dictionary<string, bool> cutSceneProgress = new Dictionary<string, bool>();
    }

}