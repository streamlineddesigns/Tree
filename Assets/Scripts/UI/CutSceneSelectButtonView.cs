using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace StudioByStorm.UI {

    public class CutSceneSelectButtonView : View
    {
        public int ID;
        public int ChapterID;
        public GameObject unlockedContainer;
        public GameObject lockContainer;
        public Image progressImage;
        public bool IsLocked;

        public void SetLockStatus(bool isLocked)
        {
            IsLocked = isLocked;

            if (IsLocked) {
                unlockedContainer.SetActive(false);
                lockContainer.SetActive(true);
            } else {
                unlockedContainer.SetActive(true);
                lockContainer.SetActive(false);
            }
        }

        public bool GetLockStatus()
        {
            return IsLocked;
        }

        public void SetProgress(bool completionValue, Color completionColor)
        {
            if (completionValue) {
                progressImage.color = completionColor;
            }
        }
    }

}