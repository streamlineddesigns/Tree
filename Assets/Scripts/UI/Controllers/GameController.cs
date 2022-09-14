using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StudioByStorm.UI.Controllers {

    public class GameController : Controller
    {
        public void PauseButtonClick()
        {
            Time.timeScale = 0.0f;
            GameManager.Singleton.UIController.ShowView(ViewName.PauseView);
        }
    }

}