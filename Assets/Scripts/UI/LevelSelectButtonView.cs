using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace StudioByStorm.UI {

    public class LevelSelectButtonView : View
    {
        public int ID;
        public int ChapterID;
        public GameObject playContainer;
        public GameObject starsContainer;
        public GameObject lockContainer;
        public Image outline;
        [SerializeField] private Color bossColor;
        public Text[] levelNumberText;
        public Image lockImage;
        public Image playImage;
        public Image[] progressImages;
        public bool IsLocked;
        private bool isBoss;
        private bool isHighestAvailableToSelect;

        protected void OnEnable()
        {
            if (isHighestAvailableToSelect) {
                StartCoroutine(FadeInOut());
            }
        }

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
                playImage.color = bossColor;
                outline.color = bossColor;
            } else {
                isBoss = false;
            }
        }

        public void SetToPlayIndication()
        {
            playContainer.SetActive(true);
            starsContainer.SetActive(false);
            lockContainer.SetActive(false);

            for (int i = 0; i < levelNumberText.Length; i++) {
                levelNumberText[i].gameObject.SetActive(false);
            }

            isHighestAvailableToSelect = true;
        }

        IEnumerator FadeInOut()
        {
            while(gameObject.activeSelf) {

                playContainer.transform.DOScale(0.6f, 0.0f);
                yield return null;

                playContainer.transform.DOScale(0.9f, 0.5f);
                yield return new WaitForSeconds(0.5f);

                playContainer.transform.DOScale(0.6f, 0.5f);
                yield return new WaitForSeconds(0.5f);
            }
        }
    }

}