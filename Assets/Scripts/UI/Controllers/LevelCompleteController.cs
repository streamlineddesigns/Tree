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

            int currentLevelEdgeCount = GameManager.Singleton.LevelManager.currentLevelEdgeCount;
            int totalEdgeCount = (GameManager.Singleton.LevelManager.currentLevelNodeCount - (GameManager.Singleton.LevelManager.currentLevelParentCount / 2));
            float edgePercent = currentLevelEdgeCount * 1.0f / totalEdgeCount * 1.0f;
            
            //Debug.Log("currentLevelEdgeCount: " + currentLevelEdgeCount);
            //Debug.Log("totalEdgeCount: " + totalEdgeCount);
            //Debug.Log("edgePercent: " + edgePercent);

            //map the percentage of the level beaten to the stars awarded to the player
            int starsAwarded = (edgePercent >= percentOfLevelCompletedToStarTierMapping[3]) ? 3 : 
                               (edgePercent >= percentOfLevelCompletedToStarTierMapping[2]) ? 2 : 
                               (edgePercent >= percentOfLevelCompletedToStarTierMapping[1]) ? 1 : 0;

            SetProgress(starsAwarded, GameManager.Singleton.LevelManager.levelCompletionColor);
            SaveProgress(currentChapterID, currentLevelID, starsAwarded);
            AnalyticsManager.NewProgressionEvent(GAProgressionStatus.Complete, GameManager.Singleton.LevelManager.displayChapterID, GameManager.Singleton.LevelManager.displayLevelID, starsAwarded);

            headingText.text = GameManager.Singleton.LevelManager.levelChapters.chapters[currentChapterID].heading;
            levelText.text = GameManager.Singleton.LevelManager.romanNumerals[currentLevelID + 1];

            yield return new WaitForSeconds(2.0f);
            //close cells used view
            GameManager.Singleton.UIController.Close(ViewName.CellsUsedView);

            //show the level complete view
            View levelCompleteView = GameManager.Singleton.ViewRegistry.TryGetValue(ViewName.LevelCompleteView) as View;
            //levelCompleteView.transform.DOScale(new Vector3(0.1f, 0.1f, 0.1f), 0.0f);
            GameManager.Singleton.UIController.ShowView(ViewName.LevelCompleteView);
            //levelCompleteView.transform.DOScale(new Vector3(1.0f, 1.0f, 1.0f), 0.5f).SetEase(Ease.InQuad);
        }

        private void SetProgress(int completionValue, Color completionColor)
        {
            for (int i = 0; i < starImages.Length; i++) {
                if (completionValue > i) {
                    starImages[i].color = completionColor;
                }
            }
        }

        private void SaveProgress(int currentChapterID, int currentLevelID, int starsAwarded)
        {
            bool needsToSave = false;

            //handle star progress
            string key = (currentChapterID + "-" + currentLevelID);

            int previousStarsAwarded = GameManager.Singleton.ProgressManager.GetLevelProgress(key);

            if (starsAwarded > previousStarsAwarded) {
                GameManager.Singleton.ProgressManager.UpdateLevel(key, starsAwarded);
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