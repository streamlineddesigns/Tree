using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using StudioByStorm.EventPublishers;
using StudioByStorm.Data;

namespace StudioByStorm.UI.Controllers {

    public class TutorialController : Controller
    {
        [SerializeField] private List<TutorialData> tutorialData;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private TMP_Text messageShadowText;
        private int completedTutorialIndex = -1;
        private int nextTutorialIndex = -1;

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
                    StopAllCoroutines();
                    HideView();
                    SaveCheck();
                    break;
            }
        }

        protected void CheckIfTutorialIsNeeded()
        {
            int currentLevelID = GameManager.Singleton.LevelManager.currentLevelID;

            int highestTutorialIndexCompleted = GameManager.Singleton.ProgressManager.GetTutorialProgress();
            nextTutorialIndex = highestTutorialIndexCompleted + 1;
            bool isThereAnyOtherTutorials = (nextTutorialIndex <= tutorialData.Count - 1);

            if (isThereAnyOtherTutorials) {
                TutorialData nextTutorialData = tutorialData[nextTutorialIndex];
                bool isTutorialNeededForCurrentLevel = (currentLevelID >= nextTutorialData.levelID);
                
                //show the tutorial only if necessary
                if (isTutorialNeededForCurrentLevel) {
                    StartCoroutine(ShowTutorial());
                }
            }
        }

        IEnumerator ShowTutorial()
        {
            yield return new WaitForSeconds(1.0f);

            TutorialData currentTutorialData = tutorialData[nextTutorialIndex];

            GameManager.Singleton.UIController.ShowView(ViewName.TutorialView);

            //Wait for text to complete typing
            string textToPrint = currentTutorialData.message;
            yield return StartCoroutine(PrintText(textToPrint));

            //update the tutorial progress so it can be saved if the level is beaten
            CompleteTutorial();

            //wait for a couple seconds so the user can read
            yield return new WaitForSeconds(3.0f);

            //continue to play tutorials as necessary
            Continue();
        }

        IEnumerator PrintText(string textToPrint)
        {
            textToPrint = textToPrint.Replace("<br>", Environment.NewLine);
            messageText.text = "";
            messageShadowText.text = "";

            //get characters from current chapter heading
            char[] charArray = textToPrint.ToCharArray();
            List<char> charList = new List<char>();
            //iterate over characters and show them one at a time
            for (int i = 0; i < charArray.Length; i++) {
                charList.Add(charArray[i]);
                char[] currentCharacters = charList.ToArray();
                messageText.text = new string(currentCharacters);
                messageShadowText.text = new string(currentCharacters);
                yield return new WaitForSeconds(0.1f);
            }
        }

        protected void CompleteTutorial()
        {
            completedTutorialIndex = nextTutorialIndex;
            GameManager.Singleton.ProgressManager.UpdateTutorial(completedTutorialIndex);
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
            if (completedTutorialIndex != -1) {
                GameManager.Singleton.ProgressManager.Save();
            }
        }
    }

}