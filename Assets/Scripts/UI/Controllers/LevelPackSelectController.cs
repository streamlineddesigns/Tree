using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StudioByStorm.Data.LevelPacks;
using StudioByStorm.Data.LevelChapters;
using GameAnalyticsSDK;
using StudioByStorm.PCG;

namespace StudioByStorm.UI.Controllers {

    public class LevelPackSelectController : Controller
    {
        public List<LevelPackProgressionData> progressionData = new List<LevelPackProgressionData>();
        public List<LevelPackData> levelPacks = new List<LevelPackData>();
        public int levelPackID = -1;
        public bool isRunning;
        
        public static bool bNeedsToCountStars = true;
        public static LevelPackName currentLevelPackName;
        public static string currentLevelPackAlias;
        public static GameObject currentLevelPackGO;
        public static LevelPack currentLevelPack;

        private bool isABTesting = false;

        private float waitTime = 5.0f;
        private float waitTimer = 0.0f;

        protected void Start()
        {
            if (GameManager.Singleton.ProgressManager.GetLevelPack() != -1) {
                levelPackID = GameManager.Singleton.ProgressManager.GetLevelPack();
                if (isABTesting) UpdateLevelPackSelectScreen();
            } else {
                StartCoroutine(InitLevelPackID());
            }

            if (LevelPackSelectController.bNeedsToCountStars) UpdateLevelPackProgressionData();
        }

        protected void Update()
        {
            if (AnalyticsManager.playerAge != 0 && waitTimer < waitTime) {
                waitTimer += Time.deltaTime;
            }
        }

        public IEnumerator Show()
        {
            isRunning = true;

            yield return new WaitUntil(() => levelPackID != -1);
            _Show();

            isRunning = false;
        }

