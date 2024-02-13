using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StudioByStorm.FX {

    public class Firework : MonoBehaviour
    {
        public ParticleSystem ps;
        private bool isPlaying;

        protected void OnEnable()
        {
            if (ps.emission.enabled && !isPlaying)  {
                isPlaying = true;
                AudioManager.Singleton.Play(SoundType.Fireworks);
            }
        }

    }

}