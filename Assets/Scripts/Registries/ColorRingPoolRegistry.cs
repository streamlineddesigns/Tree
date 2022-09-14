using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StudioByStorm.Optimizations;

namespace StudioByStorm.Registries {

    public class ColorRingPoolRegistry : Registry<NodeColor, Pool>
    {        
        public Transform parent;
        public int numberToSpawn;

        void Start()
        {
            for (int i = 0; i < GameManager.Singleton.ColorModel.coloredRings.Length; i++) {
                if (GameManager.Singleton.ColorModel.coloredRings[i] != null) {
                    Pool Pool = ScriptableObject.CreateInstance<Pool>();
                    Pool.DependencyInjection(GameManager.Singleton.ColorModel.coloredRings[i], parent, numberToSpawn);
                    Add((NodeColor) i, Pool);
                }
            }
            
        }
    }

}