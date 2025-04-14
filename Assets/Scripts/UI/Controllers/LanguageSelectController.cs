using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace StudioByStorm.UI.Controllers {
    
    public class LangugageSelectController : Controller
    {
        protected void Start()
        {
            StartCoroutine(DelayedStart());
        }

        IEnumerator DelayedStart()
        {
            yield return null;
            int playerLanguage = GameManager.Singleton.ProgressManager.GetPlayerLanguage();

            //players never seen this screen before
            if (playerLanguage == -1) {
                GameManager.Singleton.UIController.ShowView(ViewName);
            }
        }

        public void SelectEnglish()
        {
            SelectLanguagePack(LanguagePackName.English);
        }

        public void SelectFilipino()
        {
            SelectLanguagePack(LanguagePackName.Filipino);
        }

        public void SelectSpanish()
        {
            SelectLanguagePack(LanguagePackName.Spanish);
        }

        private void SelectLanguagePack(LanguagePackName languagePackName)
        {
            GameManager.Singleton.ProgressManager.UpdatePlayerLanguage((int) languagePackName);
            GameManager.Singleton.ProgressManager.Save();
            //########################################################################
            /*
            * Fixes Start Screen without Translations issue
            */
            View StartView = GameManager.Singleton.ViewRegistry.TryGetValue(ViewName.StartView);
            StartView.gameObject.SetActive(false);
            StartView.gameObject.SetActive(true);
            //########################################################################

            GameManager.Singleton.UIController.Close(ViewName); 

            //if its the first time we've ever opened this
            if (FTUEManager.singleton.isFirstOpen) {
                StartCoroutine(HandleUIOnFTUE());
            }
        }

        IEnumerator HandleUIOnFTUE()
        {
            StartController StartController = GameManager.Singleton.ControllerRegistry.TryGetValue(ViewName.StartView) as StartController;
            StartController.StartLevelSelectButtonClick();

            View LevelSelectView = GameManager.Singleton.ViewRegistry.TryGetValue(ViewName.LevelSelectView) as View;
            yield return new WaitUntil(() => LevelSelectView.gameObject.activeSelf);
            GameManager.Singleton.UIController.Close(ViewName.LevelSelectView);

            LevelSelectController LevelSelectController = GameManager.Singleton.ControllerRegistry.TryGetValue(ViewName.LevelSelectView) as LevelSelectController;
            LevelSelectController.CutSceneSelectButtonClick(0,0);

            FTUEManager.singleton.SetFTUEFirstOpenComplete();
        }
    }

}