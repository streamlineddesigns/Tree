using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using StudioByStorm.Repositories;
using StudioByStorm.Graph;
using StudioByStorm.Data;

namespace StudioByStorm.PCG {

    public class ProceduralObstacleSelector : MonoBehaviour
    {
        private ObstacleDataRepository ObstacleDataRepository;
        private GraphConstructionManager GraphConstructionManager;
        private int MaxLevel;

        public void DependencyInjection(ObstacleDataRepository obr, GraphConstructionManager gcm, int maxLevel)
        {
            ObstacleDataRepository = obr;
            GraphConstructionManager = gcm;
            MaxLevel = maxLevel;
        }
        
        public IEnumerator SelectObstacles()
        {
            //set difficulty score range
            float lowestDifficultyScore = 0.0f;
            float highestDifficultyScore = 100.0f;

            //select obstacles in that range
            List<ObstacleData> possibleObstacles = ObstacleDataRepository.data.Where(x => x.difficultyScore >= lowestDifficultyScore &&
                                                                                  x.difficultyScore <= highestDifficultyScore &&
                                                                                  (x.obstacleType == ObstacleType.SingleNode || x.obstacleType == ObstacleType.DoubleNode || x.obstacleType == ObstacleType.BetweenNode)).ToList();
            //shuffle the obstacles
            Shuffle shuffle = new Shuffle();
            possibleObstacles = shuffle.FisherYates(possibleObstacles);

            //set target amount of obstacles to use
            int targetObstacleCount = 10;
            //grab them from possible obstacles
            List<ObstacleData> obstaclesToUse = possibleObstacles.Take(targetObstacleCount).ToList();

            //add them to the level
            GraphConstructionManager.GlobalLevelData.obstacleNames = obstaclesToUse.Select(x => x.name).ToList();

            yield return null;
        }
    }

}