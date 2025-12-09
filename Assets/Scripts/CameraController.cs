using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using StudioByStorm.Registries;
using StudioByStorm.Optimizations;
using StudioByStorm.ML.Clustering;
using DG.Tweening;
using StudioByStorm.EventPublishers;
using System.Linq;
 
namespace StudioByStorm {

    public class CameraController : MonoBehaviour {
        public static Transform centroid;
        public static float levelWaitTime = 0.0f;
        public int[] maxNodeDistance;
        public float[] maxNodeDistanceIndexToProjectionSize;
        public Camera Camera;
        public GameObject CameraHitBox;
        protected float smoothing = 1f;

        protected Vector3 offset;
        protected Vector3 originalPosition;

        protected Bounds bounds;
        public float sensitivity = 0.5f;
        public float smoothSpeed = 10f;
        private Vector3 currentVelocity;
        private Vector2 swipeStart;
        private bool IsLevelComplete = false;
        private bool isShaking;
        private bool isCameraMovementOkay;

        public void MoveToCentroid()
        {
            Vector3 targetCamPos = centroid.transform.position - offset;
            transform.DOMove(targetCamPos, 1.0f).SetEase(Ease.OutQuad);
        }

        void Start () {
            originalPosition = gameObject.transform.position;
            offset =  new Vector3(0,0,5);
        }

        void OnEnable()
        {
            GameEventPublisher.OnStateChange += OnStateChange;
        }

        void OnDisable()
        {
            GameEventPublisher.OnStateChange -= OnStateChange;
        }

        public void OnStateChange(GameState state)
        {
            switch(state) {
                case GameState.GameStart :
                    StartCoroutine(GameStart());
                    break;

                case GameState.LevelComplete :
                    IsLevelComplete = true;
                    break;
            }
        }

        IEnumerator GameStart()
        {
            yield return new WaitForSeconds(0.1f);
            List<GameObject> nodes = GameManager.Singleton.NodeRegistry.getAllAsList().Select(x => x.gameObject).ToList();
            bounds = ML.Math.ComputeAABB(nodes.Select(x => x.transform.position).ToList());

            List<GameObject> gos = GameManager.Singleton.NodeRegistry.getAllAsList().Select(x => x.gameObject).ToList();
            GameObject highestObject = gos.OrderByDescending(x => x.transform.position.y).FirstOrDefault();
            GameObject lowestObject = gos.OrderByDescending(x => x.transform.position.y).Reverse().FirstOrDefault();

            GameObject rightestObject = gos.OrderByDescending(x => x.transform.position.x).FirstOrDefault();
            GameObject leftestObject = gos.OrderByDescending(x => x.transform.position.x).Reverse().FirstOrDefault();

            Vector3 targetPosition = GameManager.Singleton.LevelManager.CurrentLevelData.Centroid - offset;

            Vector3 maxPosition = targetPosition;
            Vector3 minPosition = targetPosition;

            float verticalDistance = ML.Math.GetDistance(highestObject.transform.position.y, lowestObject.transform.position.y);
            float horizontalDistance = ML.Math.GetDistance(rightestObject.transform.position.x, leftestObject.transform.position.x);
            float usedDistance = 0; 

            if (verticalDistance > horizontalDistance) {
                maxPosition.y = highestObject.transform.position.y;
                minPosition.y = lowestObject.transform.position.y;
                //Debug.Log("Vertical Distance: " + verticalDistance);
                usedDistance = verticalDistance;
            } else {
                maxPosition.x = rightestObject.transform.position.x;
                minPosition.x = leftestObject.transform.position.x;
                //Debug.Log("Horizontal Distance: " + horizontalDistance);
                usedDistance = horizontalDistance;
            }

            if (false /*ML.Math.GetDistance(maxPosition, minPosition) > 20.0f*/) {
                Sequence levelDemo = DOTween.Sequence();
                    levelDemo.Append(transform.DOMove(maxPosition, 1.5f, false))
                             .Append(transform.DOMove(minPosition, 2.5f, false).SetEase(Ease.InOutCubic));
                CameraController.levelWaitTime = 4.0f;
                yield return new WaitForSeconds(4.0f);
            } else {
                CameraController.levelWaitTime = 0.0f;
            }


            Vector3 targetCamPos = GameManager.Singleton.LevelManager.CurrentLevelData.Centroid;
            transform.position = targetCamPos;

            int maxNodeDistanceIndex = 0;
            float minDistance = 1000.0f;

            for (int i = 0; i < maxNodeDistance.Length; i++) {
                if (Mathf.Abs(maxNodeDistance[i] - usedDistance) < minDistance) {
                    minDistance = Mathf.Abs(maxNodeDistance[i] - usedDistance);
                    maxNodeDistanceIndex = i;
                }
            }


            Camera.orthographicSize = maxNodeDistanceIndexToProjectionSize[maxNodeDistanceIndex];

            GameManager.Singleton.player.SetActive(true);

            yield return new WaitForSeconds(2.0f);

            isCameraMovementOkay = (GameManager.Singleton.PlayerController.controlType != ControlType.Slingshot);
            if (isCameraMovementOkay) {
                Camera.DOOrthoSize(20, 1.0f).SetEase(Ease.InSine).OnComplete(() => {
                    if (GameManager.Singleton.PlayerController.controlType == ControlType.Jump) CameraHitBox.SetActive(true);
                });
            }
        }
    
