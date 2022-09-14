using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using StudioByStorm.Data;
using StudioByStorm.EventPublishers;

namespace StudioByStorm {

    public class LevelManager: MonoBehaviour
    {
        public Node Node;
        public List<Node> CurrentLevel = new List<Node>();
        public LevelData CurrentLevelData;
        public int currentLevelID;
        protected List<float[]> nodePositions;

        void OnEnable()
        {
            GameEventPublisher.OnStateChange += OnStateChange;
        }

        void OnDisable()
        {
            GameEventPublisher.OnStateChange -= OnStateChange;
        }

        public void OnStateChange(GameState state)
        {
            if (state == GameState.GameStart) {
                LoadLevel();
                GameStart();
            }
        }

        public void LoadLevel()
        {
            string json = "";
            string fileLine;

            string dir = Application.persistentDataPath;
            string LevelSaveFilePath = (dir + GameManager.Singleton.LevelConfig.fileNameAppend + currentLevelID + GameManager.Singleton.LevelConfig.fileNamePrepend).ToString();
            System.IO.StreamReader file = new System.IO.StreamReader(LevelSaveFilePath);  
            while((fileLine = file.ReadLine()) != null)  
            {  
                json += fileLine;
            }

            file.Close();

            CurrentLevelData = JsonConvert.DeserializeObject<LevelData>(json);
            Debug.Log("Loaded Saved LevelData: " + LevelSaveFilePath);
        }

        protected void GameStart()
        {
            nodePositions = new List<float[]>();
            
            int ID = 0;
            //iterate over the level data layers
            for (int i = 0; i < CurrentLevelData.Layers.Count; i++) {
                //iterate over nodes in each layer
                for (int j = 0; j < CurrentLevelData.Layers[i].nodeCount; j++) {
                    Node currentNode = Instantiate(Node, GameManager.Singleton.SpatialHashManager.nodeParent.transform);
                    currentNode.gameObject.transform.position = new Vector2(CurrentLevelData.Layers[i].nodePositions[j].x, CurrentLevelData.Layers[i].nodePositions[j].y);
                    nodePositions.Add(new float[2]{currentNode.gameObject.transform.position.x, currentNode.gameObject.transform.position.y});
                    currentNode.ID = ID;
                    currentNode.NodeColor = CurrentLevelData.Layers[i].nodeColors[j];
                    currentNode.NodeType = CurrentLevelData.Layers[i].nodeTypes[j];
                    currentNode.gameObject.SetActive(true);
                    ID++;
                }
            }

            float[] coords = ML.Math.GetCentroid(nodePositions.ToArray());
            GameManager.Singleton.player.transform.position = new Vector2(coords[0], coords[1]);

        }
    }

}