using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StudioByStorm.EventPublishers;

namespace StudioByStorm.UI.Controllers {

    public class StartController : MonoBehaviour
    {
        public GameObject MockNodes;

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