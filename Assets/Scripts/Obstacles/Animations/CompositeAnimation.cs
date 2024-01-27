using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace StudioByStorm.Obstacles.Animations {

    public class CompositeAnimation : MonoBehaviour
    {
        public Animation[] animations;

        public void Animate() 
        {
            for (int i = 0; i < animations.Length; i++) {
                animations[i].Animate();
            }
        }
    }

}