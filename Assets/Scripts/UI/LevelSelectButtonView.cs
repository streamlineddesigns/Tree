using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace StudioByStorm.UI {

    public class LevelSelectButtonView : View
    {
        public int ID;
        public int ChapterID;
        public Text[] levelNumberText;
        public Image[] progressImages;

        public void SetProgress(int completionValue, Color completionColor)
        {
            for (int i = 0; i < progressImages.Length; i++) {
                if (completionValue > i) {
                    progressImages[i].color = completionColor;
                }
            }
        }
    }

}