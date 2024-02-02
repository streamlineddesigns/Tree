using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace StudioByStorm.Obstacles.Animations {

    public class CollapseAnimation : Animation
    {
        public GameObject[] targetGameObjects;
        public float strength = 0.05f;
        public int vibrato = 25;
        public float randomness = 25f;
        public int vibrateTilCollapseCount = 3;
        public float collapseTime = 0.25f;

        protected bool fadeOut = false;
        protected Vector3[] targetPositions;

        protected void Awake()
        {
            base.Awake();
            targetPositions = targetGameObjects.Select(x => x.transform.localPosition).ToArray();
        }

        protected override IEnumerator AnimationUpdate()
        {
            while(animate) {

                //multiple shakes                
                for (int i = 0; i < vibrateTilCollapseCount; i++) {
                    Shake(time);
                    yield return new WaitForSeconds(time);
                }
                
                //do movement to targets
                yield return StartCoroutine(Move(targetPositions, collapseTime));

                yield return new WaitForSeconds(collapseTime + 0.0333f);

                yield return StartCoroutine(Move(buildingBlockPositions, collapseTime));

                yield return new WaitForSeconds(4.0f);
            }
        }

        protected void Shake(float t)
        {
            for (int i = 0; i < buildingBlocks.Length; i++) {
                buildingBlocks[i].transform.DOShakePosition(t, strength, vibrato, randomness, fadeOut);
            }
        }

        IEnumerator Move(Vector3[] targets, float t)
        {
            for (int i = 0; i < buildingBlocks.Length; i++) {
                buildingBlocks[i].transform.DOLocalMove(targets[i], t, false).SetEase(easing);
            }

            //wait for movement to be done
            bool isMoving = true;
            while (isMoving) {
                bool isAnyMoving = false;
                for (int i = 0; i < buildingBlocks.Length; i++) {
                    if (buildingBlocks[i].transform.localPosition != targets[i]) {
                        isAnyMoving = true;
                    }
                }

                isMoving = isAnyMoving;
                yield return new WaitForSeconds(0.0333f);
            }
        }

    }

}