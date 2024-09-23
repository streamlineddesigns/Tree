using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.SceneManagement;
using GameAnalyticsSDK;
using StudioByStorm.Data.LevelChapters;

namespace StudioByStorm.UI.Controllers {

    public class LevelCompleteController : Controller
    {
        public static Dictionary<int, LevelCompleteController> instances = new  Dictionary<int, LevelCompleteController>();
        public TMP_Text headingText;
        public TMP_Text levelText;
        public Image[] starImages;
        public Slider xpSlider;
        public TMP_Text earnedXPText;
        public TMP_Text remainingXPText;
        private int framesToWait = 2;
        private float[] percentOfLevelCompletedToStarTierMapping = new float[4]{0.0f, 0.0f, 0.75f, 1.0f};

        protected void Awake()
        { 
            gameObject.transform.parent = null;
            DontDestroyOnLoad(gameObject);
        }

        protected void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            base.OnEnable();
        }

        protected void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            base.OnDisable();
        }

        protected void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "Main") {
                StartCoroutine(InstanceCleanUpCheck());
            }
        }

        IEnumerator InstanceCleanUpCheck()
        {
            yield return StartCoroutine(WaitForFrames(framesToWait * 2));

            if (LevelCompleteController.instances.ContainsKey(gameObject.GetInstanceID())) {
                LevelCompleteController.instances.Remove(gameObject.GetInstanceID());
                Destroy(gameObject);
            } else {
                LevelCompleteController.instances.Add(gameObject.GetInstanceID(), this);
                //re-run on enable to ensure it gets added to the registry
                base.OnEnable();
            }
        }

        public void Show()
        {
            StartCoroutine(ShowRoutine());
        }

        IEnumerator ShowRoutine() 
        {
            yield return null;

            int currentLevelID = GameManager.Singleton.LevelManager.currentLevelID;
            int currentChapterID = GameManager.Singleton.LevelManager.currentChapterID;
            //handles progress
            string key = (currentChapterID + "-" + currentLevelID);

            int currentLevelEdgeCount = GameManager.Singleton.LevelManager.currentLevelEdgeCount;
            int totalEdgeCount = (GameManager.Singleton.LevelManager.currentLevelNodeCount - (GameManager.Singleton.LevelManager.currentLevelParentCount / 2));
            float edgePercent = (currentLevelEdgeCount * 1.0f) / (totalEdgeCount * 1.0f);
            
            //Debug.Log("currentLevelEdgeCount: " + currentLevelEdgeCount);
            //Debug.Log("totalEdgeCount: " + totalEdgeCount);
            //Debug.Log("edgePercent: " + edgePercent);

            //map the percentage of the level beaten to the stars awarded to the player
            int starsAwarded = (edgePercent >= percentOfLevelCompletedToStarTierMapping[3]) ? 3 : 
                               (edgePercent >= percentOfLevelCompletedToStarTierMapping[2]) ? 2 : 
                               (edgePercent >= percentOfLevelCompletedToStarTierMapping[1]) ? 1 : 0;
            int previousStarsAwarded = GameManager.Singleton.ProgressManager.GetLevelProgress(key);
            
            //get cells used for xp purposes
            int totalCellsUsed = 0;
            int totalCellsAvailable = GameManager.Singleton.LevelManager.currentLevelNodeCount;
            List<List<Node>> connectedNodes = GameManager.Singleton.ColorNodeRegistry.getAllAsList();
            for (int i = 0; i < connectedNodes.Count; i++) {
                for (int j = 0; j < connectedNodes[i].Count; j++) {
                    totalCellsUsed++;
                }
            }
            int previousCellsAwarded = GameManager.Singleton.ProgressManager.GetLevelXPProgress(key);
            
            //save the progress
            SaveProgress(key, currentChapterID, currentLevelID, starsAwarded, previousStarsAwarded, totalCellsUsed, previousCellsAwarded);
            AnalyticsManager.NewProgressionEvent(GAProgressionStatus.Complete, GameManager.Singleton.LevelManager.displayChapterID, GameManager.Singleton.LevelManager.displayLevelID, starsAwarded);

            headingText.text = GameManager.Singleton.LevelManager.levelChapters.chapters[currentChapterID].heading;
            levelText.text = GameManager.Singleton.LevelManager.romanNumerals[currentLevelID + 1];

            yield return new WaitForSeconds(2.0f);
            //display the star/xp animations
            StartCoroutine(SetXPProgress(previousCellsAwarded, totalCellsUsed, totalCellsAvailable));
            StartCoroutine(SetStarProgress(previousStarsAwarded, starsAwarded, GameManager.Singleton.LevelManager.levelCompletionColor));
            //close cells used view
            GameManager.Singleton.UIController.Close(ViewName.CellsUsedView);

            //show the level complete view
            View levelCompleteView = GameManager.Singleton.ViewRegistry.TryGetValue(ViewName.LevelCompleteView) as View;
            //levelCompleteView.transform.DOScale(new Vector3(0.1f, 0.1f, 0.1f), 0.0f);
            GameManager.Singleton.UIController.ShowView(ViewName.LevelCompleteView);
            //levelCompleteView.transform.DOScale(new Vector3(1.0f, 1.0f, 1.0f), 0.5f).SetEase(Ease.InQuad);
        }

        IEnumerator SetXPProgress(int previousCellsAwarded, int used, int available)
        {
            bool isPreviousAwardedLarger = (previousCellsAwarded >= used);

            float usedPercent = (isPreviousAwardedLarger) ? (previousCellsAwarded * 1.0f) / (available * 1.0f) : (used * 1.0f) / (available * 1.0f);
            int earnedPercentInt = (int) (usedPercent * 100.0f);
            int remainingPercentInt = 100 - earnedPercentInt;

            float easedUsedPercent = 0.0f;
            float easedEarnedPercent = 0.0f;
            float easedRemainingPercentInt = 100.0f;

            float duration = 1.0f;
            float timer = 0.0f;

            if (! isPreviousAwardedLarger) {
                while(easedUsedPercent < usedPercent) {
                    timer += Time.deltaTime;
                    float percent = timer / duration;
                    float cappedPercent = Mathf.Min(percent, 1.0f);
                    easedUsedPercent = DOVirtual.EasedValue(0, usedPercent, cappedPercent, Ease.InOutQuad);
                    easedEarnedPercent = (int) DOVirtual.EasedValue(0, earnedPercentInt, cappedPercent, Ease.InOutQuad);
                    easedRemainingPercentInt = (int) DOVirtual.EasedValue(100.0f, remainingPercentInt, cappedPercent, Ease.InOutQuad);

                    xpSlider.value = easedUsedPercent;
                    earnedXPText.text = easedEarnedPercent.ToString() + "%";
                    remainingXPText.text = easedRemainingPercentInt.ToString() + "%";
                    yield return null;
                }
            } else {
                xpSlider.value = usedPercent;
                earnedXPText.text = earnedPercentInt.ToString() + "%";
                remainingXPText.text = remainingPercentInt.ToString() + "%";
            }

            yield return null;
        }

        IEnumerator SetStarProgress(int previousStarsAwarded, int completionValue, Color completionColor)
        {
            bool isPreviousAwardedLarger = (previousStarsAwarded >= completionValue);
            float originalScale = starImages[0].gameObject.transform.localScale.x;
            float targetScale = originalScale * 1.25f;

            if (! isPreviousAwardedLarger) {
                for (int i = 0; i < starImages.Length; i++) {
                    if (completionValue > i) {
                        starImages[i].gameObject.transform.DOScale(targetScale, 0.1665f).OnComplete(() => {
                            starImages[i].color = completionColor;
                            starImages[i].gameObject.transform.DOScale(originalScale, 0.1665f);
                        });
                        yield return new WaitForSeconds(0.333f);
                    }
                }
            } else {
                for (int i = 0; i < starImages.Length; i++) {
                    if (completionValue > i) {
                        starImages[i].color = completionColor;
                    }
                }
            }

            yield return null;
        }

        private void SaveProgress(string key, int currentChapterID, int currentLevelID, int starsAwarded, int previousStarsAwarded, int cellsUsed, int previousCellsAwarded)
        {
            bool needsToSave = false;

            if (starsAwarded > previousStarsAwarded) {
                GameManager.Singleton.ProgressManager.UpdateLevel(key, starsAwarded);
                needsToSave = true;
            }

            if (cellsUsed > previousCellsAwarded) {
                GameManager.Singleton.ProgressManager.UpdateLevelXP(key, cellsUsed);
                needsToSave = true;
            }

            //handle unlock progress
            int highestLevelCompletedForCurrentChapter = GameManager.Singleton.ProgressManager.GetUnlockedLevelProgress(currentChapterID);

            if (currentLevelID > highestLevelCompletedForCurrentChapter) {
                GameManager.Singleton.ProgressManager.UpdateUnlockedLevel(currentChapterID, currentLevelID);
                needsToSave = true;
            }

            //save
            if (needsToSave) {
                GameManager.Singleton.ProgressManager.Save();
            }
        }

        public void RestartButtonClick()
        {
            AudioManager.Singleton.Play(SoundType.ButtonPress);

            int currentLevelID = GameManager.Singleton.LevelManager.currentLevelID;
            int currentChapterID = GameManager.Singleton.LevelManager.currentChapterID;

            StartCoroutine(GoToNextLevel(currentLevelID, currentChapterID));
        }

        public void ContinueButtonClick()
        {
            AudioManager.Singleton.Play(SoundType.ButtonPress);

            //check what our current level is
            int currentLevelID = GameManager.Singleton.LevelManager.currentLevelID;
            int currentChapterID = GameManager.Singleton.LevelManager.currentChapterID;
            Chapter currentChapter = GameManager.Singleton.LevelManager.levelChapters.chapters[currentChapterID];
            
            //check if it's equal to our chapters last level ID. ie should play next chapter
            int lastLevelIDForCurrentChapter = currentChapter.levels.Count - 1;
            int lastAvailableLevelIDForCurrentChapter = (GameManager.Singleton.LevelManager.initiallyAvailableLevelRowsPerChapter * 3) - 1;
            int lastLevelIDToUse = (lastLevelIDForCurrentChapter < lastAvailableLevelIDForCurrentChapter) ? lastLevelIDForCurrentChapter : lastAvailableLevelIDForCurrentChapter;

            int nextChapterID = currentChapterID + 1;

            if (currentLevelID >= lastLevelIDToUse) {
                //check if we're all ready on the last chapter. ie beaten
                int lastChapterID = GameManager.Singleton.LevelManager.levelChapters.chapters.Count - 1;
                if (currentChapterID >= lastChapterID) {
                    //Debug.LogError("last level beaten");
                    HomeButtonClick();
                } else {
                    StartCoroutine(GoToNextChapter(nextChapterID));
                }
                return;
            }

            //check if next level ID is divisble by 3 with no remainder. ie should play cut scene instead
            int nextLevelID = currentLevelID + 1;
            if (nextLevelID % 3 == 0) {
                StartCoroutine(GoToNextCutScene(nextLevelID, currentChapterID));
                return;
            }

            //otherwise, just send to next level
            StartCoroutine(GoToNextLevel(nextLevelID, currentChapterID));
        }

        public void HomeButtonClick()
        {
            AudioManager.Singleton.Play(SoundType.ButtonPress);
            
            PauseController PauseController = GameManager.Singleton.ControllerRegistry.TryGetValue(ViewName.PauseView) as PauseController;
            PauseController.HomeButtonClick();
        }

        IEnumerator GoToNextChapter(int nextChapterID)
        {
            //Debug.Log("GoToNextChapter");
            HomeButtonClick();
            yield return new WaitUntil(() => SceneManager.GetActiveScene().name == "Main");
            yield return StartCoroutine(WaitForFrames(framesToWait));

            int nextLevelID = 0;
            int currentCutSceneID = nextLevelID / 3;
            CutSceneController CutSceneController = GameManager.Singleton.ControllerRegistry.TryGetValue(ViewName.CutSceneView) as CutSceneController;
            CutSceneController.ShowCutScene(currentCutSceneID, nextChapterID);
        }

        IEnumerator GoToNextCutScene(int nextLevelID, int chapterID)
        {
            //Debug.Log("GoToNextCutScene");
            HomeButtonClick();
            yield return new WaitUntil(() => SceneManager.GetActiveScene().name == "Main");
            yield return StartCoroutine(WaitForFrames(framesToWait));

            int currentCutSceneID = nextLevelID / 3;
            CutSceneController CutSceneController = GameManager.Singleton.ControllerRegistry.TryGetValue(ViewName.CutSceneView) as CutSceneController;
            CutSceneController.ShowCutScene(currentCutSceneID, chapterID);
        }

        IEnumerator GoToNextLevel(int nextLevelID, int chapterID)
        {
            //Debug.Log("GoToNextLevel");
            HomeButtonClick();
            yield return new WaitUntil(() => SceneManager.GetActiveScene().name == "Main");
            yield return StartCoroutine(WaitForFrames(framesToWait));
            
            LevelSelectController LevelSelectController = GameManager.Singleton.ControllerRegistry.TryGetValue(ViewName.LevelSelectView) as LevelSelectController;
            LevelSelectController.LevelSelectButtonClick(nextLevelID, chapterID);
        }

        IEnumerator WaitForFrames(int frameCount)
        {
            for (int i = 0; i < frameCount; i++) {
                yield return null;
            }
        }

    }

}