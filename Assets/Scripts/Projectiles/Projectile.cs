using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StudioByStorm.Obstacles;

namespace StudioByStorm.Projectiles {

    public class Projectile : MonoBehaviour
    {
        public float speed = 7f;
        private float startScale = 0.0f;
        private float targetScale = 1.0f;
        private Tween activeTween;

        void OnEnable()
        {
            init();
            StartCoroutine(AutoDisable(4.0f, 1.0f));
        }

        void init() {
            gameObject.transform.DOScale(startScale, 0.0f);
            activeTween = gameObject.transform.DOScale(targetScale, 1.0f);
        }

        IEnumerator AutoDisable(float WaitTime, float animationTime)
        {
            yield return new WaitForSeconds(WaitTime);
            activeTween = gameObject.transform.DOScale(startScale, animationTime);
            yield return new WaitForSeconds(animationTime);
            gameObject.SetActive(false);
        }

        void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider.CompareTag("Obstacle")) {
                ColorType obstacleColorType = collider.GetComponent<ObstaclePart>().colorType;
                if (obstacleColorType == ColorType.Dark) {
                    StopAllCoroutines();
                    activeTween.Kill();
                    StartCoroutine(AutoDisable(0.0f, 0.1f));
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            transform.position += transform.up * speed * Time.deltaTime;
        }
    }

}