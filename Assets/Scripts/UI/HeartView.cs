using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace StudioByStorm.UI {

    public class HeartView : View
    {
        [SerializeField] private Image heartImage;
        private Color onHeartColor;
        private Color offHeartColor;

        public void TurnOn(Color color)
        {
            onHeartColor = color;

            heartImage.color = onHeartColor;
        }

        public void TurnOff(Color color)
        {
            offHeartColor = color;

            heartImage.color = offHeartColor;
            StartCoroutine(animateColor(onHeartColor, offHeartColor));
        }

        IEnumerator animateColor(Color startColor, Color endColor)
        {
            Sequence sequenceAnimation = DOTween.Sequence();

            sequenceAnimation.Append(heartImage.DOColor(endColor, 0.075f))
                             .Append(heartImage.DOColor(startColor, 0.075f))
                             .Append(heartImage.DOColor(endColor, 0.075f))
                             .Append(heartImage.DOColor(startColor, 0.075f))
                             .Append(heartImage.DOColor(endColor, 0.075f));

            yield return null;
        }
    }

}