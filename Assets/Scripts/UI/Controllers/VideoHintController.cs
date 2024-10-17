using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Video;
using StudioByStorm.Data;
using StudioByStorm.EventPublishers;

namespace StudioByStorm.UI.Controllers {

    public class VideoHintController : Controller
    {
        [SerializeField] private VideoPlayer videoPlayer;
        [SerializeField] public TMP_Text messageText;
        [SerializeField] private VideoHintData lightVideoHintData;
        [SerializeField] private VideoHintData darkVideoHintData;
        [SerializeField] private GameObject resumeButton;

        private ActionController actionController; 
        private bool isShowingHint = false;

        private bool isLightVideoHintShown = false;
        private bool isDarkVideoHintShown = false;

        protected void Start()
        {
            StartCoroutine(DelayedStart());
        }

        IEnumerator DelayedStart()
        {
            yield return null;
            actionController = GameManager.Singleton.ControllerRegistry.TryGetValue(ViewName.ActionView) as ActionController;

            isLightVideoHintShown = GameManager.Singleton.ProgressManager.GetVideoHintProgress(VideoHintName.LightObstacle);
            isDarkVideoHintShown = GameManager.Singleton.ProgressManager.GetVideoHintProgress(VideoHintName.DarkObstacle);
        }

        protected void OnEnable()
        {
            base.OnEnable();
            GameEventPublisher.OnPlayerHitWrongObstacle += OnPlayerHitWrongObstacle;
        }

        protected void OnDisable()
        {
            base.OnDisable();
            GameEventPublisher.OnPlayerHitWrongObstacle -= OnPlayerHitWrongObstacle;
        }

        protected void OnPlayerHitWrongObstacle()
        {
            //if light & light hint hasn't been shown
            if (GameManager.Singleton.PlayerController.isLight && !isLightVideoHintShown) {
                isLightVideoHintShown = true;
                GameManager.Singleton.ProgressManager.UpdateVideoHint(VideoHintName.LightObstacle, true);
                StartCoroutine(ShowVideoHint(lightVideoHintData));

            //if dark & dark hint hasn't been shown
            } else if (!GameManager.Singleton.PlayerController.isLight && !isDarkVideoHintShown) {
                isDarkVideoHintShown = true;
                GameManager.Singleton.ProgressManager.UpdateVideoHint(VideoHintName.DarkObstacle, true);
                StartCoroutine(ShowVideoHint(darkVideoHintData));
            }
        }

        IEnumerator ShowVideoHint(VideoHintData vhd)
        {
            yield return null;

            if (!isShowingHint) {
                isShowingHint = true;
                //lock player movement while hint view is shown
                bool isJumpLocked = true;
                actionController.LockJump(isJumpLocked);
                //set the values
                messageText.text = vhd.message;
                videoPlayer.clip = vhd.clip;
                //show the view
                GameManager.Singleton.UIController.ShowView(ViewName);
                //wait a second before activating the resume button
                yield return new WaitForSeconds(2.0f);
                resumeButton.SetActive(true);
            }
        }

        public void ResumeButtonClick()
        {
            isShowingHint = false;
            resumeButton.SetActive(false);
            bool isJumpLocked = false;
            actionController.LockJump(isJumpLocked);
            GameManager.Singleton.UIController.Close(ViewName);
        }
    }

}