using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace StudioByStorm.FX {

    public class PlayerTrailFX : MonoBehaviour
    {
        public SpriteRenderer sprite; // List of sprites
        public float scaleOutDuration = 0.5f;
        public float originalScaleValue = 0.5f;
        public Color color;

        private void OnEnable()
        {
            ScaleOut();
            StartCoroutine(AutoDeactivate());
        }

        public void SetColor(Color c)
        {
            color = c;
        }

        // Method to trigger the explosion
        private void ScaleOut()
        {
            sprite.color = color;
            sprite.gameObject.transform.localScale = new Vector3(originalScaleValue, originalScaleValue, originalScaleValue);
            sprite.transform.DOScale(Vector3.zero, scaleOutDuration).SetEase(Ease.OutQuad);
            sprite.DOFade(0.0f, scaleOutDuration).SetEase(Ease.OutQuad);
        }

        IEnumerator AutoDeactivate()
        {
            yield return new WaitForSeconds(scaleOutDuration);
            gameObject.SetActive(false);
        }
    }

}