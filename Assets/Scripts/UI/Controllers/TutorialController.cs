using System.Linq;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using StudioByStorm.EventPublishers;
using StudioByStorm.Data;
using StudioByStorm.Tutorials;

namespace StudioByStorm.UI.Controllers {

    public class TutorialController : Controller
    {
        [SerializeField] private List<TutorialData> tutorialData;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private TMP_Text messageShadowText;
        [SerializeField] private GameObject tutorialsParent;

        private bool isTutorialDismissed;
        private bool isTutorialCompleted;
        private int currentTutorialIndex = -1;
        private Tutorial currentTutorial;
        private bool isCurrentTutorialTextPrinted;
        private bool isLevelCompleted;

        protected void OnEnable()
        {
            base.OnEnable();
            GameEventPublisher.OnStateChange += OnStateChange;
        }

        protected void OnDisable()
        {
            base.OnDisable();
            GameEventPublisher.OnStateChange -= OnStateChange;
            StopAllCoroutines();
        }

        protected void OnStateChange(GameState state)
        {
            switch(state) {
                case GameState.GameStart :
                    CheckIfTutorialIsNeeded();
                    break;

                case GameState.LevelComplete :
                    isLevelCompleted = true;
                    StartCoroutine(DelayedOnLevelComplete());
                    break;
            }
        }

        public void TutorialDialogClick()
        {
            isTutorialDismissed = true;
        }

        protected void CheckIfTutorialIsNeeded()
        {
            if (isLevelCompleted) {
                return;
            }

            isTutorialDismissed = false;

            int currentLevelID = GameManager.Singleton.LevelManager.currentLevelID;
            int currentChapterID = GameManager.Singleton.LevelManager.currentChapterID;

            int highestTutorialIndexCompleted = GameManager.Singleton.ProgressManager.GetTutorialProgress();

            currentTutorialIndex = highestTutorialIndexCompleted + 1;

            bool isThereAnyOtherTutorials = (currentTutorialIndex <= tutorialData.Count - 1);

            if (isThereAnyOtherTutorials) {
                TutorialData nextTutorialData = tutorialData[currentTutorialIndex];
                bool isTutorialNeededForCurrentLevel = (currentLevelID >= nextTutorialData.levelID && currentChapterID >= nextTutorialData.chapterID);
                
                //show the tutorial only if necessary
                if (isTutorialNeededForCurrentLevel) {
                    StartCoroutine(ShowTutorial());
                }
            }
        }

        IEnumerator ShowTutorial()
        {
            yield return new WaitForSeconds(1.0f);

            TutorialData currentTutorialData = tutorialData[currentTutorialIndex];

            GameManager.Singleton.UIController.ShowView(ViewName.TutorialView);

            //instantiate tutorial prefab if any
            GameObject tutorialPrefab = currentTutorialData.prefab;
            if (tutorialPrefab != null) {
                GameObject tutorialGO = Instantiate(tutorialPrefab, tutorialsParent.transform);
                currentTutorial = tutorialGO.GetComponent<Tutorial>();
                currentTutorial.Init();
            }

            //Get and print tutorial message
            messageText.text = "";
            messageShadowText.text = "";
            isCurrentTutorialTextPrinted = false;
            string textToPrint = currentTutorialData.message;
            StartCoroutine(PrintText(textToPrint));

            if (currentTutorial != null) {
                //begin tutorial script & wait for its completion
                currentTutorial.Begin();
                StartCoroutine(currentTutorial.WaitUntilFinished());
                yield return new WaitUntil(() => isLevelCompleted || isTutorialDismissed || currentTutorial.isAborting || (currentTutorial.isFinished && isCurrentTutorialTextPrinted));
                
                //end tutorial & clean up
                currentTutorial.End();
                currentTutorial.CleanUp();
            }

            //prevents accidental saving from occuring
            if (! isLevelCompleted && ! currentTutorial.isAborting) {
                //the user dismissed the tutorial 
                if (isTutorialDismissed) {
                    yield return new WaitForSeconds(0.25f);
                
                //wait just a second... lol the user should've had time to read the tutorial and complete it if they've gotten this far but still
                } else if (currentTutorial != null) {
                    yield return new WaitForSeconds(1.0f);
                
                } else {
                    //Wait for text to complete printing & then an extra 3 seconds
                    yield return new WaitUntil(() => isCurrentTutorialTextPrinted);
                    yield return new WaitForSeconds(3.0f);
                }

                //update the tutorial progress so it can be saved when the level is beaten
                CompleteTutorial();

                //continue to play tutorials as necessary
                Continue();


            //if the level was completed we just want to hide the dialog immediately
            } else {
                HideView();
            }
        }

        IEnumerator PrintText(string textToPrint)
        {
            textToPrint = textToPrint.Replace("<br>", Environment.NewLine);

            //get characters from current chapter heading
            char[] charArray = textToPrint.ToCharArray();
            List<char> charList = new List<char>();
            //iterate over characters and show them one at a time
            for (int i = 0; i < charArray.Length; i++) {
                if (isTutorialDismissed) {
                    break;
                }
                charList.Add(charArray[i]);
                char[] currentCharacters = charList.ToArray();
                messageText.text = new string(currentCharacters);
                messageShadowText.text = new string(currentCharacters);
                yield return new WaitForSeconds(0.1f);
            }
            
            isCurrentTutorialTextPrinted = true;
        }

        IEnumerator DelayedOnLevelComplete()
        {
            HideView();
            yield return new WaitForSeconds(1.5f);
            StopAllCoroutines();
            SaveCheck();
        }

        protected void CompleteTutorial()
        {
            isTutorialCompleted = true;
            GameManager.Singleton.ProgressManager.UpdateTutorial(currentTutorialIndex);
        }

        protected void Continue()
        {
            HideView();
            CheckIfTutorialIsNeeded();
        }

        protected void HideView()
        {
            GameManager.Singleton.UIController.Close(ViewName.TutorialView);
        }

        protected void SaveCheck()
        {
            if (isTutorialCompleted) {
                GameManager.Singleton.ProgressManager.Save();
            }
        }
    }

}