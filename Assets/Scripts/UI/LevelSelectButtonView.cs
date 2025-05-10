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
        public Image outline;
        [SerializeField] private Color bossColor;
        public Text[] levelNumberText;
        public Image lockImage;
        public Image[] progressImages;
        public bool IsLocked;
        private bool isBoss;

        public void SetLockStatus(bool isLocked)
        {
            IsLocked = isLocked;

            if (IsLocked) {
                starsContainer.SetActive(false);
                lockContainer.SetActive(true);
                if (isBoss) {
                    lockImage.color = bossColor;
                }
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
                    progressImages[i].color = (isBoss) ? bossColor : completionColor;
                }
            }
        }

        public void BossCheck(int chapterID, int displayLevelID)
        {
            int previousLevels = chapterID * 15;
            int pos = ((previousLevels) + displayLevelID) % 6;
            pos = (pos == 0) ? 6 : pos;

            if (pos == 6) {
                isBoss = true;
                levelNumberText[1].color = bossColor;
                outline.color = bossColor;
            } else {
                isBoss = false;
            }
        }
    }

}