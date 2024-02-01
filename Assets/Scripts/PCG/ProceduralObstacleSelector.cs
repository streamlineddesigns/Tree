using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using StudioByStorm.Repositories;
using StudioByStorm.Graph;
using StudioByStorm.Config;
using StudioByStorm.Data;

namespace StudioByStorm.PCG {

    public class ProceduralObstacleSelector : MonoBehaviour
    {
        [SerializeField] private Ease obstacleDifficultyEasing = Ease.InSine;
        [SerializeField] private Ease obstacleCountEasing = Ease.Linear;
        [SerializeField] private int maxLevelID = 150;
        [SerializeField] private int maxObstaclesInALevel = 20;
        [SerializeField] private float maxObstacleDifficultyScore = 100.0f;
        [SerializeField] private int highestEasierLevelID = 30;
        [SerializeField] private bool isDebuggingCurrentLevelID = false;
        [SerializeField] private int debugLevelID;

        private ObstacleDataRepository ObstacleDataRepository;
        private GraphConstructionManager GraphConstructionManager;
        private LevelConfig LevelConfig;
        private int currentLevelID;

        public void DependencyInjection(ObstacleDataRepository obr, GraphConstructionManager gcm, LevelConfig levelConfig)
        {
            ObstacleDataRepository = obr;
            GraphConstructionManager = gcm;
            LevelConfig = levelConfig;
        }
        
        public IEnumerator SelectObstacles()
        {
            //get info on the current level being made
            currentLevelID = (isDebuggingCurrentLevelID) ? debugLevelID : (GetCurrentLevelFileCount() + 1);
            Debug.LogWarning("currentLevelID: " + currentLevelID);
            float levelPercent = currentLevelID / (maxLevelID  * 1.0f);
            Debug.LogWarning("levelPercent: " + levelPercent);

            //calculate difficulty score
            float currentDifficultyScore = DOVirtual.EasedValue(0.0f, maxObstacleDifficultyScore, levelPercent, obstacleDifficultyEasing);
            Debug.LogWarning("currentDifficultyScore: " + currentDifficultyScore);

            //calculate difficulty score range
            float MinInclusiveOffset = 5.0f;
            float MaxInclusiveOffset = 10.0f;
            //calculate what percent we are through the highest purposely easier level
            float highestEasierLevelPercent = currentLevelID / (highestEasierLevelID  * 1.0f);
            Debug.LogWarning("highestEasierLevelPercent: " + highestEasierLevelPercent);
            float easierLevelRangeOffset = ((MaxInclusiveOffset - MinInclusiveOffset) * highestEasierLevelPercent) + MaxInclusiveOffset;
            //calculate the range offset ie the number we'll modify our currentDifficultyScore with
            float rangeOffset = (currentLevelID <= highestEasierLevelID) ? (easierLevelRangeOffset) : MaxInclusiveOffset;

            Debug.LogWarning("rangeOffset: " + rangeOffset);

            //set difficulty score range
            float lowestDifficultyScore = Mathf.Max(currentDifficultyScore - rangeOffset, 0.0f);
            float highestDifficultyScore = currentDifficultyScore + rangeOffset;

            //override to 0 for now due to lack of in range obstacles for a lot of difficulty scores
            lowestDifficultyScore = 0.0f;

            Debug.LogWarning("lowestDifficultyScore: " + lowestDifficultyScore);
            Debug.LogWarning("highestDifficultyScore: " + highestDifficultyScore);

            //select obstacles in that range
            List<ObstacleData> possibleObstacles = ObstacleDataRepository.data.Where(x => x.difficultyScore >= lowestDifficultyScore &&
                                                                                  x.difficultyScore <= highestDifficultyScore &&
                                                                                  (x.obstacleType == ObstacleType.SingleNode || x.obstacleType == ObstacleType.DoubleNode || x.obstacleType == ObstacleType.BetweenNode)).ToList();
            //shuffle the obstacles
            Shuffle shuffle = new Shuffle();
            possibleObstacles = shuffle.FisherYates(possibleObstacles);

            //not going to use level percent. We want an additional small percent added so that an obstacle can appear in a lower level than typical
            float percentOffset = Mathf.Min((0.035f + levelPercent), 100.0f);
            //determine how many obstacles might be in the level
            int targetObstacleCount = (int) DOVirtual.EasedValue(0.0f, maxObstaclesInALevel, percentOffset, obstacleCountEasing);
            Debug.LogWarning("targetObstacleCount: " + targetObstacleCount);

            //grab them from possible obstacles
            List<ObstacleData> obstaclesToUse = possibleObstacles.Take(targetObstacleCount).ToList();

            //add them to the level
            GraphConstructionManager.GlobalLevelData.obstacleNames = obstaclesToUse.Select(x => x.name).ToList();

            yield return null;
        }

        protected int GetCurrentLevelFileCount()
        {
            string dir = Application.persistentDataPath + "/" + LevelConfig.subfolder;
            if (! Directory.Exists(dir)) {
                Directory.CreateDirectory(dir);
            }
            int LevelFileCountInDir = Directory.GetFiles(dir, "*", SearchOption.AllDirectories).Length;

            return LevelFileCountInDir;
        }
    }

}