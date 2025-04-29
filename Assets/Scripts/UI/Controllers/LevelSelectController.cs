using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using StudioByStorm.Config;
using StudioByStorm.Data.LevelChapters;

namespace StudioByStorm.UI.Controllers {

    public class LevelSelectController : Controller
    {
        public Text headingText;
        public GameObject LevelSelectButtonGO;
        public GameObject CutSceneSelectButtonGO;
        public GameObject LevelPackRowGO;
        public GameObject LevelRowGO;
        public GameObject ChapterRowGO;
        public GameObject ViewportContentSpawnLocation;
        public LevelConfig LevelConfig;

        //cache it so we can check its value & re-use views if possible
        private string levelPackName;

        public void Show()
        {
            //requesting to see a level pack
            if (levelPackName != LevelPackSelectController.currentLevelPackName.ToString()) {
                for (int i = 0; i < ViewportContentSpawnLocation.transform.childCount; i++) {
                    Destroy(ViewportContentSpawnLocation.transform.GetChild(i).gameObject);
                }

                StartCoroutine(DelayedShow());

            //requesting to see the same level pack multiple times in a row
            } else if (levelPackName == LevelPackSelectController.currentLevelPackName.ToString()) {
                GameManager.Singleton.UIController.ShowView(ViewName.LevelSelectView);
            }

            levelPackName = LevelPackSelectController.currentLevelPackAlias;
            //headingText.text = levelPackName;

            //level pack header stuff
            GameObject levelpackrowgo = Instantiate(LevelPackRowGO, ViewportContentSpawnLocation.transform);
            LevelPackRowView levelPackRowView = levelpackrowgo.GetComponent<LevelPackRowView>();
            levelPackRowView.headingText.text = levelPackName;
        }
        
