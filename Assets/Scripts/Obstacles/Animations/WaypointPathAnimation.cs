using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace StudioByStorm.Obstacles.Animations {

    public class WaypointPathAnimation : Animation
    {
        public bool isAutoLooping = true;

        protected override IEnumerator AnimationUpdate()
        {
            yield return new WaitUntil(() => animate);

            while(animate) {
                
                for (int i = 0; i < buildingBlocks.Length; i++) {
                    int buildBlockIndex = buildingBlockPositionIndexs[i];
                    buildingBlockPositionIndexs[i] = (buildBlockIndex + 1 <= buildingBlockPositionIndexs.Length - 1) ? buildBlockIndex + 1 : 0;
                    int updatedBuildBlockIndex = buildingBlockPositionIndexs[i];
                    Vector3 targetPosition = buildingBlockPositions[updatedBuildBlockIndex];

                    if (! isAutoLooping && updatedBuildBlockIndex == 0) {
                        buildingBlocks[i].SetActive(false);
                    } else if (! isAutoLooping) {
                        buildingBlocks[i].SetActive(true);
                    }
                    
                    buildingBlocks[i].transform.DOLocalMove(targetPosition, time, false).SetEase(easing);
                }

                bool isMoving = true;

                while (isMoving) {
                    bool isAnyMoving = false;
                    for (int j = 0; j < buildingBlocks.Length; j++) {
                        int buildBlockIndex = buildingBlockPositionIndexs[j];
                        Vector3 targetPosition = buildingBlockPositions[buildBlockIndex];
                        if (buildingBlocks[j].transform.localPosition != targetPosition) {
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