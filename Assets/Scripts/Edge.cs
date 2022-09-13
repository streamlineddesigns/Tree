using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.IK;

namespace StudioByStorm {

    public class Edge : MonoBehaviour
    {
        public bool connected = false;
        public int parentID;
        public Node parentNode;
        public int childID;
        public NodeColor EdgeColor;
        public SpriteRenderer[] LinkSpriteRenderers;
        public FabrikSolver2D FabrikSolver2D;
        public IKManager2D IKManager2D;

        void Start()
        {
            GameManager.Singleton.EdgeRegistry.Add(parentID, this);
        }

        void OnEnable()
        {
            EdgeColor = GameManager.Singleton.NodeRegistry.TryGetValue(parentID).NodeColor;
            int colorIndex = (int) EdgeColor;

            for (int i = 0; i < LinkSpriteRenderers.Length; i++) {
                LinkSpriteRenderers[i].sprite = GameManager.Singleton.ColorModel.ColoredGetters[colorIndex];
            }

            fabrikOn(true);
        }

        public void turnFabrikOff()
        {
            StartCoroutine(DelayedFabrikShutDown());
        }

        protected IEnumerator DelayedFabrikShutDown()
        {
            yield return new WaitForSeconds(1.0f);
            fabrikOn(false);
        }

        protected void fabrikOn(bool isOn)
        {
            FabrikSolver2D.enabled = isOn;
            IKManager2D.enabled = isOn;
        }

        
    }

}