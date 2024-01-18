using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StudioByStorm.Optimizations;
using StudioByStorm.FX.Boids;

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
        
        protected int edgeLightPoolSize = 3;
        protected int nodeRippleInPoolSize = 3;
        protected int nodeWindInPoolSize = 3;
        protected int fireworkPoolSize = 5;
        protected int boidPoolSize = 20;
        protected int colorCount = 4;
        protected int boidPerColor = 8;

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
        }

        void Start()
        {
            GenerateBoids();
        }
        
        protected void GenerateBoids()
        {
            for (int i = 0; i < colorCount; i++) {
                for (int j = 0; j < boidPerColor; j++) {
                    Color color = GameManager.Singleton.ColorModel.lightColor[i];
                    Boid boid = BoidPool.Get().GetComponent<Boid>();
                    boid.SetColor(color);
                    boid.gameObject.SetActive(true);
                }
            }
        }

        IEnumerator DelayedLaunchFireWork()
        {
            Vector3 targetPosition = GameManager.Singleton.player.transform.position;
            int fireworksToLaunch = 5;
            
            float offsetX = targetPosition.x;
            float offsetY = targetPosition.y;
            float AdditionalXOffset = 12.0f;
            float AdditionalYOffset = 4.0f;
            float timeBetweenLaunches = 0.25f;

            for (int i = 0; i < fireworksToLaunch; i++) {
                GameObject firework = FireworksPool.Get();
                Vector3 currentTargetPosition = targetPosition;
                
                float randomizedXOffset = (UnityEngine.Random.Range(0.0f, 10.0f) > 5.0f) ? AdditionalXOffset : -AdditionalXOffset;
                randomizedXOffset += UnityEngine.Random.Range(-2.0f, 2.0f);
                currentTargetPosition.x = offsetX + randomizedXOffset;

                float randomizedYOffset = (UnityEngine.Random.Range(0.0f, 10.0f) > 5.0f) ? AdditionalYOffset : -AdditionalYOffset;
                randomizedYOffset += UnityEngine.Random.Range(-2.0f, 2.0f);
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