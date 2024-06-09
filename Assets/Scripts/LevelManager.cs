using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using StudioByStorm.Data;
using StudioByStorm.EventPublishers;
using StudioByStorm.Optimizations;
using StudioByStorm.Graph;
using StudioByStorm;
using StudioByStorm.Data.LevelChapters;
using StudioByStorm.UI.Controllers;
using StudioByStorm.Obstacles.Animations;
using StudioByStorm.Repositories;

namespace StudioByStorm {

    public class LevelManager: MonoBehaviour
    {
        public Chapters levelChapters;
        public List<string> romanNumerals;
        public Color levelCompletionColor;
        public int currentLevelNodeCount;
        public int currentLevelEdgeCount;
        public int currentLevelParentCount;
        public Dictionary<NodeColor, bool> parentColorsConnected = new Dictionary<NodeColor, bool>();
        public Transform EdgeRendererParent;
        public GameObject EdgeRenderer;
        public GameObject Edge;
        public GameObject Node;
        public ObstacleDataRepository obstacleDataRepository;
        public GameObject ObstacleContainer;
        public List<Node> CurrentLevel = new List<Node>();
        public LevelData CurrentLevelData;
        public int currentLevelID;
        public int currentChapterID;
        public int initiallyAvailableLevelRowsPerChapter = 3;
        protected Pool NodePool;
        protected Pool EdgeRendererPool;
        protected List<float[]> nodePositions;

        protected List<float[]> obstaclePositions;
        protected static List<GameObject> Obstacles = new List<GameObject>();
        protected static List<AsyncOperationHandle> ObstacleHandles = new List<AsyncOperationHandle>();

        protected int playerNodeID = -1;
        protected int nearbyCompositeAnimationID;
        protected CompositeAnimation nearbyCompositeAnimation;
        protected AdjacencyList horizontalVerticalAdjacencyList;
        protected List<int> nodeIDsWithAnimations;
        protected List<int> playingAnimations = new List<int>();

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
            GameEventPublisher.OnPlayerNodeChange += OnPlayerNodeChange;
        }

        void OnDisable()
        {
            GameEventPublisher.OnStateChange -= OnStateChange;
            GameEventPublisher.OnPlayerNodeChange -= OnPlayerNodeChange;
        }

        public void OnStateChange(GameState state)
        {
            switch(state) {
                case GameState.GameStart :
                    LoadLevel();
                    GameStart();
                    AudioManager.Singleton.Play(SoundType.GameStart);
                    break;

                case GameState.LevelComplete :
                    OnLevelComplete();
                    AudioManager.Singleton.Play(SoundType.LevelComplete);
                    break;

                case GameState.LevelLost :
                    StartCoroutine(OnLevelLost());
                    AudioManager.Singleton.Play(SoundType.LevelLost);
                    break;
            }
        }

        protected void OnPlayerNodeChange(int nodeID)
        {
            playerNodeID = nodeID;
            StartCoroutine(UpdateNearbyObstacles());
        }

        protected void OnLevelComplete()
        {
            GameManager.Singleton.PlayerController.LockMovement(true);
            LevelCompleteController levelCompleteController = GameManager.Singleton.ControllerRegistry.TryGetValue(ViewName.LevelCompleteView) as LevelCompleteController;
            levelCompleteController.Show();
            GameManager.Singleton.FXManager.LaunchFireWork();
        }

        IEnumerator OnLevelLost()
        {
            ActionController actionController = GameManager.Singleton.ControllerRegistry.TryGetValue(ViewName.ActionView) as ActionController;

            if (actionController.ActionModel.CurrentEdge != null) {
                actionController.ActionModel.CurrentEdge.gameObject.SetActive(false);
                actionController.ActionModel.CurrentEdge = null;
                GameEventPublisher.PublishPlayerEdgeChange(-1);
            }

            GameManager.Singleton.UIController.Close(ViewName.ActionView);

            yield return new WaitForSeconds(3.0f);

            GameManager.Singleton.UIController.ShowView(ViewName.LevelLostView);
        }

        public void LoadLevel()
        {
            TextAsset currentLevelTextAsset = levelChapters.chapters[currentChapterID].levels[currentLevelID].levelFile;
            CurrentLevelData = JsonConvert.DeserializeObject<LevelData>(currentLevelTextAsset.text);
            //Debug.Log("Loaded Saved LevelData: " + currentLevelTextAsset.name);
            UnloadObstacles();
            LoadLevelObstacles();
        }

