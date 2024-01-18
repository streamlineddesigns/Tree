using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace StudioByStorm.UI {

    public class FadeView : View
    {
        private Image backgroundImage;

        protected void Awake()
        {
            backgroundImage = gameObject.GetComponent<Image>();
        }

        protected void Update()
        {
            if (GameManager.Singleton != null && GameManager.Singleton.ViewRegistry != null) {
                View cutSceneView = GameManager.Singleton.ViewRegistry.TryGetValue(ViewName.CutSceneView);
                if (cutSceneView.gameObject.activeSelf) {
                    FadeCallback();
                }
            }
        }

        protected void OnEnable()
        {
            backgroundImage.DOFade(0.0f, 1.5f).SetEase(Ease.InQuad).OnComplete(FadeCallback);
        }

        protected void FadeCallback()
        {
            gameObject.SetActive(false);
            backgroundImage.DOFade(1.0f, 0.0f);
        }
    }

}