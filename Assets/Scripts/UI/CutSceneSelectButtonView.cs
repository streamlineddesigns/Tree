using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace StudioByStorm.UI {

    public class CutSceneSelectButtonView : View
    {
        public int ID;
        public int ChapterID;
        public Image progressImage;

        public void SetProgress(bool completionValue, Color completionColor)
        {
            if (completionValue) {
                progressImage.color = completionColor;
            }
        }
    }

}