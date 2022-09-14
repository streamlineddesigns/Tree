using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StudioByStorm.UI.Controllers {

    public class PauseController : Controller
    {
        public GameObject nodeParent;
        public GameObject MockNodes; 

        public void HomeButtonClick()
        {
            Time.timeScale = 1.0f;
            MockNodes.SetActive(true);
            for (int i = 0; i < nodeParent.transform.childCount; i++) {
                nodeParent.transform.GetChild(i).gameObject.SetActive(false);//$$use pooling system so this is worthwhile
            }
            GameManager.Singleton.player.SetActive(false);
            GameManager.Singleton.UIController.ShowView(ViewName.StartView);
        }

        public void ResumeButtonClick()
        {
            Time.timeScale = 1.0f;
            GameManager.Singleton.UIController.ShowView(ViewName.GameView);
        }
    }

}