        IEnumerator UpdateNearbyObstacles()
        {
            yield return new WaitUntil(() => Obstacles.Count == CurrentLevelData.obstacleNames.Count && horizontalVerticalAdjacencyList != null);
            yield return new WaitForSeconds(0.1f);   

            if (nodeIDsWithAnimations != null) {
                //get any connected nodes to the players current node
                List<int> connectedNodes = horizontalVerticalAdjacencyList.Get(playerNodeID);
                //reduce that to the list of nodes that have obstacles to animate
                List<int> connectedNodesWithAnimations = (connectedNodes != null) ? connectedNodes.Where(x => nodeIDsWithAnimations.Contains(x)).ToList() : new List<int>();
                //also add the current node if it has an animation too
                if (nodeIDsWithAnimations.Contains(playerNodeID)) {
                    connectedNodesWithAnimations.Add(playerNodeID);
                }

                //Debug.Log("Connected nodes with animations: " + connectedNodesWithAnimations.Count);

                //any animation id that is not in the playing animations list needs to be animated
                List<int> animationsToEnable = connectedNodesWithAnimations.Where(x => !playingAnimations.Contains(x)).ToList();
                animationsToEnable.ForEach(x => GameManager.Singleton.CompositeAnimationRegistry.TryGetValue(x).Animate());
                //any animation id in the playing animations list that is NOT in the connectedNodesWithAnimations list needs to be disabled
                List<int> animationsToDisable = playingAnimations.Where(x => !connectedNodesWithAnimations.Contains(x)).ToList();
                //animationsToDisable.ForEach(x => GameManager.Singleton.CompositeAnimationRegistry.TryGetValue(x).Stop());
                
                //If we want animations connected to more than one node (which are in connectedNodesWithAnimations) to stay active too
                animationsToDisable.ForEach(x => {
                    CompositeAnimation currentAnim = GameManager.Singleton.CompositeAnimationRegistry.TryGetValue(x);
                    bool hasMatch = connectedNodesWithAnimations.Any(x => currentAnim.nodeIDs.Contains(x));
                    if (! hasMatch) currentAnim.Stop();
                });

                //Debug.Log("animationsToEnable: " + animationsToEnable.Count);
                //Debug.Log("animationsToDisable: " + animationsToDisable.Count);

                //now we can update our playing animations list based on connectedNodesWithAnimations
                playingAnimations = connectedNodesWithAnimations.Select(x => x).ToList();
            }
        }

        protected void UnloadObstacles()
        {
            for (int i = 0; i < Obstacles.Count; i++) {
                if (Obstacles[i] != null) {
                    Destroy(Obstacles[i]);
                }
            }
            Obstacles = new List<GameObject>();

            for (int j = 0; j < ObstacleHandles.Count; j++) {
                Addressables.Release(ObstacleHandles[j]);
            }
            ObstacleHandles = new List<AsyncOperationHandle>();
        }

        protected void LoadLevelObstacles()
        {
            for (int i = 0; i < CurrentLevelData.obstacleNames.Count; i++) {
                string currentObstacleName = CurrentLevelData.obstacleNames[i];
                AssetReference currentAssetReference = obstacleDataRepository.data.Where(x => x.name == currentObstacleName).First().assetReference;
                AsyncOperationHandle<GameObject> AsyncObstacleHandle = currentAssetReference.LoadAssetAsync<GameObject>();
                AsyncObstacleHandle.Completed += OnAsyncObstacleHandleCompleted;
            }
        }

        private void OnAsyncObstacleHandleCompleted(AsyncOperationHandle<GameObject> handle)
        {
            if (handle.Status == AsyncOperationStatus.Succeeded) {
                GameObject result = handle.Result;
                string name = handle.Result.name;
                int obstacleIndex = CurrentLevelData.obstacleNames.IndexOf(name);
                List<int> obstacleNodeIDs = CurrentLevelData.obstacleNodeIDs[obstacleIndex];
                VectorData posVectorData = CurrentLevelData.obstaclePositions[obstacleIndex];
                VectorData rotVectorData = CurrentLevelData.obstacleRotations[obstacleIndex];
                Vector3 pos = new Vector3(posVectorData.x, posVectorData.y, posVectorData.z);
                Vector3 rot = new Vector3(rotVectorData.x, rotVectorData.y, rotVectorData.z);
                GameObject currentObstacle = Instantiate(result, pos, Quaternion.Euler(rot), ObstacleContainer.transform) as GameObject;
                currentObstacle.SetActive(true);
                CompositeAnimation currentCompositeAnimation = currentObstacle.GetComponent<CompositeAnimation>();
                currentCompositeAnimation.nodeIDs = obstacleNodeIDs;

                Obstacles.Add(currentObstacle);
                ObstacleHandles.Add(handle);
                
            } else {
                Debug.LogError("OnAsyncObstacleHandleCompleted FAILED");
            }
        }

