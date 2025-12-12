using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace StudioByStorm.Obstacles.Animations {

    public abstract class Animation : MonoBehaviour
    {
        public float time = 0.25f;
        public Ease easing;
        public int direction = 0;
        public GameObject[] buildingBlocks;
        public int initialPositionTeleport = 0;//allows for customization of similar animations with different starting visual style
        public int initialRotationTeleportCount = 0;//allows for customization of similar animations with different starting visual style
        public GameObject centerPosition;

        protected bool animate = true;
        protected Vector3[] buildingBlockPositions;
        protected Quaternion[] buildingBlockRotations;
        protected int[] buildingBlockPositionIndexs;
        protected int[] buildingBlockRotationIndexs;

        private ObstaclePart[] obstacleParts;

        protected void Awake()
        {
            int size = (buildingBlocks != null) ? buildingBlocks.Length : 0;
            buildingBlockPositions = new Vector3[size];
            buildingBlockRotations = new Quaternion[size];
            buildingBlockPositionIndexs = new int[size];
            buildingBlockRotationIndexs = new int[size];
        }

        protected void Start()
        {
            if (buildingBlocks != null) {

                obstacleParts = new ObstaclePart[buildingBlocks.Length];

                for (int i = 0; i < buildingBlocks.Length; i++) {
                    buildingBlockPositions[i] = buildingBlocks[i].transform.localPosition;
                    buildingBlockRotations[i] = buildingBlocks[i].transform.localRotation;
                    buildingBlockPositionIndexs[i] = i;
                    buildingBlockRotationIndexs[i] = i;

                    obstacleParts[i] = buildingBlocks[i].GetComponent<ObstaclePart>();
                }
            }
            
            //perform initial position teleport
            for (int j = 0; j < initialPositionTeleport; j++) {
                TeleportBuildingBlocksPosition();
            }

            //perform initial rotation teleport
            for (int k = 0; k < initialRotationTeleportCount; k++) {
                TeleportBuildingBlocksRotation();
            }

            if (GameManager.Singleton.PlayerController.controlType == ControlType.Animate) {
                time /= 3.0f;

            } else if (GameManager.Singleton.PlayerController.controlType == ControlType.Tap) {
                float currentLevelID = GameManager.Singleton.LevelManager.displayLevelID * 1.0f;
                float currentChapterID = GameManager.Singleton.LevelManager.currentChapterID * 15.0f;//will be 0 or 15 for chapter 1 vs chapter 2
                float actualCurrentLevel = currentLevelID + currentChapterID;
                float maxLevel = GameManager.Singleton.LevelManager.levelChapters.chapters.Count * 15.0f;
                float percentage = actualCurrentLevel / maxLevel;
                float clampedPercent = Mathf.Min(percentage, 1.0f);
                float divisor = DOVirtual.EasedValue(1.5f, 3.0f, clampedPercent, Ease.Linear);
                time /= divisor;
            }
        }

        protected void OnDisable()
        {
            StopAllCoroutines();
        }

        public void RegisterObstaclePartsViaNodeID(int nodeID)
        {
            for (int i = 0; i < obstacleParts.Length; i++) {
                obstacleParts[i].RegisterViaNodeID(nodeID);
            }
        }

        public void Animate() 
        {
            animate = true;
            StartCoroutine(AnimationUpdate());
        }

        public void Stop() 
        {
            animate = false;
            StopCoroutine(AnimationUpdate());
            StopAllCoroutines();
        }

        protected void TeleportBuildingBlocksPosition()
        {
            for (int i = 0; i < buildingBlocks.Length; i++) {
                //check & update indexs
                int buildBlockIndex = buildingBlockPositionIndexs[i];
                buildingBlockPositionIndexs[i] = (buildBlockIndex + 1 <= buildingBlockPositionIndexs.Length - 1) ? buildBlockIndex + 1 : 0;
                //update position using new index
                int updatedBuildBlockIndex = buildingBlockPositionIndexs[i];
                buildingBlocks[i].transform.localPosition = buildingBlockPositions[updatedBuildBlockIndex];
            }
        }

        protected void TeleportBuildingBlocksRotation()
        {
            for (int i = 0; i < buildingBlocks.Length; i++) {
                //check & update indexs
                int buildBlockIndex = buildingBlockRotationIndexs[i];
                buildingBlockRotationIndexs[i] = (buildBlockIndex + 1 <= buildingBlockRotationIndexs.Length - 1) ? buildBlockIndex + 1 : 0;
                //update rotation using new index
                int updatedBuildBlockIndex = buildingBlockRotationIndexs[i];
                buildingBlocks[i].transform.localRotation = buildingBlockRotations[updatedBuildBlockIndex];
            }
        }

        protected abstract IEnumerator AnimationUpdate();

    }

}