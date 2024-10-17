using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StudioByStorm.Data {

    [System.Serializable]
    public class ProgressData
    {
        /*
         * Key is string(LevelPackName + "-" + ChapterID + "-" + LevelID)
         * Value is how many stars were received
         */
        public Dictionary<string, int> levelProgress = new Dictionary<string, int>();

        /*
         * Key is string(LevelPackName + "-" + ChapterID + "-" + LevelID)
         * Value is how much xp was received
         */
        public Dictionary<string, int> levelXPProgress = new Dictionary<string, int>();

        /*
         * Key is string(LevelPackName + "-" + ChapterID + "-" + CutSceneID)
         * Value is just a boolean for if the cut scene was watched or not
         */
        public Dictionary<string, bool> cutSceneProgress = new Dictionary<string, bool>();

        /*
         * Key is int(LevelPackName + "-" + ChapterID)
         * Value is highest level unlocked for that chapter
         */
        public Dictionary<string, int> unlockedLevelProgress = new Dictionary<string, int>();

        /*
         * Key is int(LevelPackName + "-" + ChapterID)
         * Value is highest cut scene unlocked for that chapter
         */
        public Dictionary<string, int> unlockedCutSceneProgress = new Dictionary<string, int>();

        //returns the highest tutorial ID completed
        public int tutorialProgress = -1;

        //returns the most recently played chapter
        public int mostRecentlyPlayedChapterIDProgress = -1;

        //returns player age
        public int playerAge = -1;

        //returns if a particular video hint has been shown or not
        public Dictionary<VideoHintName, bool> videoHintProgress = new Dictionary<VideoHintName, bool>();
    }

}