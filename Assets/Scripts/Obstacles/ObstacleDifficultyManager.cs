using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;
using DG.Tweening;
using StudioByStorm.Data;
using StudioByStorm.Repositories;
using StudioByStorm.Helpers;
using StudioByStorm.Obstacles.Animations;

namespace StudioByStorm.Obstacles {

    public class ObstacleDifficultyManager : MonoBehaviour
    {
        public ObstacleDataRepository obstacleDataRepository;
        public CircleCastHelper CircleCastHelper;
        public GameObject ObstacleContainer;
        public float timeToRecord = 60.0f;
        [Range(0, 4)] public float timeScale = 1.0f;
        public Ease easing = Ease.InSine;

        private List<CompositeAnimation> CompositeAnimations = new List<CompositeAnimation>();
        private CompositeAnimation CompositeAnimation;
        private GameObject[] obstacleParts;
        private Bounds partBounds;
        private bool isRecording;
        private string currentObstacleName;
        [SerializeField] private bool bJustCreateReport = false;

        void Awake()
        {
            Time.timeScale = timeScale;
        }

        void Start()
        {            
            //Load addressable references from the Obstacle Data Registry 
            for (int i = 0; i < obstacleDataRepository.data.Count; i++) {
                AssetReference currentAssetReference = obstacleDataRepository.data[i].assetReference;
                AsyncOperationHandle<GameObject> AsyncObstacleHandle = currentAssetReference.LoadAssetAsync<GameObject>();
                AsyncObstacleHandle.Completed += OnAsyncObstacleHandleCompleted;
            }

            if (bJustCreateReport) {
                CreateReport();
            } else {
                StartCoroutine(AssignDifficulty());
            }
        }

        private void OnAsyncObstacleHandleCompleted(AsyncOperationHandle<GameObject> handle)
        {
            if (handle.Status == AsyncOperationStatus.Succeeded) {
                GameObject result = handle.Result;
                GameObject currentObstacle = Instantiate(result, ObstacleContainer.transform) as GameObject;
                currentObstacle.SetActive(false);
                CompositeAnimations.Add(currentObstacle.GetComponent<CompositeAnimation>());
                
            } else {
                Debug.LogError("OnAsyncObstacleHandleCompleted FAILED");
            }
        }

        IEnumerator AssignDifficulty()
        {
            yield return new WaitUntil(() => CompositeAnimations.Count == obstacleDataRepository.data.Count);

            for (int i = 0; i < CompositeAnimations.Count; i++) {
                //get the obstacles name for later
                currentObstacleName = CompositeAnimations[i].gameObject.name.Replace("(Clone)", "");
                //get the obstacle
                CompositeAnimation = CompositeAnimations[i];
                //center it
                CenterObstacle();
                //activate it
                CompositeAnimation.gameObject.SetActive(true);
                //animate it
                CompositeAnimation.Animate();
                //start recording
                isRecording = true;
                StartCoroutine(Record());
                //wait for a minute
                yield return new WaitForSeconds(timeToRecord);
                //stop recording
                isRecording = false;
                yield return new WaitForSeconds(1.0f);
                StopCoroutine(Record());
                //deactivate it
                CompositeAnimation.gameObject.SetActive(false);
            }

            yield return new WaitForSeconds(1.0f);
            RescaleObstacleDifficulty();
            CreateReport();
            Save();
        }

