using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StudioByStorm.Optimizations;
using StudioByStorm.FX.Boids;
using StudioByStorm.UI.Controllers;
using StudioByStorm.EventPublishers;
using StudioByStorm.Tutorials.Animations;

namespace StudioByStorm.FX {

    public class FXManager : MonoBehaviour
    {
        public Transform FXParent;
        public SpatialHash<HashData> SpatialHash;
        //Sea Dust FX
        public GameObject SeaDust;
        //Boids FX
        public GameObject BoidPrefab;
        public BoidConfig BoidConfig;
        public Transform[] BoidTargets;
        [HideInInspector]
        public Pool BoidPool;
        //Fireworks FX
        public GameObject FireWorkPrefab;
        [HideInInspector]
        public Pool FireworksPool;
        //Node FX
        public GameObject NodeWindInPrefab;
        [HideInInspector]
        public Pool NodeWindInPool;
        public GameObject NodeRippleInPrefab;
        [HideInInspector]
        public Pool NodeRippleInPool;
        //EdgeFX
        public GameObject EdgeLightPrefab;
        [HideInInspector]
        public Pool EdgeLightPool;
        //ConnectionIndicatorFX
        public GameObject ConnectionIndicatorPrefab;
        [HideInInspector]
        public Pool ConnectionIndicatorPool;
        //PlayerLoseFX
        public GameObject PlayerLoseFXPrefab;
        public GameObject PlayerLoseFX;
        //Tutorial Animations
        public GameObject fingerSlingShotAnimationPrefab;
        public FingerSlingShotAnimation fingerSlingShotAnimation;
        public Transform tutorialParent;
        
        protected int connectionIndicatorPoolSize = 5;
        protected int edgeLightPoolSize = 3;
        protected int nodeRippleInPoolSize = 3;
        protected int nodeWindInPoolSize = 3;
        protected int fireworkPoolSize = 5;
        protected int boidPoolSize = 20;
        protected int colorCount = 4;
        protected int boidPerColor = 5;

        int playerEdgeChangeID = -1;

        void OnEnable()
        {
            GameEventPublisher.OnPlayerEdgeChange += OnPlayerEdgeChange;
        }

        void OnDisable()
        {
            GameEventPublisher.OnPlayerEdgeChange -= OnPlayerEdgeChange;
        }

        protected void OnPlayerEdgeChange(int ParentNodeID)
        {
            //send -1 if deactivating an edge
            if (ParentNodeID == -1) {
                ConnectionIndicatorPool.DeactivateAll();
                return;
            }

            if (playerEdgeChangeID == ParentNodeID) {
                return;
            }

            playerEdgeChangeID = ParentNodeID;

            ActionController actionController = GameManager.Singleton.ControllerRegistry.TryGetValue(ViewName.ActionView) as ActionController;
            if (actionController == null || actionController.ActionModel.CurrentEdge == null) {
                return;
            }

            StartCoroutine(DelayedOnPlayerEdgeChange(ParentNodeID, actionController));
        }

        IEnumerator DelayedOnPlayerEdgeChange(int ParentNodeID, ActionController actionController)
        {
            ConnectionIndicatorPool.DeactivateAll();

            yield return null;

            List<int> connectedNodes = GameManager.Singleton.FullAdjacencyList.Get(ParentNodeID);
            
            for (int i = 0; i < connectedNodes.Count; i++) {
                int nid = connectedNodes[i];
                Node nearbyNode = GameManager.Singleton.NodeRegistry.TryGetValue(nid);

                if  (
                        //the other parent
                        (nearbyNode.currentEdge.childID == -1 && nearbyNode.NodeColor == actionController.ActionModel.CurrentEdge.EdgeColor) || 
                        //disjoint
                        (nearbyNode.NodeType == NodeType.Disjoint)
                    ){
                    
                    //show a connection indicator at the same position of the cell if it can be connected to
                    GameObject connectionIndicator = ConnectionIndicatorPool.Get();
                    connectionIndicator.transform.position = nearbyNode.gameObject.transform.position;
                    connectionIndicator.SetActive(true);
                }
            }
        }

