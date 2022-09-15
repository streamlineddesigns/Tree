using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StudioByStorm.Registries;
using StudioByStorm.Optimizations;
using StudioByStorm.ML.Clustering;
using StudioByStorm.Gravity.Player;
using StudioByStorm.Graph;
using StudioByStorm.UI;
using StudioByStorm.Config;
using StudioByStorm.GestureRecognition;

namespace StudioByStorm {


    public class GameManager : MonoBehaviour
    {
        public ColorRingPoolRegistry ColorRingPoolRegistry;
        public Pool NodePool;
        public MobileInput MobileInput;
        public LevelManager LevelManager;
        public LevelConfig LevelConfig;
        public SpatialHashManager SpatialHashManager;
        public UIController UIController;
        public AdjacencyList AdjacencyList;
        public ColorModel ColorModel;
        public static GameManager Singleton;
        public ControllerRegistry ControllerRegistry;
        public ColorEdgeRegistry ColorEdgeRegistry;
        public ColorNodeRegistry ColorNodeRegistry;
        public ViewRegistry ViewRegistry;
        public NodeRegistry NodeRegistry;
        public EdgeRegistry EdgeRegistry;
        public GameObject Edge;
        public GameObject Node;
        public HashData PlayerHashData;
        public GameObject player;
        public PlayerController PlayerController;
        public HashData nearbyNode;
        
        protected void Awake()
        {
            if (Singleton == null) {
                Singleton = this;
                PlayerHashData = new HashData(player);
                GameManager.Singleton.SpatialHashManager.SetPlayerHashData(PlayerHashData);
                PlayerController = player.GetComponent<PlayerController>();
                AdjacencyList = new AdjacencyList();
                NodePool = ScriptableObject.CreateInstance<Pool>();
                NodePool.DependencyInjection(Node, SpatialHashManager.nodeParent.transform, 25);
            } else {
                Destroy(this);
            }
        }
        
        public void LevelComplete()
        {
            PlayerController.LockMovement(true);
            UIController.ShowView(ViewName.LevelCompleteView);
        }
                
        protected void Update()
        {
            SetNearbyNode();
        }

        protected void SetNearbyNode()
        {
            if (GameManager.Singleton.SpatialHashManager.SpatialHash == null || GameManager.Singleton.SpatialHashManager.Nodes.Count == 0) {
                return;
            }
            Vector2 cellID = GameManager.Singleton.SpatialHashManager.SpatialHash.GetCellIDForObj(PlayerHashData);
            List<HashData> nearbyNodes = GameManager.Singleton.SpatialHashManager.SpatialHash.GetNearby(cellID);
            List<HashData> nearestNeighbor = KNN.GetKNearestNeighbors(PlayerHashData, nearbyNodes, 1);
            nearbyNode = (nearestNeighbor[0] != null) ? nearestNeighbor[0] : null;
        }
    }

}