using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using StudioByStorm.UI.Controllers;
using StudioByStorm.Gravity.Player;
using StudioByStorm.EventPublishers;
using StudioByStorm.Obstacles.Animations;

namespace StudioByStorm.Helpers {

	public class TrailerHelper : MonoBehaviour
    {
        public Vector3 StartPosition;
        public Vector3 TargetPosition;
        public GameObject FourConnected;
        public GameObject NodeContainer;
        public GameObject ObstacleContainer;
        public GameObject Canvas;
        public bool enableCanvasAfter = false;
        public float SpinDuration;
        public Vector3 TargetEndRotation;
        public float ScaleTo;
        public EdgePlacementHelper EdgePlacementHelper;
        public GameController GameController;
        public CameraController CameraController;
        public PlayerController PlayerController;

        private bool isGameStarted = false;

        protected void OnEnable()
        {
            GameEventPublisher.OnStateChange += OnStateChange;
        }

        protected void OnDisable()
        {
            GameEventPublisher.OnStateChange -= OnStateChange;
        }

        public void OnStateChange(GameState state)
        {
            switch(state) {
                case GameState.GameStart :
                    StartCoroutine(GameStart());
                    break;
            }
        }

        IEnumerator GameStart()
        {
            yield return new WaitForSeconds(2.0f);

            isGameStarted = true;

            CameraController.enabled = false;
            
            yield return new WaitForSeconds(2.0f);

            CameraController.gameObject.transform.position = StartPosition;
            PlayerController.gameObject.SetActive(false);
            NodeContainer.SetActive(false);
            ObstacleContainer.SetActive(false);

    
            yield return new WaitForSeconds(2.0f);

            CameraController.gameObject.transform.DOMove(TargetPosition, 2.0f).SetEase(Ease.InOutQuad);

            yield return new WaitForSeconds(2.0f);

            List<SpinAnimation> spins = new List<SpinAnimation>();

            for (int i = 0; i < 15; i++) {
                SpinAnimation currentSpin = FourConnected.AddComponent<SpinAnimation>();
                spins.Add(currentSpin);
                spins[spins.Count - 1].Animate();
                yield return new WaitForSeconds(0.1f);
            }

            yield return new WaitForSeconds(2.0f);

            for (int j = 0; j < spins.Count; j++) {
                spins[j].Stop();
                yield return new WaitForSeconds(0.01f);
            }
            
            FourConnected.gameObject.transform.DORotate(TargetEndRotation, 0.25f);
            yield return new WaitForSeconds(0.5f);
            FourConnected.gameObject.transform.DOScale(ScaleTo, 1.0f);

            yield return new WaitForSeconds(0.5f);

            NodeContainer.SetActive(true);
            ObstacleContainer.SetActive(true);
            GameController.ZoomButtonClick();

            yield return new WaitForSeconds(1.0f);

            EdgePlacementHelper.LinearPlacement();

            yield return new WaitForSeconds(5.0f);

            if (enableCanvasAfter) Canvas.SetActive(true);
        }

        protected void Update()
        {
            if (isGameStarted && GameManager.Singleton != null && GameManager.Singleton.UIController.CurrentViewScreen != null && GameManager.Singleton.UIController.CurrentViewScreen.gameObject.activeSelf) {
                GameManager.Singleton.UIController.Close(GameManager.Singleton.UIController.CurrentViewScreen.ViewName);
            }
        }

    }

}