        protected void CenterObstacle()
        {
            GameObject[] centerParts = CompositeAnimation.animations.Select<Animations.Animation, GameObject>(x => x.centerPosition).ToArray();
            GameObject centerPart = null;
            bool foundCenterPart = false;

            for (int i = 0; i < centerParts.Length; i++) {
                if (centerParts[i] != null) {
                    centerPart = centerParts[i];
                    foundCenterPart = true;
                    break;
                }
            }

            if (foundCenterPart) {
                CompositeAnimation.gameObject.transform.position = centerPart.transform.position;
                return;
            }

            //Get all the composite animation's parts
            obstacleParts = CompositeAnimation.animations.SelectMany<Animations.Animation, GameObject>(x => x.buildingBlocks).ToArray();
            //calculate an AABB on the part's positions
            partBounds = ML.Math.ComputeAABB(obstacleParts.Select(x => x.transform.position).ToList());
            //using this, we can determine the offset from the screen's center position 
            Vector3 CompositeAnimationOffset = Vector3.zero - partBounds.center;
            //then we can add this offset to the composite animation's position to center it properly
            Vector3 CompositeAnimationTargetPosition = CompositeAnimation.gameObject.transform.position + CompositeAnimationOffset;
            CompositeAnimation.gameObject.transform.position = CompositeAnimationTargetPosition;
        }

        IEnumerator Record()
        {
            int lightCount = 0;
            int darkCount = 0;
            int defaultCount = 0;
            ObstaclePartData totalObstaclePartData = new ObstaclePartData();
            ObstaclePartData maxObstaclePartData = new ObstaclePartData();
            int iterationCount = 0;

            while(isRecording) {
                
                CircleCastHelper.SensorRayCast();

                lightCount += CircleCastHelper.colorTypes.Count(x => x == NodeColor.Green);
                darkCount += CircleCastHelper.colorTypes.Count(x => x == NodeColor.Black);
                defaultCount += CircleCastHelper.colorTypes.Count(x => x == NodeColor.GrayScale);

                ObstaclePartData tempObstaclePartData = GetMaxDistance();
                foreach (NodeColor ct in Enum.GetValues(typeof(NodeColor))) {
                    //get highest max color distance
                    if ( (tempObstaclePartData.maxColorDistance.ContainsKey(ct) && ! maxObstaclePartData.maxColorDistance.ContainsKey(ct)) ||
                         (tempObstaclePartData.maxColorDistance.ContainsKey(ct) && maxObstaclePartData.maxColorDistance.ContainsKey(ct) && tempObstaclePartData.maxColorDistance[ct] > maxObstaclePartData.maxColorDistance[ct])) {
                            maxObstaclePartData.maxColorDistance.Remove(ct);
                            maxObstaclePartData.maxColorDistance.Add(ct, tempObstaclePartData.maxColorDistance[ct]);
                    }

                    //get highest max consecutive color type count
                    if ( (tempObstaclePartData.maxConsecutiveColorTypeCount.ContainsKey(ct) && ! maxObstaclePartData.maxConsecutiveColorTypeCount.ContainsKey(ct)) ||
                         (tempObstaclePartData.maxConsecutiveColorTypeCount.ContainsKey(ct) && maxObstaclePartData.maxConsecutiveColorTypeCount.ContainsKey(ct) && tempObstaclePartData.maxConsecutiveColorTypeCount[ct] > maxObstaclePartData.maxConsecutiveColorTypeCount[ct])) {
                            maxObstaclePartData.maxConsecutiveColorTypeCount.Remove(ct);
                            maxObstaclePartData.maxConsecutiveColorTypeCount.Add(ct, tempObstaclePartData.maxConsecutiveColorTypeCount[ct]);
                    }

                    //get total max color distance
                    if (tempObstaclePartData.maxColorDistance.ContainsKey(ct) && ! totalObstaclePartData.maxColorDistance.ContainsKey(ct)) {
                        totalObstaclePartData.maxColorDistance.Add(ct, tempObstaclePartData.maxColorDistance[ct]);
                    } else if (tempObstaclePartData.maxColorDistance.ContainsKey(ct) && totalObstaclePartData.maxColorDistance.ContainsKey(ct)) {
                        totalObstaclePartData.maxColorDistance[ct] += tempObstaclePartData.maxColorDistance[ct];
                    }

                    //get total max consecutive color type count
                    if (tempObstaclePartData.maxConsecutiveColorTypeCount.ContainsKey(ct) && ! totalObstaclePartData.maxConsecutiveColorTypeCount.ContainsKey(ct)) {
                        totalObstaclePartData.maxConsecutiveColorTypeCount.Add(ct, tempObstaclePartData.maxConsecutiveColorTypeCount[ct]);
                    } else if (tempObstaclePartData.maxConsecutiveColorTypeCount.ContainsKey(ct) && totalObstaclePartData.maxConsecutiveColorTypeCount.ContainsKey(ct)) {
                        totalObstaclePartData.maxConsecutiveColorTypeCount[ct] += tempObstaclePartData.maxConsecutiveColorTypeCount[ct];
                    }
                }

                iterationCount++;
                yield return null;
            }

            lightCount /= iterationCount;
            darkCount /= iterationCount;
            defaultCount /= iterationCount;

            Debug.Log("Light Count: " + lightCount);
            Debug.Log("Dark Count: " + darkCount);
            Debug.Log("Default Count: " + defaultCount);

            UpdateObstacleDifficulty(totalObstaclePartData, maxObstaclePartData, iterationCount);
        }

