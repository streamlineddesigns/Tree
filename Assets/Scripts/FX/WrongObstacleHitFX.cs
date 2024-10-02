using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace StudioByStorm.FX {

    public class WrongObstacleHitFX : MonoBehaviour
    {
        public List<GameObject> sprites; // List of sprites
        public float explosionForce = 5f; // Force of the explosion
        public float scaleOutDuration = 0.5f;
        public float originalScaleValue = 0.5f;
        public Color color;

        private void OnEnable()
        {
            TriggerExplosion();
            StartCoroutine(AutoDeactivate());
        }

        public void SetColor(Color c)
        {
            color = c;
        }

        // Method to trigger the explosion
        private void TriggerExplosion()
        {
            for (int i = 0; i < sprites.Count; i++)
            {
                //reset everything
                GameObject currentSprite = sprites[i];
                currentSprite.GetComponent<SpriteRenderer>().color = color;
                currentSprite.gameObject.transform.localPosition = Vector3.zero;
                currentSprite.gameObject.transform.localScale = new Vector3(originalScaleValue, originalScaleValue, originalScaleValue);
                currentSprite.gameObject.SetActive(true);

                // Get the Rigidbody2D component
                Rigidbody2D rb = currentSprite.GetComponent<Rigidbody2D>();

                if (rb != null)
                {
                    // Calculate a random direction for the explosion
                    Vector2 randomDirection = Random.insideUnitCircle.normalized;

                    // Apply explosion force in the random direction
                    rb.AddForce(randomDirection * explosionForce, ForceMode2D.Impulse);
                }

                currentSprite.transform.DOScale(Vector3.zero, scaleOutDuration).SetEase(Ease.OutQuad);
            }
        }

        IEnumerator AutoDeactivate()
        {
            yield return new WaitForSeconds(scaleOutDuration);
            gameObject.SetActive(false);
        }
    }

}