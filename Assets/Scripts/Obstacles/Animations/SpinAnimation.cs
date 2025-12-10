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

            float speed = 2.0f;

            if (GameManager.Singleton.PlayerController.controlType == ControlType.Animate) {
                speed = 4.0f;
            }

            while(animate) {
                if (direction == 0) {
                    gameObject.transform.Rotate(0.0f, 0.0f, -speed);
                } else {
                    gameObject.transform.Rotate(0.0f, 0.0f, speed);
                }

                /*float speed = (360.0f / time) / 30.0f;
                gameObject.transform.Rotate(0f, 0f, speed);*/

                
                yield return new WaitForSeconds(0.0333f);
                
            }
        }

    }

}