using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using StudioByStorm.EventPublishers;

namespace StudioByStorm.UI.Controllers {

    public class LevelLostController : Controller
    {
        public Text headingText;
        public Text levelText;

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

        public void HomeButtonClick()
        {
            LevelPackSelectController.bNeedsToCountStars = true;
            
            AudioManager.Singleton.Play(SoundType.ButtonPress);

            Time.timeScale = 1.0f;
            SceneManager.LoadScene("Main");
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