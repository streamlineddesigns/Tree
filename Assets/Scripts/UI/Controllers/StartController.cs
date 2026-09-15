using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using StudioByStorm.EventPublishers;

namespace StudioByStorm.UI.Controllers {

    public class StartController : Controller
    {
        public GameObject MockNodes;

        public void OnEnable()
        {
            base.OnEnable();
            GameEventPublisher.OnViewChange += OnViewChange;
        }

        public void OnDisable()
        {
            base.OnDisable();
            GameEventPublisher.OnViewChange -= OnViewChange;
        }

        private void OnViewChange(ViewName currentViewName)
        {
            if (currentViewName == ViewName) {
                MockNodes.SetActive(true);
            } else {
                MockNodes.SetActive(false);
            }
        }

        public void StartLevelSelectButtonClick()
        {
            AudioManager.Singleton.Play(SoundType.ButtonPress);
            LevelPackSelectController levelPackSelectController = GameManager.Singleton.ControllerRegistry.TryGetValue(ViewName.LevelPackSelectView) as LevelPackSelectController;
            if (! levelPackSelectController.isRunning) StartCoroutine(levelPackSelectController.Show());
        }

        public void LevelCreatorButtonClick()
        {
            SceneManager.LoadScene("Graph");
        }

        public void PlayButtonClick()
        {
            AudioManager.Singleton.Play(SoundType.ButtonPress);
            GameManager.Singleton.UIController.ShowView(ViewName.FadeView);
            MockNodes.SetActive(false);
            GameManager.Singleton.UIController.ShowView(ViewName.GameView);
            GameEventPublisher.PublishGameStateChange(GameState.GameStart);
        }

        public void CharacterButtonClick()
        {
            AudioManager.Singleton.Play(SoundType.ButtonPress);
            GameManager.Singleton.UIController.ShowView(ViewName.CharacterSelectView);
        }

        public void ShopButtonClick()
        {
            AudioManager.Singleton.Play(SoundType.ButtonPress);
            GameManager.Singleton.UIController.ShowView(ViewName.ShopView);
        }

        public void SettingsButtonClick()
        {
            AudioManager.Singleton.Play(SoundType.ButtonPress);
            GameManager.Singleton.UIController.ShowView(ViewName.SettingsView);
        }

        public void NewsButtonClick()
        {
            AudioManager.Singleton.Play(SoundType.ButtonPress);
            GameManager.Singleton.UIController.ShowView(ViewName.NewsView);
        }

        public void AccountButtonClick()
        {
            AudioManager.Singleton.Play(SoundType.ButtonPress);
            GameManager.Singleton.UIController.ShowView(ViewName.AccountView);
        }

        public void CreditsButtonClick()
        {
            AudioManager.Singleton.Play(SoundType.ButtonPress);
            GameManager.Singleton.UIController.ShowView(ViewName.CreditsView);
        }
    }

}