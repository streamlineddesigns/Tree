using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace StudioByStorm.Obstacles.Animations {

    public class WaypointPartialPathAnimation : Animation
    {
        public GameObject[] targetGameObjects;
        protected Vector3[] targetPositions;
        protected int[] targetPositionIndexs;

        protected void Awake()
        {
            base.Awake();
            targetPositions = new Vector3[targetGameObjects.Length];
            targetPositionIndexs = new int[targetGameObjects.Length];
        }

        protected void Start()
        {
            base.Start();

            for (int i = 0; i < targetGameObjects.Length; i++) {
                targetPositions[i] = targetGameObjects[i].transform.localPosition;
                targetPositionIndexs[i] = i;
            }
        }

        protected void OnEnable()
        {
            StartCoroutine(DelayedOnEnable());
        }

        IEnumerator DelayedOnEnable()
        {
            yield return null;

            for (int i = 0; i < buildingBlocks.Length; i++) {
                int buildBlockTargetIndex = targetPositionIndexs[i];
                Vector3 targetPosition = targetPositions[buildBlockTargetIndex];
                buildingBlocks[i].transform.localPosition = targetPosition;
            }
        }

        protected override IEnumerator AnimationUpdate()
        {
            yield return new WaitUntil(() => animate);

            while(animate) {
                
                for (int i = 0; i < targetPositionIndexs.Length; i++) {
                    int currentIndex = targetPositionIndexs[i];
                    targetPositionIndexs[i] = (currentIndex + 1 <= targetPositionIndexs.Length - 1) ? currentIndex + 1 : 0;
                }

                for (int j = 0; j < buildingBlocks.Length; j++) {
                    int buildBlockTargetIndex = targetPositionIndexs[j];
                    Vector3 targetPosition = targetPositions[buildBlockTargetIndex];
                    buildingBlocks[j].transform.DOLocalMove(targetPosition, time, false).SetEase(easing);
                }

                bool isMoving = true;

                while (isMoving) {
                    bool isAnyMoving = false;
                    for (int k = 0; k < buildingBlocks.Length; k++) {
                        int buildBlockTargetIndex = targetPositionIndexs[k];
                        Vector3 targetPosition = targetPositions[buildBlockTargetIndex];
                        if (buildingBlocks[k].transform.localPosition != targetPosition) {
                            isAnyMoving = true;
                        }
                    }

                    isMoving = isAnyMoving;
                    yield return new WaitForSeconds(0.0333f);
                }
                
            }
        }

    }

}