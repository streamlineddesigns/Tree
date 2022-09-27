using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using StudioByStorm.Data;
using StudioByStorm.EventPublishers;
using StudioByStorm.Optimizations;
using StudioByStorm.Graph;
using StudioByStorm;

namespace StudioByStorm {

    public class LevelManager: MonoBehaviour
    {
        public int currentLevelNodeCount;
        public int currentLevelEdgeCount;
        public int currentLevelParentCount;
        public Dictionary<NodeColor, bool> parentColorsConnected = new Dictionary<NodeColor, bool>();
        public Transform EdgeRendererParent;
        public GameObject EdgeRenderer;
        public GameObject Edge;
        public GameObject Node;
        public List<Node> CurrentLevel = new List<Node>();
        public LevelData CurrentLevelData;
        public int currentLevelID;
        protected Pool NodePool;
        protected Pool EdgeRendererPool;
        protected List<float[]> nodePositions;

        void Awake()
        {
            NodePool = ScriptableObject.CreateInstance<Pool>();
            EdgeRendererPool = ScriptableObject.CreateInstance<Pool>();
        }

        void Start()
        {
            NodePool.DependencyInjection(Node, GameManager.Singleton.SpatialHashManager.nodeParent.transform, 25);
            EdgeRendererPool.DependencyInjection(EdgeRenderer, EdgeRendererParent, 40);
        }

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

        protected void CleanUpOnGameStart()
        {
            GameManager.Singleton.AdjacencyList.Clear();
            EdgeRendererPool.DeactivateAll();
            NodePool.DeactivateAll();
            ActionController actionController = GameManager.Singleton.ControllerRegistry.TryGetValue(ViewName.ActionView) as ActionController;
            actionController.ActionModel.CurrentEdge = null;
        }

        protected void GameStart()
        {
            CleanUpOnGameStart();

            nodePositions = new List<float[]>();
            
            int ID = 0;
            //iterate over the level data layers
            for (int i = 0; i < CurrentLevelData.Layers.Count; i++) {
                //iterate over nodes in each layer
                for (int j = 0; j < CurrentLevelData.Layers[i].nodeCount; j++) {
                    int currentNodeID = j;
                    Node currentNode = NodePool.Get().GetComponent<Node>();//no more Instantiate(Node, GameManager.Singleton.SpatialHashManager.nodeParent.transform);
                    currentNode.gameObject.transform.position = new Vector2(CurrentLevelData.Layers[i].nodePositions[j].x, CurrentLevelData.Layers[i].nodePositions[j].y);
                    nodePositions.Add(new float[2]{currentNode.gameObject.transform.position.x, currentNode.gameObject.transform.position.y});
                    currentNode.ID = ID;
                    currentNode.NodeColor = CurrentLevelData.Layers[i].nodeColors[j];
                    if (! parentColorsConnected.ContainsKey(currentNode.NodeColor)) {
                        parentColorsConnected.Add(currentNode.NodeColor, false);
                    }
                    currentNode.NumOfConnections = 0;
                    currentNode.NodeType = CurrentLevelData.Layers[i].nodeTypes[j];
                    currentNode.gameObject.SetActive(true);
                    currentLevelParentCount = (currentNode.NodeType == NodeType.Parent) ? currentLevelParentCount + 1 : currentLevelParentCount;
                    currentLevelNodeCount++;
                    ID++;
                }
            }

            SetEdgeRenderers();

            float[] coords = ML.Math.GetCentroid(nodePositions.ToArray());
            GameManager.Singleton.player.transform.position = new Vector2(coords[0], coords[1]);
            GameManager.Singleton.FXManager.SeaDust.transform.position = new Vector2(coords[0], coords[1]);

        }

        protected void SetEdgeRenderers()
        {
            for (int i = 0; i < nodePositions.Count; i++) {
                int currentNodeID = i;
                for (int k = 0; k < CurrentLevelData.AdjacencyListData[currentNodeID].Count; k++) {
                    int connectedNodeID = CurrentLevelData.AdjacencyListData[currentNodeID][k];
                        
                    LineRenderer currentEdgeRenderer = EdgeRendererPool.Get().GetComponent<LineRenderer>();
                    currentEdgeRenderer.SetPosition(0, new Vector3(nodePositions[currentNodeID][0], nodePositions[currentNodeID][1], 0.0f));
                    currentEdgeRenderer.SetPosition(1, new Vector3(nodePositions[connectedNodeID][0], nodePositions[connectedNodeID][1], 0.0f));
                    currentEdgeRenderer.gameObject.SetActive(true);
                }
            }
        }

    }

}