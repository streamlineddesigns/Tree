using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using StudioByStorm.Data.LevelChapters;

namespace StudioByStorm.UI.Controllers {

    public class CutSceneController : Controller
    {
        public int currentCutSceneID;
        public int currentChapterID;
        public TMP_Text headingText;
        public TMP_Text messageText;
        public GameObject continueButton;

        public void ShowCutScene(int cutSceneID, int chapterID)
        {
            currentCutSceneID = cutSceneID;
            currentChapterID = chapterID;
            GameManager.Singleton.UIController.ShowView(ViewName.CutSceneView);
            StartCoroutine(CutScene());
        }

        IEnumerator CutScene()
        {
            yield return 0;

            //Hide the messages / reset state
            headingText.gameObject.SetActive(false);
            messageText.gameObject.SetActive(false);
            continueButton.SetActive(false);
            headingText.text = "";
            messageText.text = "";

            Chapter currentChapter = GameManager.Singleton.LevelManager.levelChapters.chapters[currentChapterID];

            //only run this for the first cut scene of each chapter
            if (currentCutSceneID == 0) {
                headingText.gameObject.SetActive(true);
                yield return StartCoroutine(PrintText(headingText, currentChapter.heading));
            }

            messageText.gameObject.SetActive(true);
            yield return StartCoroutine(PrintText(messageText, currentChapter.cutScenes[currentCutSceneID].message));

            continueButton.SetActive(true);
        }

        IEnumerator PrintText(TMP_Text TMP, string textToPrint)
        {
            //get characters from current chapter heading
            char[] charArray = textToPrint.ToCharArray();
            List<char> charList = new List<char>();
            //iterate over characters and show them one at a time
            for (int i = 0; i < charArray.Length; i++) {
                charList.Add(charArray[i]);
                TMP.text = new string(charList.ToArray());
                Color targetColor = TMP.color;
                targetColor.a = (i * 1.0f / charArray.Length);
                TMP.color = targetColor;
                yield return new WaitForSeconds(0.1f);
            }
        }

        public void ContinueButtonClick()
        {
            int levelID = currentCutSceneID * 3;
            LevelSelectController LevelSelectController = GameManager.Singleton.ControllerRegistry.TryGetValue(ViewName.LevelSelectView) as LevelSelectController;
            LevelSelectController.LevelSelectButtonClick(levelID, currentChapterID);
        }
    }

}