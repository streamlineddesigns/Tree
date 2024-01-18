using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace StudioByStorm.UI {

    public class LevelSelectButtonView : View
    {
        public int ID;
        public int ChapterID;
        public GameObject starsContainer;
        public GameObject lockContainer;
        public Text[] levelNumberText;
        public Image[] progressImages;
        public bool IsLocked;

        public void SetLockStatus(bool isLocked)
        {
            IsLocked = isLocked;

            if (IsLocked) {
                starsContainer.SetActive(false);
                lockContainer.SetActive(true);
            } else {
                starsContainer.SetActive(true);
                lockContainer.SetActive(false);
            }
        }

        public bool GetLockStatus()
        {
            return IsLocked;
        }

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