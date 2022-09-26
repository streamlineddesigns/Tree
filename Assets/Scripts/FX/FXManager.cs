using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using StudioByStorm.Optimizations;
using StudioByStorm.FX.Boids;

namespace StudioByStorm.FX {

    public class FXManager : MonoBehaviour
    {
        public GameObject SeaDust;
        public GameObject BoidPrefab;
        public Transform BoidParent;
        public BoidConfig BoidConfig;
        public Transform[] BoidTargets;
        public SpatialHash<HashData> SpatialHash;
        public Pool BoidPool;
        protected int poolSize = 20;
        protected int colorCount = 4;
        protected int boidPerColor = 4;

        void Awake()
        {
            int cellsize = 15;
            SpatialHash = new SpatialHash<HashData>(cellsize);
            
            BoidPool = ScriptableObject.CreateInstance<Pool>();
            BoidPool.DependencyInjection(BoidPrefab, BoidParent, poolSize);
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
    }

}