        protected void _Show()
        {
            //Debug.Log("here");

            LevelPackName levelPackNameEnum = (LevelPackName) levelPackID;
            string levelPackNameString = levelPackNameEnum.ToString();
            int currentChapterID = 0;
            int currentLevelID = 14;//0 starting index so it's 15
            string key = (levelPackNameString + "-" + currentChapterID + "-" + currentLevelID);
            int previousStarsAwarded = GameManager.Singleton.ProgressManager.GetLevelProgress(key);

            //if the first 15 levels haven't been beaten for the level pack
            if ((previousStarsAwarded == 0 && false) || FTUEManager.singleton.isFirstOpen) {
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
            //default to square ie id = level pack enum cast to int
            int usedLevelPackID = 1;

            if (isABTesting) {
                //wait until player age has been set
                yield return new WaitUntil(() => AnalyticsManager.playerAge != 0);
                
                //if they're not old enough for analytics to be on just use random level pack
                if (AnalyticsManager.playerAge < AnalyticsManager.minAge) {
                    usedLevelPackID = UnityEngine.Random.Range(0, levelPacks.Count);
                    //Debug.Log("Analytics Off. Local Level Pack ID: " + usedLevelPackID);
                } else {
                    while (waitTimer < waitTime) {
                        if (GameAnalytics.IsRemoteConfigsReady()) {
                            waitTimer = waitTime;
                        }

                        yield return new WaitForSeconds(0.25f);
                    }
                    //retrieve level pack from remote config
                    string levelPackString = GameAnalytics.GetRemoteConfigsValueAsString("LevelPack");
                    
                    //parse level pack string as an int so its usable
                    if (levelPackString != null && int.TryParse(levelPackString, out int remoteLevelPackID)) {
                        usedLevelPackID = remoteLevelPackID;
                        Debug.Log("Remote Level Pack ID: " + remoteLevelPackID);
                    } else {
                        //remote level pack id wasn't usable so we'll set it to square!
                        usedLevelPackID = 1;
                        Debug.Log("Local Level Pack ID: " + usedLevelPackID);
                    }
                }
            }

            levelPackID = usedLevelPackID;
            GameManager.Singleton.ProgressManager.UpdateLevelPack(levelPackID);
            if (isABTesting) InitLevelPackOrder();
            GameManager.Singleton.ProgressManager.Save();
            if (isABTesting) UpdateLevelPackSelectScreen();
        }

        private void InitLevelPackOrder()
        {
            Shuffle shuffle = new Shuffle();
            List<LevelPackData> toShuffleLevelPacks = new List<LevelPackData>(levelPacks);
            List<LevelPackData> shuffledLevelPacks = shuffle.FisherYates(toShuffleLevelPacks);
            List<LevelPackData> filteredLevelPacks = new List<LevelPackData>();

            //add the primary level pack first
            filteredLevelPacks.Add(levelPacks[levelPackID]);

            for (int i = 0; i < shuffledLevelPacks.Count; i++) {
                //ensure the primmary level pack doesn't get added twice
                if (shuffledLevelPacks[i].name != (LevelPackName) levelPackID) {
                    filteredLevelPacks.Add(shuffledLevelPacks[i]);
                }
            }

            //add the level pack order to the users save data
            for (int i = 0; i < filteredLevelPacks.Count; i++) {
                GameManager.Singleton.ProgressManager.UpdateLevelPackOrder(filteredLevelPacks[i].name, i);
                //log to verify proper order
                //Debug.Log("Name: " + filteredLevelPacks[i].name + " Order: " + i);
            }
        }

        private void UpdateLevelPackSelectScreen()
        {
            LevelPackData[] reOrderedLevelPacks = new LevelPackData[levelPacks.Count];

            for (int i = 0; i < levelPacks.Count; i++) {
                int currentIndex = GameManager.Singleton.ProgressManager.GetLevelPackOrder(levelPacks[i].name);
                reOrderedLevelPacks[currentIndex] = levelPacks[i];
            }
            
            //resort the level pack cards ie the ui on the level pack select screen
            for (int i = 0; i < reOrderedLevelPacks.Length; i++) {
                //$$test now we'll only resort the first level pack
                /*if (i == 0)*/ reOrderedLevelPacks[i].UICard.transform.SetSiblingIndex(i + 1);
            }

            //move to sibling index 1 ie right after heading text
            //levelPacks[levelPackID].UICard.transform.SetSiblingIndex(1);
        }

        private void UpdateLevelPackProgressionData()
        {
            
            int totalStarCount = 0;

            //iterate over all level packs/chapters/levels to get total star count
            for (int i = 0; i < progressionData.Count; i++) {
                for (int j = 0; j < progressionData[i].chaptersCount; j++) {
                    for (int k = 0; k < progressionData[i].levelPerChapterCount; k++) {
                        //level pack name
                        LevelPackName levelPackNameEnum = (LevelPackName) progressionData[i].name;
                        string levelPackNameString = levelPackNameEnum.ToString();
                        //level pack key
                        string key = (levelPackNameString + "-" + j + "-" + k);
                        //get previous stars awarded
                        int previousStarsAwarded = GameManager.Singleton.ProgressManager.GetLevelProgress(key);
                        //add up total stars
                        totalStarCount += previousStarsAwarded;
                    }
                }
            }

            //start at 1 because first is always unlocked
            for (int l = 1; l < progressionData.Count; l++) {
                bool isLocked = (totalStarCount < progressionData[l].starsToUnlock);
                float dividend = totalStarCount * 1.0f;
                float divisor = progressionData[l].starsToUnlock * 1.0f;
                float percentage = dividend / divisor;
                int percentageInteger = (int) (percentage * 100.0f);
                string percentageString = percentageInteger.ToString() + "%";

                if (isLocked) {
                    progressionData[l].playButton.SetActive(false);
                    progressionData[l].lockedButton.SetActive(true);
                    progressionData[l].lockedBanner.SetActive(true);
                    progressionData[l].progressBar.value = percentage;
                    progressionData[l].progressPercentText.gameObject.SetActive(true);
                    progressionData[l].progressPercentText.text = percentageString;

                } else {
                    progressionData[l].playButton.SetActive(true);
                    progressionData[l].lockedButton.SetActive(false);
                    progressionData[l].lockedBanner.SetActive(false);
                    progressionData[l].progressBar.value = 1.0f;
                    progressionData[l].progressPercentText.gameObject.SetActive(false);
                }
            }

            //Debug.LogError(totalStarCount);
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
            currentLevelPackAlias = currentLevelPackData.aliasTranslations[GameManager.Language];

            LevelManager.currentLevelPackName = currentLevelPackName;

            AudioManager.Singleton.Play(SoundType.ButtonPress);

            LevelSelectController levelSelectController = GameManager.Singleton.ControllerRegistry.TryGetValue(ViewName.LevelSelectView) as LevelSelectController;
            levelSelectController.Show();
        }
    }

}