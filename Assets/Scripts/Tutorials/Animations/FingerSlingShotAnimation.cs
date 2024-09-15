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
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite tapLightSprite;
        [SerializeField] private Sprite tapDarkSprite;
        [SerializeField] private Sprite swipeSprite;

        private bool isAnimating = false;
        private Vector3 startPosition;
        private Vector3 targetPosition;
        private Vector3 direction;
        private float jumpIndicatorScaling = 0.9f;
        private bool isJumpIndicatorOn;

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
            targetPosition = startPosition + (direction * magnitude);
        }

        public void Animate()
        {
            isAnimating = true;
            StartCoroutine(AnimationUpdate());
        }

        public void Stop()
        {
            isAnimating = false;
            StopCoroutine(AnimationUpdate());
        }

        protected IEnumerator AnimationUpdate()
        {
            while (isAnimating) {
                //reset all the start data
                transform.position = startPosition;
                spriteRenderer.sprite = swipeSprite;
                spriteRenderer.DOFade(0.0f, 0.0f);
                spriteRenderer.DOFade(1.0f, 1.0f);
                jumpIndicator.transform.DOScale(new Vector3(0.0f, 0.0f, 0.0f), 0.0f);
                jumpIndicator.SetActive(true);
                yield return new WaitForSeconds(1.0f);
                //show the "tapping" animation 2 times
                int timesToTap = 2;
                for (int i = 0; i < timesToTap; i++) {
                    spriteRenderer.sprite = (GameManager.Singleton.PlayerController.isLight) ? tapLightSprite : tapDarkSprite;
                    yield return new WaitForSeconds(0.5f);
                    spriteRenderer.sprite = swipeSprite;
                    yield return new WaitForSeconds(0.25f);
                }
                //calculate the angle between the direction and the X axis
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                //rotate the object around the Z axis to match the direction
                jumpIndicator.transform.rotation = Quaternion.Euler(0, 0, angle + 90.0f);
                //move the hand to the target position
                transform.DOMove(targetPosition, 1.5f);
                //scale the jump indicator simultaneously
                jumpIndicator.transform.DOScale(new Vector3(jumpIndicatorScaling, jumpIndicatorScaling, jumpIndicatorScaling), 1.5f);
                yield return new WaitForSeconds(1.5f);
                //deactivate everything
                jumpIndicator.SetActive(false);
                spriteRenderer.DOFade(0.0f, 0.5f);
                yield return new WaitForSeconds(0.5f);
            }
        }
    }

}