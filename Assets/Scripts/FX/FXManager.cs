using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using StudioByStorm.Optimizations;
using StudioByStorm.FX.Boids;
using StudioByStorm.UI;
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
        //Wrong Obstacle Hit FX
        public GameObject WrongObstacleHitFX;
        [HideInInspector]
        public Pool WrongObstacleHitPool;
        //Correct Obstacle Hit FX
        public GameObject CorrectObstacleHitFX;
        [HideInInspector]
        public Pool CorrectObstacleHitPool;
        //Player Trail FX
        public GameObject PlayerTrailFX;
        [HideInInspector]
        public Pool PlayerTrailPool;
        //Jump FX
        public GameObject PlayerJumpFX;
        [HideInInspector]
        public Pool PlayerJumpPool;
        //Tutorial Animations
        public GameObject fingerSlingShotAnimationPrefab;
        public FingerSlingShotAnimation fingerSlingShotAnimation;
        public Transform tutorialParent;
        //node connect
        [HideInInspector]
        public Dictionary<NodeColor, Pool> nodeConnectPools = new Dictionary<NodeColor, Pool>();
        public GameObject BlueNodeConnectPrefab;
        public GameObject GreenNodeConnectPrefab;
        public GameObject PurpleNodeConnectPrefab;
        public GameObject OrangeNodeConnectPrefab;
        public GameObject WhitNodeConnectPrefab;
        
        protected int connectionIndicatorPoolSize = 5;
        protected int edgeLightPoolSize = 3;
        protected int nodeRippleInPoolSize = 3;
        protected int nodeWindInPoolSize = 3;
        protected int fireworkPoolSize = 5;
        protected int boidPoolSize = 12;
        protected int colorCount = 4;
        protected int boidPerColor = 3;
        protected int nodeConnectPoolSizes = 2;
        protected int wrongObstacleHitPoolSize = 3;
        protected int correctObstacleHitPoolSize = 3;
        protected int playerTrailPoolSize = 3;
        protected int playerJumpPoolSize = 2;

        int playerEdgeChangeID = -1;

        private int rewardAnimationCount = 0;
        private List<Edge> edgesWithAnimationsOn = new List<Edge>();

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
        
            Pool BlueNodeConnectPool = ScriptableObject.CreateInstance<Pool>();
            BlueNodeConnectPool.DependencyInjection(BlueNodeConnectPrefab, FXParent, nodeConnectPoolSizes);
            nodeConnectPools.Add(NodeColor.Blue, BlueNodeConnectPool);

            Pool GreenNodeConnectPool = ScriptableObject.CreateInstance<Pool>();
            GreenNodeConnectPool.DependencyInjection(GreenNodeConnectPrefab, FXParent, nodeConnectPoolSizes);
            nodeConnectPools.Add(NodeColor.Green, GreenNodeConnectPool);

            Pool PurpleNodeConnectPool = ScriptableObject.CreateInstance<Pool>();
            PurpleNodeConnectPool.DependencyInjection(PurpleNodeConnectPrefab, FXParent, nodeConnectPoolSizes);
            nodeConnectPools.Add(NodeColor.Purple, PurpleNodeConnectPool);

            /*Pool OrangeNodeConnectPool = ScriptableObject.CreateInstance<Pool>();
            OrangeNodeConnectPool.DependencyInjection(OrangeNodeConnectPrefab, FXParent, nodeConnectPoolSizes);
            nodeConnectPools.Add(NodeColor.Orange, OrangeNodeConnectPool);*/

            Pool WhiteNodeConnectPool = ScriptableObject.CreateInstance<Pool>();
            WhiteNodeConnectPool.DependencyInjection(WhitNodeConnectPrefab, FXParent, nodeConnectPoolSizes);
            nodeConnectPools.Add(NodeColor.White, WhiteNodeConnectPool);

            WrongObstacleHitPool = ScriptableObject.CreateInstance<Pool>();
            WrongObstacleHitPool.DependencyInjection(WrongObstacleHitFX, FXParent, wrongObstacleHitPoolSize);

            CorrectObstacleHitPool = ScriptableObject.CreateInstance<Pool>();
            CorrectObstacleHitPool.DependencyInjection(CorrectObstacleHitFX, FXParent, correctObstacleHitPoolSize);

            PlayerTrailPool = ScriptableObject.CreateInstance<Pool>();
            PlayerTrailPool.DependencyInjection(PlayerTrailFX, FXParent, playerTrailPoolSize);

            PlayerJumpPool = ScriptableObject.CreateInstance<Pool>();
            PlayerJumpPool.DependencyInjection(PlayerJumpFX, FXParent, playerJumpPoolSize);
        }

        void Start()
        {
            //GenerateBoids();
        }

        public IEnumerator nodesLookAtPlayerAnimation(List<Node> nodes)
        {
            while(true) {
                for (int i = 0; i < nodes.Count; i++) {
                    Vector3 directionToPlayer = GameManager.Singleton.player.transform.position - nodes[i].transform.position;
                    Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, directionToPlayer);
                    nodes[i].InnerGraphic.transform.rotation = targetRotation;
                }

                yield return new WaitForSeconds(0.0333f);
            }
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

        public IEnumerator LevelCompleteRewardAnimation()
        {
            //slow-mo
            //StartCoroutine(SlowMoEffect());
            //link loop
            StartCoroutine(LinkLoopAnimation());
            //zoom out
            GameController GameController = GameManager.Singleton.ControllerRegistry.TryGetValue(ViewName.GameView) as GameController;
            GameController.ZoomButtonClick(2.0f);
            //close the zoom view
            GameManager.Singleton.UIController.Close(ViewName.ZoomView);
            yield return new WaitForSeconds(2.0f);
            //show cells used view
            GameManager.Singleton.UIController.ShowView(ViewName.CellsUsedView);

            List<NodeColor> connectedColors = GameManager.Singleton.LevelManager.parentColorsConnected.Keys.ToList();

            for (int i = 0; i < connectedColors.Count; i++) {
                NodeColor currentColor = connectedColors[i];
                Node parentColorNode = GameManager.Singleton.ColorNodeRegistry.TryGetValue(currentColor).Where(x => x.NodeType == NodeType.Parent && x.currentEdge.gameObject.activeSelf).First();
                StartCoroutine(HighLightCell(parentColorNode));
            }

            yield return new WaitUntil(() => rewardAnimationCount >= connectedColors.Count);

            //show level complete view
            LevelCompleteController levelCompleteController = GameManager.Singleton.ControllerRegistry.TryGetValue(ViewName.LevelCompleteView) as LevelCompleteController;
            levelCompleteController.Show();
        }

        IEnumerator HighLightCell(Node node)
        {
            float originalScale = node.gameObject.transform.localScale.x;
            float targetScale = originalScale * 1.5f;

            //rotate node
            Vector3 StartNodeTargetRotation = node.gameObject.transform.localEulerAngles;
            StartNodeTargetRotation.z -= 180.0f;
            node.gameObject.transform.DORotate(StartNodeTargetRotation, 0.5f, RotateMode.LocalAxisAdd);
            
            //scale up
            node.gameObject.transform.DOScale(targetScale, 0.25f).OnComplete(() => {
                //increment cells used text
                CellsUsedView cellsUsedView = GameManager.Singleton.ViewRegistry.TryGetValue(ViewName.CellsUsedView) as CellsUsedView;
                cellsUsedView.incrementUsedCells(1);
                //scale back down
                node.gameObject.transform.DOScale(originalScale, 0.25f);
            });

            //show edgeLightFX Animation
            AudioManager.Singleton.Play(SoundType.EnergyTravel);
            GameObject edgeLightFX = GameManager.Singleton.FXManager.EdgeLightPool.Get();
            edgeLightFX.GetComponent<EdgeLight>().SetColor(GameManager.Singleton.ColorModel.lightColor[(int)node.NodeColor]);
            edgeLightFX.transform.position = node.gameObject.transform.position;
            edgeLightFX.SetActive(true);
            edgeLightFX.transform.DOMove(GameManager.Singleton.player.transform.position, 0.5f).OnComplete(() => {
                edgeLightFX.SetActive(false);
                GameManager.Singleton.PlayerController.BounceAnimation();
            });
            
            if (node.currentEdge.gameObject.activeSelf) {
                //hide lineRendererFX 
                node.currentEdge.lineRendererFX.SetWidth(0.0f, 0.0f);
                //show link animation
                SpriteRenderer[] SpriteRenderers = node.currentEdge.LinkSpriteRenderers.Select(x => x).Take(node.currentEdge.activeLinkIndex).ToArray();
                StartCoroutine(node.currentEdge.DoPlayerPathAnimation(SpriteRenderers, 0.05f));
                //wait for the animation to finish
                yield return new WaitForSeconds(0.05f * SpriteRenderers.Length);
                //add to link loop animations
                edgesWithAnimationsOn.Add(node.currentEdge);
                //show lineRendererFX
                node.currentEdge.lineRendererFX.SetWidth(0.2f, 0.2f);
                //continue for child node
                Node childNode = GameManager.Singleton.NodeRegistry.TryGetValue(node.currentEdge.childID);
                StartCoroutine(HighLightCell(childNode));
                
            } else {
                rewardAnimationCount++;
            }
        }

        IEnumerator LinkLoopAnimation()
        {
            while(true) {
                for (int i = 0; i < edgesWithAnimationsOn.Count; i++) {
                    SpriteRenderer[] SpriteRenderers = edgesWithAnimationsOn[i].LinkSpriteRenderers.Select(x => x).Take(edgesWithAnimationsOn[i].activeLinkIndex).ToArray();
                    StartCoroutine(edgesWithAnimationsOn[i].DoPlayerPathAnimation(SpriteRenderers, 0.05f));
                }
                yield return new WaitForSeconds(0.5f);
            }
        }

        IEnumerator SlowMoEffect()
        {
            float duration = 0.5f;
            float timer = 0.0f;
            bool completed = false;

            Time.timeScale = 0.5f;

            while(! completed) {
                if (timer < duration) {
                    timer += Time.deltaTime;
                    float percent = timer / duration;
                    float easedValue = DOVirtual.EasedValue(0.5f, 1.0f, percent, Ease.InBack);
                    Time.timeScale = easedValue;
                } else {
                    completed = true;
                }
                yield return new WaitForSeconds(0.01667f);
            }

            Time.timeScale = 1.0f;
        }
    }

}