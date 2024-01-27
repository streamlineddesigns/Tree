using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace StudioByStorm.Obstacles.Animations {

    public class SpinAnimation : Animation
    {
        protected override IEnumerator AnimationUpdate()
        {
            yield return new WaitUntil(() => animate);

            while(animate) {
                if (direction == 0) {
                    gameObject.transform.Rotate(0.0f, 0.0f, -2.0f);
                } else {
                    gameObject.transform.Rotate(0.0f, 0.0f, 2.0f);
                }
                
                yield return new WaitForSeconds(0.0333f);
                
            }
        }

    }

}