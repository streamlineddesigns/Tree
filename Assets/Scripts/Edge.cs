using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
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
        public SpriteRenderer[] LinkConnectorSpriteRenderers;
        public FabrikSolver2D FabrikSolver2D;
        public IKManager2D IKManager2D;
        public int activeLinkIndex;
        public GameObject emptyTarget;
        public SpriteRenderer hookSpriteRenderer;
        public LineRenderer lineRendererFX;
        public bool isOutOfBounds;

        void Start()
        {
            GameManager.Singleton.EdgeRegistry.Add(parentID, this);
        }

        void OnEnable()
        {
            EdgeColor = (parentNode != null) ? parentNode.NodeColor : GameManager.Singleton.NodeRegistry.TryGetValue(parentID).NodeColor;
            int colorIndex = (int) EdgeColor;

            for (int i = 0; i < LinkSpriteRenderers.Length; i++) {
                LinkSpriteRenderers[i].sprite = GameManager.Singleton.ColorModel.ColoredGetters[colorIndex];
                LinkSpriteRenderers[i].gameObject.SetActive(true);
                if (i < LinkConnectorSpriteRenderers.Length) LinkConnectorSpriteRenderers[i].gameObject.SetActive(true);
            }

            activeLinkIndex = LinkSpriteRenderers.Length - 1;

            lineRendererFX.enabled = false;

            fabrikOn(true);
        }

        public void DisplayLineRendererFX(float size = 0.1f)
        {
            Vector3 parentPosition = gameObject.transform.position;
            Vector3 childPosition = GameManager.Singleton.NodeRegistry.TryGetValue(childID).gameObject.transform.position;

            lineRendererFX.enabled = true;
            lineRendererFX.SetPosition(0, parentPosition);
            lineRendererFX.SetPosition(1, childPosition);
            lineRendererFX.SetWidth(size, size);
            lineRendererFX.SetColors(GameManager.Singleton.ColorModel.darkColor[(int) EdgeColor], GameManager.Singleton.ColorModel.darkColor[(int) EdgeColor]);
        }

        public void OutOfBoundsIndicator()
        {
            for(int i = 0; i < LinkConnectorSpriteRenderers.Length; i++) {
                LinkConnectorSpriteRenderers[i].material = GameManager.Singleton.ColorModel.unlitMaterial;
            }
            hookSpriteRenderer.material = GameManager.Singleton.ColorModel.unlitMaterial;
        }

        public void InBoundsIndicator()
        {
            for(int i = 0; i < LinkConnectorSpriteRenderers.Length; i++) {
                LinkConnectorSpriteRenderers[i].material = GameManager.Singleton.ColorModel.litMaterial;
            }
            hookSpriteRenderer.material = GameManager.Singleton.ColorModel.litMaterial;
        }

        public IEnumerator CantSetEdgeAnimation()//v1.13
        {
            for(int i = 0; i < LinkConnectorSpriteRenderers.Length; i++) {
                LinkConnectorSpriteRenderers[i].material = GameManager.Singleton.ColorModel.unlitMaterial;
            }
            hookSpriteRenderer.material = GameManager.Singleton.ColorModel.unlitMaterial;

            yield return new WaitForSeconds(0.075f);

            for(int i = 0; i < LinkConnectorSpriteRenderers.Length; i++) {
                LinkConnectorSpriteRenderers[i].material = GameManager.Singleton.ColorModel.litMaterial;
            }
            hookSpriteRenderer.material = GameManager.Singleton.ColorModel.litMaterial;

            yield return new WaitForSeconds(0.075f);

            for(int i = 0; i < LinkConnectorSpriteRenderers.Length; i++) {
                LinkConnectorSpriteRenderers[i].material = GameManager.Singleton.ColorModel.unlitMaterial;
            }
            hookSpriteRenderer.material = GameManager.Singleton.ColorModel.unlitMaterial;

            yield return new WaitForSeconds(0.075f);

            for(int i = 0; i < LinkConnectorSpriteRenderers.Length; i++) {
                LinkConnectorSpriteRenderers[i].material = GameManager.Singleton.ColorModel.litMaterial;
            }
            hookSpriteRenderer.material = GameManager.Singleton.ColorModel.litMaterial;

            yield return new WaitForSeconds(0.075f);

            for(int i = 0; i < LinkConnectorSpriteRenderers.Length; i++) {
                LinkConnectorSpriteRenderers[i].material = GameManager.Singleton.ColorModel.unlitMaterial;
            }
            hookSpriteRenderer.material = GameManager.Singleton.ColorModel.unlitMaterial;

            yield return new WaitForSeconds(0.075f);

            for(int i = 0; i < LinkConnectorSpriteRenderers.Length; i++) {
                LinkConnectorSpriteRenderers[i].material = GameManager.Singleton.ColorModel.litMaterial;
            }
            hookSpriteRenderer.material = GameManager.Singleton.ColorModel.litMaterial;

            yield return new WaitForSeconds(0.075f);

            if (isOutOfBounds) OutOfBoundsIndicator();
        }

        public void turnFabrikOff()
        {
            StartCoroutine(DelayedFabrikShutDown());
        }

        protected IEnumerator DelayedFabrikShutDown()
        {
            //get child node
            Node childNode = GameManager.Singleton.NodeRegistry.TryGetValue(childID);
            //calculate direction between child and parent node
            Vector2 direction = (childNode.gameObject.transform.position - parentNode.gameObject.transform.position).normalized;
            //get child node position (ie the target) and scale vector in that direction
            Vector2 scaledTargetPosition = (Vector2)childNode.gameObject.transform.position + (direction * 14.0f);
            emptyTarget.transform.position = childNode.gameObject.transform.position;

            //get distance too so we know how many links to disable visually
            float distance = Vector2.Distance(childNode.gameObject.transform.position, parentNode.gameObject.transform.position);
            //Debug.Log(distance);

            //tell chain to target the empty target
            FabrikSolver2D.GetChain(FabrikSolver2D.chainCount).target = emptyTarget.transform;
            //then move the empty target to that scaled target position for a smoother looking animation
            emptyTarget.transform.DOMove(scaledTargetPosition, 0.3f).SetEase(Ease.InQuad);

            //while that's happening, disable some of the end links, so they don't extend passed child node
            if (distance >= 9.0f) {
                StartCoroutine(DisableLinks(1));
            } else if (distance >= 7.5f) {    
                StartCoroutine(DisableLinks(2));
            } else {
                StartCoroutine(DisableLinks(3));
            }

            yield return new WaitForSeconds(0.325f);

            //$$jiggle the edge
            Vector3 edgeTarget = Vector3.zero;
            edgeTarget.y += 0.2f;
            //gameObject.transform.DOPunchPosition(edgeTarget, 0.2f, 0, 0.2f, false);

            //need to get the nearest nodes id
            //Node nearestNode = GameManager.Singleton.nearbyNode.GetData<Node>();
            //set new target
            //FabrikSolver2D.GetChain(FabrikSolver2D.chainCount).target = nearestNode.gameObject.transform;
            //yield return new WaitForSeconds(1.0f);

            fabrikOn(false);
        }

        IEnumerator DisableLinks(int count)
        {
            int targetIndex = (LinkSpriteRenderers.Length - 1) - count;

            for (int i = LinkSpriteRenderers.Length - 1; i > targetIndex; i--) {
                LinkSpriteRenderers[i].gameObject.SetActive(false);
                if (i < LinkConnectorSpriteRenderers.Length) LinkConnectorSpriteRenderers[i].gameObject.SetActive(false);
                activeLinkIndex = i - 1;
                yield return new WaitForSeconds(0.08335f);//approx 5 frames
            }

            activeLinkIndex = targetIndex;
        }

        protected void fabrikOn(bool isOn)
        {
            FabrikSolver2D.enabled = isOn;
            IKManager2D.enabled = isOn;
        }

        
    }

}