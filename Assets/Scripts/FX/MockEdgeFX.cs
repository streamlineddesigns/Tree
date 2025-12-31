using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StudioByStorm.FX {

    public class MockEdgeFX : MonoBehaviour
    {
        public Edge[] edgesWithAnimationsOn;

        protected void OnEnable()
        {
            StartCoroutine(LinkLoopAnimation());
        }

        IEnumerator LinkLoopAnimation()
        {
            yield return new WaitUntil(() => GameManager.Singleton != null);
            while(true) {
                for (int i = 0; i < edgesWithAnimationsOn.Length; i++) {
                    SpriteRenderer[] SpriteRenderers = edgesWithAnimationsOn[i].LinkSpriteRenderers.Select(x => x).Take(edgesWithAnimationsOn[i].activeLinkIndex).ToArray();
                    StartCoroutine(DoPlayerPathAnimation(SpriteRenderers, 0.05f));
                }
                yield return new WaitForSeconds(0.5f);
            }
        }

        IEnumerator DoPlayerPathAnimation(SpriteRenderer[] linksToAnimate, float duration = 0.1f)
        {
            for (int i = 0; i < linksToAnimate.Length; i++) {
                if (this.gameObject.activeSelf) StartCoroutine(linkAnimation(linksToAnimate[i], duration * 2.0f));
                yield return new WaitForSeconds(duration);
            }
        }

        IEnumerator linkAnimation(SpriteRenderer sr, float duration)
        {
            sr.material = GameManager.Singleton.ColorModel.unlitMaterial;
            yield return new WaitForSeconds(duration);
            sr.material = GameManager.Singleton.ColorModel.litMaterial;
        }
    }

}