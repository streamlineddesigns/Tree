using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StudioByStorm.Data;
using StudioByStorm.EventPublishers;

namespace StudioByStorm {

    public class LevelController : MonoBehaviour
    {
        public Node Node;
        public List<Node> CurrentLevel = new List<Node>();
        public LevelData CurrentLevelData;

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
                GameStart();
            }
        }

        protected void GameStart()
        {
            int ID = 0;
            //iterate over the level data layers
            for (int i = 0; i < CurrentLevelData.Layers.Count; i++) {
                //iterate over nodes in each layer
                for (int j = 0; j < CurrentLevelData.Layers[i].nodeCount; j++) {
                    Node currentNode = Instantiate(Node, GameManager.Singleton.SpatialHashManager.nodeParent.transform);
                    currentNode.gameObject.transform.position = new Vector2(CurrentLevelData.Layers[i].nodePositions[j].x, CurrentLevelData.Layers[i].nodePositions[j].y);
                    currentNode.ID = ID;
                    currentNode.NodeColor = CurrentLevelData.Layers[i].nodeColors[j];
                    currentNode.NodeType = CurrentLevelData.Layers[i].nodeTypes[j];
                    currentNode.gameObject.SetActive(true);
                    ID++;
                }
            }

        }
    }

}