        void Awake()
        {
            int cellsize = 15;
            SpatialHash = new SpatialHash<HashData>(cellsize);
            
            BoidPool = ScriptableObject.CreateInstance<Pool>();
            BoidPool.DependencyInjection(BoidPrefab, FXParent, boidPoolSize);

            FireworksPool = ScriptableObject.CreateInstance<Pool>();
            FireworksPool.DependencyInjection(FireWorkPrefab, FXParent, fireworkPoolSize);

            NodeWindInPool = ScriptableObject.CreateInstance<Pool>();
            NodeWindInPool.DependencyInjection(NodeWindInPrefab, FXParent, nodeWindInPoolSize);

            NodeRippleInPool = ScriptableObject.CreateInstance<Pool>();
            NodeRippleInPool.DependencyInjection(NodeRippleInPrefab, FXParent, nodeRippleInPoolSize);

            EdgeLightPool = ScriptableObject.CreateInstance<Pool>();
            EdgeLightPool.DependencyInjection(EdgeLightPrefab, FXParent, edgeLightPoolSize);

            ConnectionIndicatorPool = ScriptableObject.CreateInstance<Pool>();
            ConnectionIndicatorPool.DependencyInjection(ConnectionIndicatorPrefab, FXParent, connectionIndicatorPoolSize);

            PlayerLoseFX = Instantiate(PlayerLoseFXPrefab, FXParent);

            fingerSlingShotAnimation = Instantiate(fingerSlingShotAnimationPrefab, tutorialParent).GetComponent<FingerSlingShotAnimation>();
        }

        void Start()
        {
            GenerateBoids();
        }
        
        protected void GenerateBoids()
        {
            for (int i = 0; i < GameManager.Singleton.ColorModel.colorsInUse.Length; i++) {
                for (int j = 0; j < boidPerColor; j++) {
                    NodeColor currentColor = GameManager.Singleton.ColorModel.colorsInUse[i];
                    int nodeColorIndex = (int) currentColor;
                    Color color = GameManager.Singleton.ColorModel.lightColor[nodeColorIndex];
                    Boid boid = BoidPool.Get().GetComponent<Boid>();
                    boid.SetColor(color);
                    boid.gameObject.SetActive(true);
                }
            }
        }

        IEnumerator DelayedLaunchFireWork()
        {
            //Vector3 targetPosition = GameManager.Singleton.player.transform.position;
            GameController GameController = GameManager.Singleton.ControllerRegistry.TryGetValue(ViewName.GameView) as GameController;
            Vector3 targetPosition = GameController.Centroid.transform.position;

            int fireworksToLaunch = 5;
            
            float offsetX = targetPosition.x;
            float offsetY = targetPosition.y;
            float AdditionalXOffset = 15.0f;
            float AdditionalYOffset = 5.0f;
            float timeBetweenLaunches = 0.25f;

            for (int i = 0; i < fireworksToLaunch; i++) {
                GameObject firework = FireworksPool.Get();
                Vector3 currentTargetPosition = targetPosition;
                
                float randomizedXOffset = (UnityEngine.Random.Range(0.0f, 10.0f) > 5.0f) ? AdditionalXOffset : -AdditionalXOffset;
                randomizedXOffset += UnityEngine.Random.Range(-5.0f, 5.0f);
                currentTargetPosition.x = offsetX + randomizedXOffset;

                float randomizedYOffset = (UnityEngine.Random.Range(0.0f, 10.0f) > 5.0f) ? AdditionalYOffset : -AdditionalYOffset;
                randomizedYOffset += UnityEngine.Random.Range(-5.0f, 5.0f);
                currentTargetPosition.y = offsetY + randomizedYOffset;

                firework.transform.position = currentTargetPosition;
                firework.SetActive(true);
                yield return new WaitForSeconds(timeBetweenLaunches);
            }
        }

        public void LaunchFireWork()
        {
            StartCoroutine(DelayedLaunchFireWork());
        }

        
    }

}