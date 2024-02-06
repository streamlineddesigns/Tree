using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StudioByStorm.UI.Controllers {

    public class LevelLostController : Controller
    {
        public void HomeButtonClick()
        {
            Time.timeScale = 1.0f;
            SceneManager.LoadScene("Main");
        }

        public void RestartButtonClick()
        {
            Time.timeScale = 1.0f;
            LevelCompleteController levelCompleteController = GameManager.Singleton.ControllerRegistry.TryGetValue(ViewName.LevelCompleteView) as LevelCompleteController;
            levelCompleteController.RestartButtonClick();
        }
    }

}