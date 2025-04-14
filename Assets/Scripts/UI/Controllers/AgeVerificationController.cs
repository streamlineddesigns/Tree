using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace StudioByStorm.UI.Controllers {
    
    public class AgeVerificationController : Controller
    {
        [SerializeField] private Slider ageSlider;
        [SerializeField] private TMP_Text ageText;
        [SerializeField] private Button acceptButton;
        [SerializeField] private Color alertColor;
        [SerializeField] private Color activeColor;
        [SerializeField] private Color inActiveColor;
        [SerializeField] private Image ageSliderOutline;
        [SerializeField] private Image acceptButtonOutline;
        private bool hasAgeValueChanged = false;
        private string termsOfServiceURL = "https://colorflowarcadepuzzles.com/termsofservice.html";
        private string privacyPolicyURL = "https://colorflowarcadepuzzles.com/privacy.html";
        private int playerAge;


        protected void Start()
        {
            StartCoroutine(DelayedStart());
        }

        IEnumerator DelayedStart()
        {
            yield return null;
            int playerLanguage = GameManager.Singleton.ProgressManager.GetPlayerLanguage();
            playerAge = GameManager.Singleton.ProgressManager.GetPlayerAge();

            //players never seen this screen before
            if (playerAge == -1) {

                //if its not the FTUE
                if (!FTUEManager.singleton.isFTUE) {
                    //if language isn't set we'll show it ourselves otherwise language controller will handle that
                    if (playerLanguage != -1) {
                        GameManager.Singleton.UIController.ShowView(ViewName);
                    }
                //if it is the FTUE, init sdk without PII, and wait to confirm age
                } else {
                    AnalyticsManager.InitSDK();
                }
                
            
            //players seen this screen before and accepted, so init sdk and it will handle age logic
            } else {
                AnalyticsManager.InitSDK();
            }
        }

        public void OnAgeSliderValueChange()
        {
            playerAge = (int) ageSlider.value;
            ageText.text = playerAge.ToString();
            
            if (! hasAgeValueChanged) {
                hasAgeValueChanged = true;
            }

            if (playerAge <= 0) {
                ageSliderOutline.color = activeColor;
                acceptButtonOutline.color = inActiveColor;
            } else {
                ageSliderOutline.color = inActiveColor;
                acceptButtonOutline.color = activeColor;
            }
        }

        public void termsOfServiceButtonClick()
        {
            Application.OpenURL(termsOfServiceURL);
        }

        public void privacyPolicyButtonClick()
        {
            Application.OpenURL(privacyPolicyURL);
        }

        public void acceptButtonClick()
        {
            if (! hasAgeValueChanged || playerAge <= 0) {
                AgeSliderAlertAnimation();
            } else {
                GameManager.Singleton.UIController.Close(ViewName);
                GameManager.Singleton.ProgressManager.UpdatePlayerAge(playerAge);
                GameManager.Singleton.ProgressManager.Save();
                AnalyticsManager.InitSDK();
            }
        }

        private void AgeSliderAlertAnimation()
        {
            Sequence alertAnimation = DOTween.Sequence();
            alertAnimation.Append(ageSliderOutline.DOColor(alertColor, 0.075f))
                          .Append(ageSliderOutline.DOColor(activeColor, 0.075f))
                          .Append(ageSliderOutline.DOColor(alertColor, 0.075f))
                          .Append(ageSliderOutline.DOColor(activeColor, 0.075f))
                          .Append(ageSliderOutline.DOColor(alertColor, 0.075f))
                          .Append(ageSliderOutline.DOColor(activeColor, 0.075f));
        }
    }

}