        IEnumerator DelayedShow()
        {
            yield return null;
            
            for (int i = 0; i < GameManager.Singleton.LevelManager.levelChapters.chapters.Count; i++) {
                Chapter currentChapter = GameManager.Singleton.LevelManager.levelChapters.chapters[i];
                

                //place chapter heading and subheading ie chapterrowview
                GameObject chapterrowgo = Instantiate(ChapterRowGO, ViewportContentSpawnLocation.transform);
                ChapterRowView ChapterRowView = chapterrowgo.GetComponent<ChapterRowView>();
                ChapterRowView.headingText.text = currentChapter.headingTranslations[GameManager.Language];
                ChapterRowView.subHeadingText.text = currentChapter.subHeadingTranslations[GameManager.Language];

                int levelID = 0;
                int ChapterID = i;

                int cutSceneCount = GameManager.Singleton.LevelManager.levelChapters.chapters[i].cutScenes.Count;
                int cutSceneIterationNumber = (cutSceneCount < GameManager.Singleton.LevelManager.initiallyAvailableLevelRowsPerChapter) ? cutSceneCount : GameManager.Singleton.LevelManager.initiallyAvailableLevelRowsPerChapter;

                for (int j = 0; j < cutSceneIterationNumber; j++) {
                    int cutSceneID = j;
                    string cutSceneKey = (LevelPackSelectController.currentLevelPackName + "-" + ChapterID +"-"+ cutSceneID);
                    //place a level row 
                    GameObject levelrowgo = Instantiate(LevelRowGO, ViewportContentSpawnLocation.transform);

                    //get cut scene locked status
                    string unlockedCutSceneProgressKey = LevelPackSelectController.currentLevelPackName + "-" + ChapterID;
                    int highestCutSceneCompletedForCurrentChapter = GameManager.Singleton.ProgressManager.GetUnlockedCutSceneProgress(unlockedCutSceneProgressKey);
                    int highestCutSceneAllowedToBePlayed = highestCutSceneCompletedForCurrentChapter + 1;

                    int previousRowsLastLevelID = cutSceneID * 3 - 1;
                    string previousRowsLastLevelIDKEY = (LevelPackSelectController.currentLevelPackName + "-" + ChapterID +"-"+ previousRowsLastLevelID);
                    bool isPreviousRowsLastLevelCompleted = (GameManager.Singleton.ProgressManager.GetLevelProgress(previousRowsLastLevelIDKEY) > 0);
                    bool isCutSceneLocked = ((cutSceneID == 0 || isPreviousRowsLastLevelCompleted) && /*maybe unecessary after here*/ highestCutSceneAllowedToBePlayed >= cutSceneID) ? false : true;

                    //place a cut scene button 
                    GameObject cutsceneselectbuttongo = Instantiate(CutSceneSelectButtonGO, levelrowgo.transform);
                    CutSceneSelectButtonView CutSceneSelectButtonView = cutsceneselectbuttongo.GetComponent<CutSceneSelectButtonView>();
                    CutSceneSelectButtonView.GetComponent<Button>().onClick.AddListener(delegate { 
                                                                                                   CutSceneSelectButtonClick(CutSceneSelectButtonView.ID, 
                                                                                                                             CutSceneSelectButtonView.ChapterID,
                                                                                                                             CutSceneSelectButtonView.GetLockStatus()); 
                                                                                                 });
                    CutSceneSelectButtonView.ID = cutSceneID;
                    CutSceneSelectButtonView.ChapterID = ChapterID;

                    //set cut scene lock status
                    if (! isCutSceneLocked) {
                        //set completion progress indicators
                        bool cutSceneCompletionValue = GameManager.Singleton.ProgressManager.GetCutSceneProgress(cutSceneKey);
                        CutSceneSelectButtonView.SetProgress(cutSceneCompletionValue, GameManager.Singleton.LevelManager.levelCompletionColor);
                        CutSceneSelectButtonView.SetLockStatus(false);
                    } else {
                        CutSceneSelectButtonView.SetLockStatus(true);
                    }

                    //place 3 level select buttons
                    for (int k = 0; k < 3; k++) {
                        //get level locked status
                        string unlockedLevelProgressKey = LevelPackSelectController.currentLevelPackName + "-" + ChapterID;
                        int highestLevelCompletedForCurrentChapter = GameManager.Singleton.ProgressManager.GetUnlockedLevelProgress(unlockedLevelProgressKey);
                        int highestLevelAllowedToBePlayed = highestLevelCompletedForCurrentChapter + 1;

                        bool isRowCutSceneCompleted = GameManager.Singleton.ProgressManager.GetCutSceneProgress(cutSceneKey);
                        bool isLevelLocked = (isRowCutSceneCompleted && highestLevelAllowedToBePlayed >= levelID) ? false : true;

                        GameObject levelselectbuttongo = Instantiate(LevelSelectButtonGO, levelrowgo.transform);
                        LevelSelectButtonView LevelSelectButtonView = levelselectbuttongo.GetComponent<LevelSelectButtonView>();
                        LevelSelectButtonView.GetComponent<Button>().onClick.AddListener(delegate { 
                                                                                                    LevelSelectButtonClick(LevelSelectButtonView.ID, 
                                                                                                                           LevelSelectButtonView.ChapterID, 
                                                                                                                           LevelSelectButtonView.GetLockStatus()); 
                                                                                                  });
                        //set text and data for level select button
                        for (int l = 0; l < LevelSelectButtonView.levelNumberText.Length; l++) {
                            LevelSelectButtonView.ID = levelID;
                            LevelSelectButtonView.ChapterID = ChapterID;
                            LevelSelectButtonView.levelNumberText[l].text = GameManager.Singleton.LevelManager.romanNumerals[levelID + 1];
                        }

                        //boss check
                        LevelSelectButtonView.BossCheck(ChapterID, levelID + 1);

                        //set level lock status
                        if (! isLevelLocked) {
                            //set completion progress indicators
                            string levelKey = (LevelPackSelectController.currentLevelPackName + "-" + ChapterID +"-"+ levelID);
                            int levelCompletionValue = GameManager.Singleton.ProgressManager.GetLevelProgress(levelKey);
                            LevelSelectButtonView.SetProgress(levelCompletionValue, GameManager.Singleton.LevelManager.levelCompletionColor);
                            LevelSelectButtonView.SetLockStatus(false);
                        } else {
                            LevelSelectButtonView.SetLockStatus(true);
                        }

                        

                        levelID++;
                    }
                }
            }

            yield return null;

            GameManager.Singleton.UIController.ShowView(ViewName.LevelSelectView);
        }

        public void LevelSelectButtonClick(int ID, int chapterID, bool isLocked = false)
        {
            if (! isLocked) {
                AudioManager.Singleton.Play(SoundType.ButtonPress);

                //plays current chapter music
                AudioManager.Singleton.SetChapterMusic(chapterID);
                //save most recently played chapter so we can start with that music
                GameManager.Singleton.ProgressManager.UpdateMostRecentlyPlayedChapterIDProgress(chapterID);

                GameManager.Singleton.LevelManager.currentLevelID = ID;
                GameManager.Singleton.LevelManager.currentChapterID = chapterID;
                StartController StartController = GameManager.Singleton.ControllerRegistry.TryGetValue(ViewName.StartView) as StartController;
                ///GameManager.Singleton.UIController.Back();
                StartController.PlayButtonClick();
            }
        }

        public void CutSceneSelectButtonClick(int ID, int chapterID, bool isLocked = false)
        {
            if (! isLocked) {
                AudioManager.Singleton.Play(SoundType.ButtonPress);
                
                CutSceneController CutSceneController = GameManager.Singleton.ControllerRegistry.TryGetValue(ViewName.CutSceneView) as CutSceneController;
                //GameManager.Singleton.UIController.Back();
                CutSceneController.ShowCutScene(ID, chapterID);
            }
        }
    }

}