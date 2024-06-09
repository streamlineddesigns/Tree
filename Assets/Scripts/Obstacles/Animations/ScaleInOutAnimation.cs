using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace StudioByStorm.Obstacles.Animations {

    public class ScaleInOutAnimation : Animation
    {
        public float delayTilScale = 1.0f;
        [SerializeField] private float scaleMultiplier = 0.0f;
        protected Vector3 targetScale;
        protected Vector3 originalScale;
        protected bool bScaleSwitch;

        protected void Awake()
        {
            base.Awake();
            originalScale = buildingBlocks[0].transform.localScale;
        }

        protected override IEnumerator AnimationUpdate()
        {
            yield return new WaitUntil(() => animate);

            while(animate) {
                for (int i = 0; i < buildingBlocks.Length; i++) {
                    if (bScaleSwitch) {
                        targetScale = originalScale;
                    } else {
                        targetScale = originalScale * scaleMultiplier;
                    }

                    buildingBlocks[i].transform.DOScale(targetScale, time).SetEase(easing);
                }
                
                bool isScaling = true;

                while (isScaling) {
                    bool isAnyScaling = false;
                    for (int j = 0; j < buildingBlocks.Length; j++) {
                        if (buildingBlocks[j].transform.localScale != targetScale) {
                            isAnyScaling = true;
                        }
                    }

                    isScaling = isAnyScaling;
                    yield return new WaitForSeconds(0.0333f);
                }
                
                //teleport to next position if parts are not visible
                if (targetScale == originalScale * scaleMultiplier) {
                    TeleportBuildingBlocksPosition();
                }

                yield return new WaitForSeconds(delayTilScale);

                bScaleSwitch = (!bScaleSwitch);
            }
        }

    }

}