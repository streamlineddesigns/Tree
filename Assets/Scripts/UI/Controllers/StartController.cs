using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using StudioByStorm.EventPublishers;

namespace StudioByStorm.UI.Controllers {

    public class StartController : Controller
    {
        public GameObject MockNodes;

        public void StartLevelSelectButtonClick()
        {
            GameManager.Singleton.UIController.ShowView(ViewName.LevelSelectView);
        }

        public void LevelCreatorButtonClick()
        {
            SceneManager.LoadScene("Graph");
        }

        public void PlayButtonClick()
        {
            MockNodes.SetActive(false);
            GameManager.Singleton.UIController.ShowView(ViewName.GameView);
            GameEventPublisher.PublishGameStateChange(GameState.GameStart);
            StartCoroutine(ActivatePlayer());
        }

        IEnumerator ActivatePlayer()
        {
            yield return 0;
            GameManager.Singleton.player.SetActive(true);
        }
    }

}