        /*
         * Difficulty score is calculated by taking the maximum number of obstacle parts, in a row, for each color, for each frame, over x amount of time
         * then adding up the distance between those obstacle parts ie their overall length and averaging it over the total number of frames
         * this gives us a low number for higher difficulty, so it's re-projected between 0 and the highest numerical difficulty score assigned to an obstacle ie a high number
         * then it's scaled between 0 and 100 for easier readability
         * then it gets passed through an easing function for modifying it's rate of change
         * but its split between multiple functions. This one and RescaleObstacleDifficulty()
         */
        protected void UpdateObstacleDifficulty(ObstaclePartData totalObstaclePartData, ObstaclePartData maxObstaclePartData, int iterations)
        {
            float unitMeasurement = 0;
            float totalDifficulty = 0;
            float defaultMaxConsecutiveColorTypeCount = 0;
            float defaultAverageMaxConsecutiveColorTypeCount = 0;

            float currentAverageMaxLightDistance = 0.0f;
            float currentAverageMaxDarkDistance = 0.0f;

            foreach (NodeColor ct in Enum.GetValues(typeof(NodeColor))) {
                int currentMaxConsecutiveColorTypeCount = 0;
                float currentMaxColorDistance = 0.0f;

                if (maxObstaclePartData.maxConsecutiveColorTypeCount.ContainsKey(ct)) {
                    currentMaxConsecutiveColorTypeCount = maxObstaclePartData.maxConsecutiveColorTypeCount[ct];
                    Debug.Log("maxConsecutiveColorTypeCount - " + ct.ToString() + ": " + currentMaxConsecutiveColorTypeCount);

                    /*if (ct == ColorType.Default) {
                        defaultMaxConsecutiveColorTypeCount = currentMaxConsecutiveColorTypeCount;
                    }*/
                } 
                if (maxObstaclePartData.maxColorDistance.ContainsKey(ct)) {
                    currentMaxColorDistance = maxObstaclePartData.maxColorDistance[ct];
                    Debug.Log("maxColorDistance - " + ct.ToString() + ": " + currentMaxColorDistance);

                    /*if (ct == ColorType.Light) {
                        totalDifficulty += currentMaxColorDistance;
                    }*/
                }

                if (ct == NodeColor.Green) {
                    unitMeasurement = (currentMaxConsecutiveColorTypeCount > 0) ? (currentMaxColorDistance / currentMaxConsecutiveColorTypeCount * 1.0f) : 0.0f;
                }

                float currentAverageMaxConsecutiveColorTypeCount;
                float currentAverageMaxColorDistance;

                if (totalObstaclePartData.maxConsecutiveColorTypeCount.ContainsKey(ct)) {
                    currentAverageMaxConsecutiveColorTypeCount = (totalObstaclePartData.maxConsecutiveColorTypeCount[ct] * 1.0f) / iterations;
                    Debug.Log("AVERAGE maxConsecutiveColorTypeCount - " + ct.ToString() + ": " + currentAverageMaxConsecutiveColorTypeCount);

                    if (ct == NodeColor.GrayScale) {
                        defaultAverageMaxConsecutiveColorTypeCount = currentAverageMaxConsecutiveColorTypeCount;
                    }
                }
                if (totalObstaclePartData.maxColorDistance.ContainsKey(ct)) {
                    currentAverageMaxColorDistance = totalObstaclePartData.maxColorDistance[ct] / iterations;
                    Debug.Log("AVERAGE maxColorDistance - " + ct.ToString() + ": " + currentAverageMaxColorDistance);

                    if (ct == NodeColor.Green) {
                        currentAverageMaxLightDistance += currentAverageMaxColorDistance;
                    } else if (ct == NodeColor.Black) {
                        currentAverageMaxDarkDistance += currentAverageMaxColorDistance;
                    }
                }
            }

            /*float defaultMaxDistance = defaultMaxConsecutiveColorTypeCount * 0.5f;
            Debug.Log("MODIFIED default max distance: " + defaultMaxDistance);
            totalDifficulty += defaultMaxDistance;*/

            totalDifficulty += currentAverageMaxLightDistance;

            float defaultAverageDistance = defaultAverageMaxConsecutiveColorTypeCount * 0.4f;
            Debug.Log("MODIFIED default average distance: " + defaultAverageDistance);
            //totalDifficulty += defaultAverageDistance;

            int index = obstacleDataRepository.data.FindIndex(x => x.name == currentObstacleName);
            switch(obstacleDataRepository.data[index].obstacleType) {
                case ObstacleType.SingleNode:
                    totalDifficulty = totalDifficulty * 1.0f;
                    break;
                case ObstacleType.DoubleNode:
                    totalDifficulty = totalDifficulty * 0.5f;
                    break;
            }

            obstacleDataRepository.data[index].difficultyScore = totalDifficulty;
        }

