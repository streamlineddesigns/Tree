using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StudioByStorm.Optimizations;
using StudioByStorm.FX.Boids;

namespace StudioByStorm.FX {

    public class EdgeLight : MonoBehaviour
    {
        public ParticleSystem[] particleSystems;
        
        public void SetColor(Color color)
        {
            for (int i = 0; i < particleSystems.Length; i++) {
                particleSystems[i].startColor = color;
            }
        }
    }

}