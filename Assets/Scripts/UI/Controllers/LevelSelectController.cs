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
        public GameObject LevelSelectButtonGO;
        public GameObject CutSceneSelectButtonGO;
        public GameObject LevelRowGO;
        public GameObject ChapterRowGO;
        public GameObject ViewportContentSpawnLocation;
        public LevelConfig LevelConfig;
        public List<string> romanNumerals;
        public Color completionColor;

        void Start()
        {
            StartCoroutine(DelayedStart());
        }
        
        IEnumerator DelayedStart()
        {
            yield return 0;

            for (int i = 0; i < GameManager.Singleton.LevelManager.levelChapters.chapters.Count; i++) {
                Chapter currentChapter = GameManager.Singleton.LevelManager.levelChapters.chapters[i];

                //place chapter heading and subheading ie chapterrowview
                GameObject chapterrowgo = Instantiate(ChapterRowGO, ViewportContentSpawnLocation.transform);
                ChapterRowView ChapterRowView = chapterrowgo.GetComponent<ChapterRowView>();
                ChapterRowView.headingText.text = currentChapter.heading;
                ChapterRowView.subHeadingText.text = currentChapter.subHeading;

                int levelID = 0;
                int ChapterID = i;

                for (int j = 0; j < currentChapter.cutScenes.Count; j++) {
                    int cutSceneID = j;
                    //place a level row 
                    GameObject levelrowgo = Instantiate(LevelRowGO, ViewportContentSpawnLocation.transform);
                    //place a cut scene button 
                    GameObject cutsceneselectbuttongo = Instantiate(CutSceneSelectButtonGO, levelrowgo.transform);
                    CutSceneSelectButtonView CutSceneSelectButtonView = cutsceneselectbuttongo.GetComponent<CutSceneSelectButtonView>();
                    CutSceneSelectButtonView.GetComponent<Button>().onClick.AddListener(delegate { CutSceneSelectButtonClick(CutSceneSelectButtonView.ID, CutSceneSelectButtonView.ChapterID); });
                    CutSceneSelectButtonView.ID = cutSceneID;
                    CutSceneSelectButtonView.ChapterID = ChapterID;
                    //set completion progress indicators
                    string cutSceneKey = (ChapterID +"-"+ cutSceneID);
                    bool cutSceneCompletionValue = GameManager.Singleton.ProgressManager.GetCutSceneProgress(cutSceneKey);
                    CutSceneSelectButtonView.SetProgress(cutSceneCompletionValue, completionColor);

                    //place 3 level select buttons
                    for (int k = 0; k < 3; k++) {
                        GameObject levelselectbuttongo = Instantiate(LevelSelectButtonGO, levelrowgo.transform);
                        LevelSelectButtonView LevelSelectButtonView = levelselectbuttongo.GetComponent<LevelSelectButtonView>();
                        LevelSelectButtonView.GetComponent<Button>().onClick.AddListener(delegate { LevelSelectButtonClick(LevelSelectButtonView.ID, LevelSelectButtonView.ChapterID); });
                        //set text and data for level select button
                        for (int l = 0; l < LevelSelectButtonView.levelNumberText.Length; l++) {
                            LevelSelectButtonView.ID = levelID;
                            LevelSelectButtonView.ChapterID = ChapterID;
                            LevelSelectButtonView.levelNumberText[l].text = romanNumerals[levelID + 1];
                        }
                        //set completion progress indicators
                        string levelKey = (ChapterID +"-"+ levelID);
                        int levelCompletionValue = GameManager.Singleton.ProgressManager.GetLevelProgress(levelKey);
                        LevelSelectButtonView.SetProgress(levelCompletionValue, completionColor);

                        levelID++;
                    }
                }
            }
        }

        public void LevelSelectButtonClick(int ID, int chapterID)
        {
            GameManager.Singleton.LevelManager.currentLevelID = ID;
            GameManager.Singleton.LevelManager.currentChapterID = chapterID;
            StartController StartController = GameManager.Singleton.ControllerRegistry.TryGetValue(ViewName.StartView) as StartController;
            GameManager.Singleton.UIController.Back();
            StartController.PlayButtonClick();
        }

        public void CutSceneSelectButtonClick(int ID, int chapterID)
        {
            CutSceneController CutSceneController = GameManager.Singleton.ControllerRegistry.TryGetValue(ViewName.CutSceneView) as CutSceneController;
            GameManager.Singleton.UIController.Back();
            CutSceneController.ShowCutScene(ID, chapterID);
        }
    }

}