        protected void RescaleObstacleDifficulty()
        {
            //find the highest difficulty score (add 0.1f so the lowest score doesn't become 0)
            float highestDifficultyScore = obstacleDataRepository.data.Max(x => x.difficultyScore) + 0.1f;
            //set the new updated highest difficulty score to rescale to
            float rescaledHighestDifficultyScore = 100.0f;

            //re-project between 0 and the highest numerical difficulty score assigned to an obstacle
            for (int i = 0; i < obstacleDataRepository.data.Count; i++) {
                float currentDifficultyScore = obstacleDataRepository.data[i].difficultyScore;
                float percent = currentDifficultyScore / highestDifficultyScore;
                float easedDifficultyScore = DOVirtual.EasedValue(0, rescaledHighestDifficultyScore, percent, easing);
                //then update the obstacles difficulty score
                obstacleDataRepository.data[i].difficultyScore = easedDifficultyScore;
                Debug.Log("Name: " + obstacleDataRepository.data[i].name + " highestDifficultyScore: " + highestDifficultyScore + " currentDifficultyScore: " + currentDifficultyScore + " percent: " + percent + " easedDifficultyScore: " + easedDifficultyScore + " difficultyScore: " + obstacleDataRepository.data[i].difficultyScore);
            }
        }

        protected void CreateReport()
        {
            for (int i = 0; i < 10; i++) {
                int maxValue = (i * 10) + 10;
                int minValue = maxValue - 10;
                int count = obstacleDataRepository.data.Where(x => x.difficultyScore >= minValue && x.difficultyScore <= maxValue).Count();
                Debug.LogError(minValue + "-" + maxValue + ": " + count + "\n");
            }

            
            List<string> names = obstacleDataRepository.data.OrderBy(x => x.difficultyScore).Select(x => x.name).ToList();
            names.ForEach(x => Debug.LogError(x + "\n"));
        }

        protected void Save()
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(obstacleDataRepository);
            AssetDatabase.SaveAssets();
#endif
        }

