using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using StudioByStorm.Registries;
using StudioByStorm.Optimizations;
using StudioByStorm.ML.Clustering;
using DG.Tweening;
 
namespace StudioByStorm {

    public class CameraController : MonoBehaviour {
        protected static Transform target;
        protected float smoothing = 1f;
        protected Vector3 offset;
        protected Vector3 originalPosition;
    
        void Start () {
            originalPosition = gameObject.transform.position;
            offset =  new Vector3(0,0,5);
        }
    
        void Update () {
            if (! GameManager.Singleton.player.activeSelf || GameManager.Singleton == null || GameManager.Singleton.nearbyNode == null) {
                return;
            }
            Vector3 targetCamPos = (Vector3) GameManager.Singleton.nearbyNode.GetPosition() - offset;
            transform.position = Vector3.Lerp (transform.position, targetCamPos, smoothing * Time.deltaTime);
        }

        public void ResetToOriginalPosition()
        {
            gameObject.transform.DOMove(originalPosition, 1.0f, false);
        }
    }

}