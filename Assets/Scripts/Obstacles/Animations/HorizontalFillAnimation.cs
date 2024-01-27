using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace StudioByStorm.Obstacles.Animations {

    public class HorizontalFillAnimation : Animation
    {
        [Range(0, 1)] public float fillAmount = 0.5f;
        public Material[] horizontalFillMaterials;
        
        protected override IEnumerator AnimationUpdate()
        {
            while(animate) {
                for (int i = 0; i < horizontalFillMaterials.Length; i++) {
                    horizontalFillMaterials[i].SetFloat("_FillAmount", fillAmount);
                    horizontalFillMaterials[i].SetInt("_FillDirection", direction);
                }

                if (fillAmount < 0.222f) {
                    fillAmount += 0.00555f;
                } else {
                    direction = (direction == 0) ? 1 : 0;
                    fillAmount = 0.0f;
                }

                if (direction == 0 && fillAmount >= 0.00555f || direction == 1 && fillAmount >= 0.111f) gameObject.transform.Rotate(0.0f, 0.0f, -2.0f);

                yield return new WaitForSeconds(0.0333f);
            }
        }

    }

}