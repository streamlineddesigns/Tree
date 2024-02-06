using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StudioByStorm.FX {

    public class PlayerLoseFX : MonoBehaviour
    {
        [SerializeField] private ParticleSystem gameOverExplosionFX;
        [SerializeField] private ParticleSystem gameOverGlowingLightFX;

        public void Play()
        {
            StartCoroutine(PlayFX());
        }

        IEnumerator PlayFX()
        {
            gameOverExplosionFX.gameObject.SetActive(true);
            yield return new WaitForSeconds(0.5f);
            gameOverGlowingLightFX.gameObject.SetActive(true);
        }
    }

}