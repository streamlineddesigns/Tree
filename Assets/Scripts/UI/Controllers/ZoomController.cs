using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using TMPro;

namespace StudioByStorm.UI.Controllers {

    public class ZoomController : Controller
    {
        public TMP_Text sensitivityValue;
        public TMP_Text smoothingValue;
        public void ZoomOutButtonClick()
        {
            CameraController.centroid = null;
            GameManager.Singleton.CameraController.GetComponent<Camera>().DOOrthoSize(11, 1.0f);
            GameManager.Singleton.UIController.ShowView(ViewName.GameView);
        }

        public void OnSensitivityValueChange(float incoming)
        {
            GameManager.Singleton.CameraController.sensitivity = incoming;
            sensitivityValue.text = incoming.ToString();
        }

        public void OnSmoothingValueChange(float incoming)
        {
            GameManager.Singleton.CameraController.smoothSpeed = incoming;
            smoothingValue.text = incoming.ToString();
        }
    }

}