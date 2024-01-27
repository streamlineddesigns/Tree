using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StudioByStorm.Data;
using StudioByStorm.Helpers;
using StudioByStorm.Obstacles.Animations;

namespace StudioByStorm.Obstacles {

    public class ObstacleDifficultyManager : MonoBehaviour
    {
        public CircleCastHelper CircleCastHelper;
        public CompositeAnimation[] CompositeAnimations;
        public float timeToRecord = 60.0f;

        private CompositeAnimation CompositeAnimation;
        private GameObject[] obstacleParts;
        private Bounds partBounds;
        private bool isRecording;

        void Start()
        {
            for (int i = 0; i < CompositeAnimations.Length; i++) {
                CompositeAnimations[i].gameObject.SetActive(false);
            }

            StartCoroutine(AssignDifficulty());
        }

        IEnumerator AssignDifficulty()
        {
            for (int i = 0; i < CompositeAnimations.Length; i++) {
                //get the obstacle
                CompositeAnimation = CompositeAnimations[i];
                //activate it
                CompositeAnimation.gameObject.SetActive(true);
                //center it
                CenterObstacle();
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
        }

        protected void CenterObstacle()
        {
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

                lightCount += CircleCastHelper.colorTypes.Count(x => x == ColorType.Light);
                darkCount += CircleCastHelper.colorTypes.Count(x => x == ColorType.Dark);
                defaultCount += CircleCastHelper.colorTypes.Count(x => x == ColorType.Default);

                ObstaclePartData tempObstaclePartData = GetMaxDistance();
                foreach (ColorType ct in Enum.GetValues(typeof(ColorType))) {
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


            foreach (ColorType ct in Enum.GetValues(typeof(ColorType))) {
                if (maxObstaclePartData.maxConsecutiveColorTypeCount.ContainsKey(ct)) Debug.Log("maxConsecutiveColorTypeCount - " + ct.ToString() + ": " + maxObstaclePartData.maxConsecutiveColorTypeCount[ct]);
                if (maxObstaclePartData.maxColorDistance.ContainsKey(ct)) Debug.Log("maxColorDistance - " + ct.ToString() + ": " + maxObstaclePartData.maxColorDistance[ct]);

                if (totalObstaclePartData.maxConsecutiveColorTypeCount.ContainsKey(ct)) Debug.Log("TOTAL maxConsecutiveColorTypeCount - " + ct.ToString() + ": " + totalObstaclePartData.maxConsecutiveColorTypeCount[ct] / iterationCount);
                if (totalObstaclePartData.maxColorDistance.ContainsKey(ct)) Debug.Log("TOTAL maxColorDistance - " + ct.ToString() + ": " + totalObstaclePartData.maxColorDistance[ct] / iterationCount);
            }
        }

        protected ObstaclePartData GetMaxDistance()
        {
            ObstaclePartData obstaclePartData = new ObstaclePartData();

            Dictionary<ColorType, float> maxColorDistance = new Dictionary<ColorType, float>();
            //ColorType : List of indexes to CircleCastHelper.colorTypes which are the same type
            Dictionary<ColorType, List<int>> consecutiveColorTypes = new Dictionary<ColorType, List<int>>();
            Dictionary<ColorType, int> maxConsecutiveColorTypeCount = new Dictionary<ColorType, int>();

            ColorType currentColorType = ColorType.PurposelyUnassigned;
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

            foreach (ColorType ct in Enum.GetValues(typeof(ColorType))) {
                    if (! maxColorDistance.ContainsKey(ct)) {
                        maxColorDistance.Add(ct, 0.0f);
                    }

                    float totalDistance = 0.0f;

                    if (consecutiveColorTypes.ContainsKey(ct)) {
                        for (int j = 0; j < consecutiveColorTypes[ct].Count; j++) {
                            int indexOne = consecutiveColorTypes[ct][j];
                            if ((j + 1) < consecutiveColorTypes[ct].Count - 1 && CircleCastHelper.hitGameObjects[indexOne] != null) {
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