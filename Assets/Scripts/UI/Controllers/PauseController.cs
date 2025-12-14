using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using StudioByStorm.EventPublishers;

namespace StudioByStorm.UI.Controllers {

    public class PauseController : Controller
    {
        public GameObject nodeParent;
        public GameObject MockNodes; 

        public TMP_Text headingText;
        public TMP_Text levelText;

        protected void OnEnable()
        {
            base.OnEnable();
            GameEventPublisher.OnStateChange += OnStateChange;
        }

        protected void OnDisable()
        {
            base.OnDisable();
            GameEventPublisher.OnStateChange -= OnStateChange;
        }

        void OnStateChange(GameState state)
        {
            if (state == GameState.GameStart) {
                int currentLevelID = GameManager.Singleton.LevelManager.currentLevelID;
                int currentChapterID = GameManager.Singleton.LevelManager.currentChapterID;
                headingText.text = GameManager.Singleton.LevelManager.levelChapters.chapters[currentChapterID].headingTranslations[GameManager.Language];
                levelText.text = GameManager.Singleton.LevelManager.romanNumerals[currentLevelID + 1];
            }
        }

        public void HomeButtonClick(bool needsToCountStars = true)
        {
            LevelPackSelectController.bNeedsToCountStars = needsToCountStars;
            
            AudioManager.Singleton.Play(SoundType.ButtonPress);

            Time.timeScale = 1.0f;
            /*MockNodes.SetActive(true);
            GameManager.Singleton.CameraController.ResetToOriginalPosition();
            for (int i = 0; i < nodeParent.transform.childCount; i++) {
                nodeParent.transform.GetChild(i).gameObject.SetActive(false);//$$use pooling system so this is worthwhile
            }
            GameManager.Singleton.player.SetActive(false);
            GameManager.Singleton.UIController.ShowView(ViewName.StartView);*/

            SceneManager.LoadScene("Main");//$$Testing just a temporary fix. Not a permanent solution. The system that resets everything after a level is left isnt workinng so this should do it too
        }

        public void ResumeButtonClick()
        {
            AudioManager.Singleton.Play(SoundType.ButtonPress);

            Time.timeScale = 1.0f;
            GameManager.Singleton.UIController.ShowView(ViewName.GameView);
        }

        public void RestartButtonClick()
        {
            AudioManager.Singleton.Play(SoundType.ButtonPress);
            
            Time.timeScale = 1.0f;
            LevelCompleteController levelCompleteController = GameManager.Singleton.ControllerRegistry.TryGetValue(ViewName.LevelCompleteView) as LevelCompleteController;
            levelCompleteController.RestartButtonClick();
        }
    }

}