        protected ObstaclePartData GetMaxDistance()
        {
            ObstaclePartData obstaclePartData = new ObstaclePartData();

            Dictionary<NodeColor, float> maxColorDistance = new Dictionary<NodeColor, float>();
            //ColorType : List of indexes to CircleCastHelper.colorTypes which are the same type
            Dictionary<NodeColor, List<int>> consecutiveColorTypes = new Dictionary<NodeColor, List<int>>();
            Dictionary<NodeColor, int> maxConsecutiveColorTypeCount = new Dictionary<NodeColor, int>();

            NodeColor currentColorType = NodeColor.GrayScale;
            int consecutiveCurrentColorType = 0;
            List<int> currentConsecutiveColorTypes = new List<int>();
            
            //iterate over the list of hit color types
            for (int i = 0; i < CircleCastHelper.colorTypes.Length; i++) {
                bool isAddingMaxDistance = false;

                //if the current color type changes, reset the consecutive color count!
                if (currentColorType != CircleCastHelper.colorTypes[i]) {
                    consecutiveCurrentColorType = 0;
                }

                //set the color type
                currentColorType = CircleCastHelper.colorTypes[i];
                
                //check if we just started or restarted & if so; reset consecutiveColorTypes 
                if (consecutiveCurrentColorType == 0) {
                    currentConsecutiveColorTypes = new List<int>();
                }

                //increment the consecutive color count
                consecutiveCurrentColorType++;

                //check the maximum consecutive count for each color and override it if it doesn't exist yet!
                if (! maxConsecutiveColorTypeCount.ContainsKey(currentColorType)) {
                    maxConsecutiveColorTypeCount.Add(currentColorType, consecutiveCurrentColorType);
                    isAddingMaxDistance = true;
                
                //check the maximum consecutive count for each color and see if it exceeds previous max
                } else if (consecutiveCurrentColorType > maxConsecutiveColorTypeCount[currentColorType]) {
                    maxConsecutiveColorTypeCount[currentColorType] = consecutiveCurrentColorType;
                    isAddingMaxDistance = true;
                }

                //check to see if we exceeded max consecutive of the current color type
                if (isAddingMaxDistance) {
                    currentConsecutiveColorTypes.Add(i);

                    if (consecutiveColorTypes.ContainsKey(currentColorType)) {
                        consecutiveColorTypes[currentColorType] = currentConsecutiveColorTypes;
                    } else {
                        consecutiveColorTypes.Add(currentColorType, currentConsecutiveColorTypes);
                    }
                    
                }
            }

            foreach (NodeColor ct in Enum.GetValues(typeof(NodeColor))) {
                    if (! maxColorDistance.ContainsKey(ct)) {
                        maxColorDistance.Add(ct, 0.0f);
                    }

                    float totalDistance = 0.0f;

                    if (consecutiveColorTypes.ContainsKey(ct)) {
                        for (int j = 0; j < consecutiveColorTypes[ct].Count; j++) {
                            int indexOne = consecutiveColorTypes[ct][j];
                            if ((j + 1) <= consecutiveColorTypes[ct].Count - 1 && CircleCastHelper.hitGameObjects[indexOne] != null) {
                                int indexTwo = consecutiveColorTypes[ct][j + 1];
                                totalDistance += Vector3.Distance(CircleCastHelper.hitGameObjects[indexOne].transform.position, 
                                                                CircleCastHelper.hitGameObjects[indexTwo].transform.position);
                            }
                        }
                    }
                    

                    maxColorDistance[ct] += totalDistance;
            }

            obstaclePartData.maxColorDistance = maxColorDistance;
            obstaclePartData.maxConsecutiveColorTypeCount = maxConsecutiveColorTypeCount;
            return obstaclePartData;
        }

        private void OnDrawGizmos()
        {
            // Set the Gizmo color (you can change this to your preferred color)
            Gizmos.color = Color.green;
            // Draw the wireframe of the AABB using the bounds
            Gizmos.DrawWireCube(partBounds.center, partBounds.size);
        }
    }

}