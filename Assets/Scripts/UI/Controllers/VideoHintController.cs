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
        [SerializeField]
        private VideoPlayer videoPlayer;
        [SerializeField]
        public TMP_Text messageText;
        [SerializeField]
        private List<VideoHintData> videoHints = new List<VideoHintData>();
        [SerializeField]
        private GameObject resumeButton;

        private ActionController actionController; 
        private List<(int, int)> currentHintIndexs;
        private List<int> hintsShown = new List<int>();
        private bool isShowingHint = false;

        protected void Start()
        {
            StartCoroutine(DelayedStart());
        }

        IEnumerator DelayedStart()
        {
            yield return null;
            actionController = GameManager.Singleton.ControllerRegistry.TryGetValue(ViewName.ActionView) as ActionController;
            StartCoroutine(CheckIfHintExists());
        }

        protected void OnEnable()
        {
            base.OnEnable();
            GameEventPublisher.OnStateChange += OnStateChange;
        }

        protected void OnDisable()
        {
            base.OnDisable();
            GameEventPublisher.OnStateChange -= OnStateChange;
        }

        protected void OnStateChange(GameState state)
        {
            if (state == GameState.GameStart) {
                //check if a hint for the current level exists
                currentHintIndexs = videoHints
                    .Select((hint, index) => new { hint, index })  // Create an anonymous object with element and index
                    .Where(x => x.hint.levelID == GameManager.Singleton.LevelManager.currentLevelID // Filter by LevelID
                             && x.hint.chapterID == GameManager.Singleton.LevelManager.currentChapterID)  // Filter by ChapterID
                    .Select(x => (x.index, x.hint.nodeID))      // Select a tuple of (index, nodeID)
                    .ToList();
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

        IEnumerator CheckIfHintExists()
        {
            while (true) {
                if (currentHintIndexs != null && actionController.ActionModel.CurrentNode != null) {
                    //check if a hint for the current node exists
                    bool exists = (currentHintIndexs.Any(t => t.Item2 == actionController.ActionModel.CurrentNode.ID));

                    if (! isShowingHint && exists && !hintsShown.Contains(actionController.ActionModel.CurrentNode.ID)) {
                        //ensure it only runs one time and not every x frames
                        isShowingHint = true;
                        hintsShown.Add(actionController.ActionModel.CurrentNode.ID);
                        //lock player movement while hint view is shown
                        bool isJumpLocked = true;
                        actionController.LockJump(isJumpLocked);
                        //retrieve the index/node tuple
                        (int, int) hintIndexTuple = currentHintIndexs.First(t => t.Item2 == actionController.ActionModel.CurrentNode.ID);
                        //retrieve the actual hint data object
                        VideoHintData videoHintData = videoHints[hintIndexTuple.Item1];
                        //set the values
                        messageText.text = videoHintData.message;
                        videoPlayer.clip = videoHintData.clip;
                        //show the view
                        GameManager.Singleton.UIController.ShowView(ViewName);
                        //wait a second before activating the resume button
                        yield return new WaitForSeconds(2.0f);
                        resumeButton.SetActive(true);
                    }
                }
                

                yield return new WaitForSeconds(0.05f);
            }
        }
    }

}