using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StudioByStorm.Data.LevelPacks;
using StudioByStorm.Data.LevelChapters;
using GameAnalyticsSDK;

namespace StudioByStorm.UI.Controllers {

    public class LevelPackSelectController : Controller
    {
        public List<LevelPackData> levelPacks = new List<LevelPackData>();
        public int levelPackID = -1;
        
        public static LevelPackName currentLevelPackName;
        public static string currentLevelPackAlias;
        public static GameObject currentLevelPackGO;
        public static LevelPack currentLevelPack;

        private float waitTimer = 0.0f;

        protected void Start()
        {
            if (GameManager.Singleton.ProgressManager.GetLevelPack() != -1) {
                levelPackID = GameManager.Singleton.ProgressManager.GetLevelPack();
                UpdateLevelPackSelectScreen();
            } else {
                StartCoroutine(InitLevelPackID());
            }
        }

        protected void Update()
        {
            if (AnalyticsManager.playerAge != 0 && waitTimer <= 2.0f) {
                waitTimer += Time.deltaTime;
            }
        }

        public IEnumerator Show()
        {
            yield return new WaitUntil(() => levelPackID != -1);

            LevelPackName levelPackNameEnum = (LevelPackName) levelPackID;
            string levelPackNameString = levelPackNameEnum.ToString();
            int currentChapterID = 0;
            int currentLevelID = 14;//0 starting index so it's 15
            string key = (levelPackNameString + "-" + currentChapterID + "-" + currentLevelID);
            int previousStarsAwarded = GameManager.Singleton.ProgressManager.GetLevelProgress(key);

            //if the first 15 levels haven't been beaten for the level pack
            if (previousStarsAwarded == 0) {
                SelectLevelPack(levelPackNameEnum);
                //Debug.Log("ZERO STARS FOR LEVEL 15");
            //if the first 15 levels have been beaten
            } else {
                GameManager.Singleton.UIController.ShowView(ViewName.LevelPackSelectView);
                //Debug.Log("HAS STARS FOR LEVEL 15");
            }
        }

        public void HomeButtonClick()
        {
            AudioManager.Singleton.Play(SoundType.ButtonPress);
            GameManager.Singleton.UIController.ShowView(ViewName.StartView);
        }
    
        public void SelectOriginalLevelPack()
        {
            SelectLevelPack(LevelPackName.Original);
        }

        public void SelectSquareLevelPack()
        {
            SelectLevelPack(LevelPackName.Square);
        }

        public void SelectLineLevelPack()
        {
            SelectLevelPack(LevelPackName.Line);
        }

        public void SelectZigZagLevelPack()
        {
            SelectLevelPack(LevelPackName.ZigZag);
        }

        public void SelectShapeLevelPack()
        {
            SelectLevelPack(LevelPackName.Shape);
        }

        public void SelectPolygonLevelPack()
        {
            SelectLevelPack(LevelPackName.Polygon);
        }

        public void SelectLoopLevelPack()
        {
            SelectLevelPack(LevelPackName.Loop);
        }

        public void SelectGroupLevelPack()
        {
            SelectLevelPack(LevelPackName.Group);
        }

        public void SelectAsymetricalLevelPack()
        {
            
        }

        public void SelectRectangleLevelPack()
        {
            SelectLevelPack(LevelPackName.Rectangle);
        }

        private IEnumerator InitLevelPackID()
        {
            int usedLevelPackID = 0;

            //wait until player age has been set
            yield return new WaitUntil(() => AnalyticsManager.playerAge != 0);
            
            //if they're not old enough for analytics to be on just use random level pack
            if (AnalyticsManager.playerAge < AnalyticsManager.minAge) {
                usedLevelPackID = UnityEngine.Random.Range(0, levelPacks.Count);
                //Debug.Log("Analytics Off. Local Level Pack ID: " + usedLevelPackID);
            } else {
                yield return new WaitUntil(() => GameAnalytics.IsRemoteConfigsReady() || waitTimer >= 2.0f);
                //retrieve level pack from remote config
                string levelPackString = GameAnalytics.GetRemoteConfigsValueAsString("LevelPack");
                
                //parse level pack string as an int so its usable
                if (levelPackString != null && int.TryParse(levelPackString, out int remoteLevelPackID)) {
                    usedLevelPackID = remoteLevelPackID;
                    //Debug.Log("Remote Level Pack ID: " + remoteLevelPackID);
                } else {
                    //remote level pack id wasn't usable so we'll set it to a random one
                    usedLevelPackID = UnityEngine.Random.Range(0, levelPacks.Count);
                    //Debug.Log("Local Level Pack ID: " + usedLevelPackID);
                }
            }
            
            levelPackID = usedLevelPackID;
            GameManager.Singleton.ProgressManager.UpdateLevelPack(levelPackID);
            GameManager.Singleton.ProgressManager.Save();
            UpdateLevelPackSelectScreen();
        }

        private void UpdateLevelPackSelectScreen()
        {
            //move to sibling index 1 ie right after heading text
            levelPacks[levelPackID].UICard.transform.SetSiblingIndex(1);
        }

        private void SelectLevelPack(LevelPackName levelPackName)
        {
            if (currentLevelPackGO != null) {
                Destroy(currentLevelPackGO);
            }

            LevelPackData currentLevelPackData = levelPacks.Where(x => x.name == levelPackName).First();
            currentLevelPackGO = Instantiate(currentLevelPackData.prefab);
            currentLevelPack = currentLevelPackGO.GetComponent<LevelPack>();
            DontDestroyOnLoad(currentLevelPackGO);

            currentLevelPackName = currentLevelPackData.name;
            currentLevelPackAlias = currentLevelPackData.alias;

            LevelManager.currentLevelPackName = currentLevelPackName;

            AudioManager.Singleton.Play(SoundType.ButtonPress);

            LevelSelectController levelSelectController = GameManager.Singleton.ControllerRegistry.TryGetValue(ViewName.LevelSelectView) as LevelSelectController;
            levelSelectController.Show();
        }
    }

}