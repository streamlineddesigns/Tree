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
        protected float smoothing = 1f;

        protected Vector3 offset;
        protected Vector3 originalPosition;

        protected Bounds bounds;
        public float sensitivity = 0.5f;
        public float smoothSpeed = 10f;
        private Vector3 currentVelocity;
        private Vector2 swipeStart;

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
                    break;
            }
        }

        IEnumerator GameStart()
        {
            yield return new WaitForSeconds(0.1f);
            List<GameObject> nodes = GameManager.Singleton.NodeRegistry.getAllAsList().Select(x => x.gameObject).ToList();
            bounds = ML.Math.ComputeAABB(nodes.Select(x => x.transform.position).ToList());
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
                Vector3 targetCamPos = (Vector3) GameManager.Singleton.nearbyNode.GetPosition() - offset;
                transform.position = Vector3.Lerp (transform.position, targetCamPos, smoothing * Time.deltaTime);
            }
        }

        private void SwipeDetection()
        {
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
    }

}