        private void OnDrawGizmos()
        {
            // Set the Gizmo color (you can change this to your preferred color)
            Gizmos.color = Color.green;

            // Draw the wireframe of the AABB using the bounds
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }
    
        void Update () {
            if (! GameManager.Singleton.player.activeSelf || GameManager.Singleton == null || GameManager.Singleton.nearbyNode == null) {
                return;
            }
            

            if (centroid != null) {
                SwipeDetection();
            } else {
                Vector3 targetCamPos = (Vector3) GameManager.Singleton.PlayerController.parentChildCentroid - offset;
                if (isCameraMovementOkay) transform.position = Vector3.Lerp (transform.position, targetCamPos, smoothing * Time.deltaTime);
            }
        }

        private void SwipeDetection()
        {
            if (IsLevelComplete) {
                return;
            }

            // Detect swipe input
            if (Input.GetMouseButtonDown(0)) {

                swipeStart = Input.mousePosition;

            } else if (Input.GetMouseButton(0)) {

                Vector2 swipeDelta = (Vector2)Input.mousePosition - swipeStart;

                Vector3 newPosition = transform.position - new Vector3(swipeDelta.x * sensitivity, swipeDelta.y * sensitivity, 0);

                // Clamp the new camera position within the bounds
                newPosition.x = Mathf.Clamp(newPosition.x, bounds.min.x, bounds.max.x);
                newPosition.y = Mathf.Clamp(newPosition.y, bounds.min.y, bounds.max.y);

                // Smoothly move the camera to the new position
                transform.position = Vector3.SmoothDamp(transform.position, newPosition, ref currentVelocity, smoothSpeed * Time.deltaTime);

                swipeStart = Input.mousePosition;
            }
        }

        public void ResetToOriginalPosition()
        {
            gameObject.transform.DOMove(originalPosition, 1.0f, false);
        }

        public void Shake(float intensity, float duration, float x = 0.0f, float y = 0.0f)
        {
            if (! isShaking) {
                isShaking = true;
                StartCoroutine(ShakeCoroutine(intensity, duration, x, y));
            }
        }

        private IEnumerator ShakeCoroutine(float intensity, float duration, float x = 0.0f, float y = 0.0f)
        {
            Vector3 originalPosition = gameObject.transform.localPosition;
            float elapsed = 0.0f;

            while (elapsed < duration)
            {
                float xOffset = (x != 0.0f) ? x * intensity : Random.Range(-1f, 1f) * intensity;
                float yOffset = (y != 0.0f) ? y * intensity : Random.Range(-1f, 1f) * intensity;

                gameObject.transform.localPosition = originalPosition + new Vector3(xOffset, yOffset, 0);

                elapsed += Time.deltaTime;

                yield return null;
            }

            isShaking = false;
        }

    }

}