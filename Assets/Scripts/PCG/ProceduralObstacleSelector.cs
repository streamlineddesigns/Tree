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
        
        /*
         * order by difficulty score
         * then using the level percent to see what obstacle index we'd be at based on the obstacle count
         * then select within the range to create an in inRangePossibleObstacles list
         * then select everything under that range for an underRangePossibleObstacle list
         * then in a loop, create selectedObstacleList
         * if selectedObstacleList.Count >= inRangePossibleObstacles.Count + underRangePossibleObstacle.Count then break out
         * if selectedObstacleList.Count >= targetObstacleCount then break out
         * then simply randomly choose which list to pick from inRangePossibleObstacles vs underRangePossibleObstacle and do something like 75/25 or 50/50
         */

        public IEnumerator SelectObstacles()
        {
            //get current level id and level percent
            currentLevelID = (isDebuggingCurrentLevelID) ? debugLevelID : (GetCurrentLevelFileCount() + 1);
            float levelPercent = currentLevelID / (maxLevelID  * 1.0f);
            
            //get target obstacle index using level percent
            int targetObstacleIndex = (int) ((ObstacleDataRepository.data.Count * 1.0f) * levelPercent);
            //get possible obstacles simply by ordering by difficulty score
            List<ObstacleData> possibleObstacles = ObstacleDataRepository.data.OrderBy(x => x.difficultyScore).ToList();

            //get min and max obstacle index by +/- 5
            int minObstacleIndex = targetObstacleIndex - 5;
            int maxObstacleIndex = targetObstacleIndex + 5;
        
            //IN range is simply index >= min && index <= max and then we shuffle
            List<ObstacleData> inRangePossibleObstacles = ObstacleDataRepository.data.Where((x, index) => (index >= minObstacleIndex && index <= maxObstacleIndex)).ToList();
            Shuffle inRangeShuffle = new Shuffle();
            inRangePossibleObstacles = inRangeShuffle.FisherYates(inRangePossibleObstacles);

            //UNDER range is simply index <= min and then we shuffle
            List<ObstacleData> underRangePossibleObstacles = ObstacleDataRepository.data.Where((x, index) => (index <= minObstacleIndex)).ToList();
            Shuffle underRangeShuffle = new Shuffle();
            underRangePossibleObstacles = underRangeShuffle.FisherYates(underRangePossibleObstacles);

            //target obstacle count is our level percent interpolated based on the the min and max obstacles in a level an our easing fucntion
            float percentOffset = Mathf.Min((0.035f + levelPercent), 100.0f);
            int targetObstacleCount = (int) DOVirtual.EasedValue(0.0f, maxObstaclesInALevel, percentOffset, obstacleCountEasing);

            List<ObstacleData> selectedObstacleList = new List<ObstacleData>();

            bool isSearching = true;
            int inRangeIndex = 0;
            int underRangeIndex = 0;

            while(isSearching) {

                int selectedObstacleCount = selectedObstacleList.Count;
                
                if (selectedObstacleCount >= targetObstacleCount || selectedObstacleCount >= possibleObstacles.Count) {
                    isSearching = false;
                }

                //ie a 50% chance of using the in range obstacles over the under range obstacles
                bool useInRange = (UnityEngine.Random.Range(1.0f, 10.0f) >= 5.0f);
                if (useInRange && inRangeIndex < inRangePossibleObstacles.Count) {
                    selectedObstacleList.Add(inRangePossibleObstacles[inRangeIndex]);
                    inRangeIndex++;
                } else if (! useInRange && underRangeIndex < underRangePossibleObstacles.Count) {
                    selectedObstacleList.Add(underRangePossibleObstacles[underRangeIndex]);
                    underRangeIndex++;
                }
                yield return null;
            }

            //grab them from possible obstacles
            List<ObstacleData> obstaclesToUse = selectedObstacleList.Take(targetObstacleCount).ToList();
            //add them to the level
            GraphConstructionManager.GlobalLevelData.obstacleNames = obstaclesToUse.Select(x => x.name).ToList();

            yield return null;
        }

        /*public IEnumerator SelectObstacles()
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
        }*/

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