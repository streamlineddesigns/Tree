using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace StudioByStorm.Tutorials.Animations {

    public class FingerSlingShotAnimation : MonoBehaviour
    {
        [SerializeField] private bool isDebugging = false;
        [SerializeField] private GameObject startDebug;
        [SerializeField] private GameObject targetDebug;

        [SerializeField] private GameObject jumpIndicator;
        [SerializeField] private GameObject fingerIndicatorContainer;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite tapLightSprite;
        [SerializeField] private Sprite tapDarkSprite;
        [SerializeField] private Sprite swipeSprite;

        public bool isAnimating {
            get {
                return _isAnimating;
            }
        }
        private bool _isAnimating = false;
        private Vector3 startPosition;
        private Vector3 targetPosition;
        private Vector3 direction;
        private float jumpIndicatorScaling = 0.8f;
        private bool isJumpIndicatorOn;
        private Tween spriteTween;
        private Tween jumpIndicatorTween;

        public void OnEnable()
        {
            if (isDebugging) {
                SetPositions(startDebug, targetDebug);
                Animate();
            }
        }

        public void SetPositions(GameObject start, GameObject target)
        {
            startPosition = start.transform.position;
            direction = (startPosition - target.transform.position).normalized;
            float magnitude = Vector3.Distance(start.transform.position, target.transform.position);
            //targetPosition = startPosition + (direction * magnitude);
            targetPosition = startPosition - (direction * magnitude);
        }

        public void Animate()
        {
            _isAnimating = true;
            StartCoroutine(AnimationUpdate());
        }

        public void Stop()
        {
            _isAnimating = false;
            StopCoroutine(AnimationUpdate());
        }

        protected IEnumerator AnimationUpdate()
        {
            while (_isAnimating) {
                //reset all the start data
                spriteTween.Kill();
                jumpIndicatorTween.Kill();
                //transform.position = startPosition;
                //skip to target position
                transform.position = targetPosition;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                fingerIndicatorContainer.transform.rotation = Quaternion.Euler(0, 0, angle + 90.0f);
                spriteRenderer.sprite = swipeSprite;
                spriteRenderer.DOFade(0.0f, 0.0f);
                spriteRenderer.DOFade(1.0f, 1.0f);
                jumpIndicator.transform.DOScale(new Vector3(0.0f, 0.0f, 0.0f), 0.0f);
                jumpIndicator.SetActive(false);
                yield return new WaitForSeconds(1.0f);
                //show the "tapping" animation 2 times
                int timesToTap = 1;
                for (int i = 0; i < timesToTap; i++) {
                    spriteRenderer.sprite = (GameManager.Singleton.PlayerController.isLight) ? tapLightSprite : tapDarkSprite;
                    yield return new WaitForSeconds(0.5f);
                    spriteRenderer.sprite = swipeSprite;
                    yield return new WaitForSeconds(0.25f);
                }
                /*jumpIndicator.SetActive(true);
                //rotate the object around the Z axis to match the direction
                jumpIndicator.transform.rotation = Quaternion.Euler(0, 0, angle + 90.0f);
                //move the hand to the target position
                spriteTween = transform.DOMove(targetPosition, 1.5f);
                //scale the jump indicator simultaneously
                jumpIndicatorTween = jumpIndicator.transform.DOScale(new Vector3(jumpIndicatorScaling, jumpIndicatorScaling, jumpIndicatorScaling), 1.5f);
                yield return new WaitForSeconds(1.5f);
                //deactivate everything
                jumpIndicator.SetActive(false);*/
                spriteRenderer.DOFade(0.0f, 0.5f);
                yield return new WaitForSeconds(0.5f);
            }
        }
    }

}