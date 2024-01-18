using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using StudioByStorm.Data.LevelChapters;

namespace StudioByStorm.UI.Controllers {

    public class LevelCompleteController : Controller
    {
        public static Dictionary<int, LevelCompleteController> instances = new  Dictionary<int, LevelCompleteController>();
        public TMP_Text headingText;
        public TMP_Text levelText;
        public Image[] starImages;
        private int framesToWait = 2;

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
            int currentLevelID = GameManager.Singleton.LevelManager.currentLevelID;
            int currentChapterID = GameManager.Singleton.LevelManager.currentChapterID;

            int starsAwarded = 3;
            SetProgress(starsAwarded, GameManager.Singleton.LevelManager.levelCompletionColor);
            SaveProgress(currentChapterID, currentLevelID, starsAwarded);

            headingText.text = GameManager.Singleton.LevelManager.levelChapters.chapters[currentChapterID].heading;
            levelText.text = GameManager.Singleton.LevelManager.romanNumerals[currentLevelID + 1];

            GameManager.Singleton.UIController.ShowView(ViewName);
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
            string key = (currentChapterID + "-" + currentLevelID);

            int previousStarsAwarded = GameManager.Singleton.ProgressManager.GetLevelProgress(key);

            if (starsAwarded > previousStarsAwarded) {
                GameManager.Singleton.ProgressManager.UpdateLevel(key, starsAwarded);
                GameManager.Singleton.ProgressManager.Save();
            }
        }

        public void ContinueButtonClick()
        {
            //check what our current level is
            int currentLevelID = GameManager.Singleton.LevelManager.currentLevelID;
            int currentChapterID = GameManager.Singleton.LevelManager.currentChapterID;
            Chapter currentChapter = GameManager.Singleton.LevelManager.levelChapters.chapters[currentChapterID];
            
            //check if it's equal to our chapters last level ID. ie should play next chapter
            int lastLevelIDForCurrentChapter = currentChapter.levels.Count - 1;
            int nextChapterID = currentChapterID + 1;

            if (currentLevelID >= lastLevelIDForCurrentChapter) {
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