        protected void CleanUpOnGameStart()
        {
            GameManager.Singleton.AdjacencyList.Clear();
            GameManager.Singleton.FXManager.FireworksPool.DeactivateAll();
            EdgeRendererPool.DeactivateAll();
            NodePool.DeactivateAll();
            ActionController actionController = GameManager.Singleton.ControllerRegistry.TryGetValue(ViewName.ActionView) as ActionController;
            actionController.ActionModel.CurrentEdge = null;
            GameEventPublisher.PublishPlayerEdgeChange(-1);
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
                    currentNode.gameObject.name = "Node-" +ID;
                    currentLevelParentCount = (currentNode.NodeType == NodeType.Parent) ? currentLevelParentCount + 1 : currentLevelParentCount;
                    currentLevelNodeCount++;
                    ID++;
                }
            }

            SetEdgeRenderers();

            //$$TODO we all ready have a centroid in our LevelData.. plus we can't just randomly throw the player there because there could be obstacles
            float[] coords = ML.Math.GetCentroid(nodePositions.ToArray());
            GameManager.Singleton.player.transform.position = (CurrentLevelData.PlayerStartPosition != null) ? CurrentLevelData.PlayerStartPosition : new Vector3(coords[0], coords[1], 0);
            GameManager.Singleton.FXManager.SeaDust.transform.position = GameManager.Singleton.LevelManager.CurrentLevelData.Centroid;

            //save our node ids with animations
            nodeIDsWithAnimations = CurrentLevelData.obstacleNodeIDs.SelectMany<List<int>, int>(x => x).ToList();
            //create our full adjacency list
            CreateFullAdjacencyList();
            //create our horizontal/vertical adjacency list
            StartCoroutine(createHorizontalVerticalAdjacencyList());
        }

        IEnumerator createHorizontalVerticalAdjacencyList()
        {
            yield return new WaitUntil(() => GameManager.Singleton.NodeRegistry.Count() == CurrentLevelData.Layers.Sum(x => x.nodeCount));

            horizontalVerticalAdjacencyList = new AdjacencyList();
            float[] angleOffsets = new float[5]{0.0f, -180.0f, 180.0f, 90.0f, -90.0f};

            //iterate over current level's adjacency list
            for (int i = 0; i < CurrentLevelData.AdjacencyListData.Count; i++) {
                //get the current node
                int currentNodeID = i;

                Vector3 currendNodePosition = Vector3.zero;
                
                if (GameManager.Singleton.NodeRegistry.Contains(currentNodeID)) {
                    currendNodePosition = GameManager.Singleton.NodeRegistry.TryGetValue(currentNodeID).gameObject.transform.position;
                }

                /*if (currendNodePosition == Vector3.zero) {
                    Debug.LogError("non existent node ID issue?");
                }*/

                //iterate over list of adjacent nodes
                for (int j = 0; j < CurrentLevelData.AdjacencyListData[currentNodeID].Count; j++) {
                    int adjacentNodeID = CurrentLevelData.AdjacencyListData[currentNodeID][j];

                    if (currentNodeID ==  adjacentNodeID) {
                        continue;
                    }

                    Vector3 adjacentNodePosition = GameManager.Singleton.NodeRegistry.TryGetValue(adjacentNodeID).gameObject.transform.position;
                    Vector3 nodeDir = (currendNodePosition - adjacentNodePosition).normalized;
                    Vector3 perpVec = Vector3.Cross(nodeDir, Vector3.forward);
                    float angle = Mathf.Atan2(perpVec.y, perpVec.x) * Mathf.Rad2Deg;

                    if (angleOffsets.Contains(angle)) {
                        horizontalVerticalAdjacencyList.Add(currentNodeID, adjacentNodeID);
                        horizontalVerticalAdjacencyList.Add(adjacentNodeID, currentNodeID);
                    }
                }
            }

            //horizontalVerticalAdjacencyList.Log();
        }

        protected void CreateFullAdjacencyList()
        {
            GameManager.Singleton.FullAdjacencyList = new AdjacencyList();

            for (int i = 0; i < CurrentLevelData.AdjacencyListData.Count; i++) {
                int currentNodeID = i;

                for (int j = 0; j < CurrentLevelData.AdjacencyListData[i].Count; j++) {
                    int adjacentNodeID = CurrentLevelData.AdjacencyListData[currentNodeID][j];

                    if (currentNodeID ==  adjacentNodeID) {
                        continue;
                    }

                    GameManager.Singleton.FullAdjacencyList.Add(currentNodeID, adjacentNodeID);
                    GameManager.Singleton.FullAdjacencyList.Add(adjacentNodeID, currentNodeID);
                }
            }
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