using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace StudioByStorm.Obstacles.Animations {

    public class CompositeAnimation : MonoBehaviour
    {
        public Animation[] animations;
        public List<int> nodeIDs;
        private bool isAnimating;

        protected void Start()
        {
            StartCoroutine(DelayedStart());
        }

        protected IEnumerator DelayedStart()
        {
            yield return null;
            
            for (int i = 0; i < nodeIDs.Count; i++) {
                GameManager.Singleton.CompositeAnimationRegistry.Add(nodeIDs[i], this);

                for (int j = 0; j < animations.Length; j++) {
                    animations[j].RegisterObstaclePartsViaNodeID(nodeIDs[i]);
                }
            }
        }

        protected void OnDisable()
        {
            for (int i = 0; i < nodeIDs.Count; i++) {
                GameManager.Singleton.CompositeAnimationRegistry.Remove(nodeIDs[i]);
            }
        }

        public void Animate() 
        {
            if (! isAnimating) {
                for (int i = 0; i < animations.Length; i++) {
                    animations[i].Animate();
                }
                isAnimating = true;
            }
            
        }

        public void Stop() 
        {
            if (isAnimating) {
                for (int i = 0; i < animations.Length; i++) {
                    animations[i].Stop();
                }
                isAnimating = false;
            }
        }
    }

}