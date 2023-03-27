using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StudioByStorm.EventPublishers;
using DG.Tweening;

namespace StudioByStorm.UI.Controllers {

    public class GameController : Controller
    {
        public GameObject Centroid;
        protected bool isZooming = false;

        void OnEnable()
        {
            GameEventPublisher.OnStateChange += OnStateChange;
        }

        void OnDisable()
        {
            GameEventPublisher.OnStateChange -= OnStateChange;
        }

        public void PauseButtonClick()
        {
            Time.timeScale = 0.0f;
            GameManager.Singleton.UIController.ShowView(ViewName.PauseView);
        }

        public void ZoomButtonClick()
        {
            CameraController.centroid = Centroid.transform;
            GameManager.Singleton.CameraController.MoveToCentroid();
            GameManager.Singleton.CameraController.GetComponent<Camera>().DOOrthoSize(20, 1.0f);
            GameManager.Singleton.UIController.ShowView(ViewName.ZoomView);
        }

        public void OnStateChange(GameState state)
        {
            switch(state) {
                case GameState.GameStart :
                    GameStart();
                    break;

                case GameState.LevelComplete :
                    break;
            }
        }
        

        protected void GameStart()
        {
            Centroid.transform.position = GameManager.Singleton.LevelManager.CurrentLevelData.Centroid;
        }
    }

}