using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StudioByStorm.Registries;
using DG.Tweening;

namespace StudioByStorm {

    public class Node : MonoBehaviour
    {
        public int ID;
        public int NumOfConnections;
        public NodeType NodeType;
        public NodeColor NodeColor;
        public GameObject ColorSurface;
        public GameObject DarkSurface;
        public SpriteRenderer LightColored;
        public SpriteRenderer DarkColored;
        public SpriteRenderer[] Hairs;
        public SpriteRenderer InnerGraphic;
        public Transform[] randomizedTransforms;
        public SpriteRenderer[] randomizedSpriteRenderer;
        public Sprite[] randomSprites;
        public GameObject coloredRing;
        public Edge currentEdge;

        protected NodeType OriginalNodeType;
        protected NodeColor OriginalNodeColor;

        void Start()
        {
            //GameManager.Singleton.NodeRegistry.Add(ID, this);
            

            currentEdge = Instantiate(GameManager.Singleton.LevelManager.Edge, gameObject.transform.position, Quaternion.identity, GameManager.Singleton.EdgeRegistry.EdgeParent.transform).GetComponent<Edge>();
            
            UpdateEdge();
            currentEdge.gameObject.transform.name = "Edge-ID" + ID;
            currentEdge.parentNode = this;
        }

        void UpdateEdge()
        {
            currentEdge.childID = -1;
            currentEdge.parentID = ID;
            currentEdge.EdgeColor = NodeColor;
            currentEdge.FabrikSolver2D.GetChain(currentEdge.FabrikSolver2D.chainCount).target = GameManager.Singleton.player.transform;
            currentEdge.transform.position = gameObject.transform.position;
            GameManager.Singleton.EdgeRegistry.Add(ID, currentEdge);
            GameManager.Singleton.NodeRegistry.Add(ID, this);
            GameManager.Singleton.AdjacencyList.Add(ID);
        }

        void OnEnable()
        {
            StartCoroutine(DelayedEnable());
        }

        void OnDisable() 
        {
            if (coloredRing != null && NodeType != NodeType.Parent) {
                coloredRing.SetActive(false);
            }
            if (currentEdge != null) {
                currentEdge.gameObject.SetActive(false);
            }
            
            NodeType = OriginalNodeType;
            NodeColor = OriginalNodeColor;
            NumOfConnections = 0;

            GameManager.Singleton.EdgeRegistry.Remove(ID);
            GameManager.Singleton.NodeRegistry.Remove(ID);
            GameManager.Singleton.AdjacencyList.Remove(ID);
        }

        IEnumerator DelayedEnable()
        {
            yield return 0;
            UpdateEdge();

            OriginalNodeType = NodeType;
            OriginalNodeColor = NodeColor;

            //randomize rotations of specified transforms
            for (int i = 0; i < randomizedTransforms.Length; i++) {

                Vector3 rotation = randomizedTransforms[i].rotation.eulerAngles;
                rotation.z = UnityEngine.Random.Range(0.0f, 360.0f);
                randomizedTransforms[i].rotation = Quaternion.Euler(rotation);
            }

            Vector3 sharedSurfaceVector = DarkSurface.transform.rotation.eulerAngles;
            sharedSurfaceVector.z = UnityEngine.Random.Range(0.0f, 360.0f);
            Quaternion sharedSurfaceRotation = Quaternion.Euler(sharedSurfaceVector);
            DarkSurface.transform.rotation = sharedSurfaceRotation;
            ColorSurface.transform.rotation = sharedSurfaceRotation;

            //randomize sprites of specifies sprite renderers
            for (int j = 0; j < randomizedSpriteRenderer.Length; j++) {
                randomizedSpriteRenderer[j].sprite = randomSprites[UnityEngine.Random.Range(0, randomSprites.Length)];
            }

            ResetToFactorySettings();

            InnerGraphic.sprite = GameManager.Singleton.ColorModel.ColoredInners[(int)NodeColor];
        }

        public void ResetToFactorySettings()
        {
            //Either show gray scale or color depending on the NodeColor value
            if (OriginalNodeColor == NodeColor.GrayScale) {
                DisplayGrayScale();
            } else {
                Color d = GameManager.Singleton.ColorModel.darkColor[(int)NodeColor];
                Color l = GameManager.Singleton.ColorModel.lightColor[(int)NodeColor];
                DarkColored.color = d;
                LightColored.color = l;
                DisplayColor();
                DisplayHairColor();
            }



            //add a color ring for the parent nodes
            if (OriginalNodeType == NodeType.Parent && coloredRing == null && GameManager.Singleton.ColorModel.coloredRings[(int) NodeColor] != null) {
                AddColorRing();
                //ActivateHairs();
            } else if (coloredRing != null && OriginalNodeType == NodeType.Parent) {
                coloredRing.SetActive(true);
            }
        }

        public void DisplayHairColor()
        {
            for (int i = 0; i < Hairs.Length; i++) {
                Color withNoAlpha = GameManager.Singleton.ColorModel.lightColor[(int)NodeColor];
                withNoAlpha.a = 0.0f;
                Hairs[i].color = withNoAlpha;
            }
        }

        public void ActivateHairs()
        {
            for (int i = 0; i < Hairs.Length; i++) {
                Hairs[i].transform.parent.gameObject.SetActive(true);
            }
        }

        public void AddColorRing(bool ScaleInRing = false)
        {
            coloredRing = GameManager.Singleton.ColorRingPoolRegistry.TryGetValue(NodeColor).Get();//Instantiate(GameManager.Singleton.ColorModel.coloredRings[(int) NodeColor], gameObject.transform);
            coloredRing.transform.SetParent(gameObject.transform);
            coloredRing.transform.position = gameObject.transform.position;
            coloredRing.SetActive(true);
            if (ScaleInRing) {
                
                GameObject firstChild = coloredRing.transform.GetChild(0).gameObject;;
                Vector3 targetScale = firstChild.transform.localScale;
                for (int i = 0; i < coloredRing.transform.childCount; i++) {
                    coloredRing.transform.GetChild(i).localScale = Vector3.zero;
                    coloredRing.transform.GetChild(i).DOScale(targetScale, 3f).SetEase(Ease.InOutCubic);
                }
            }
        }

        public void DisplayColor() {
            InnerGraphic.sprite = GameManager.Singleton.ColorModel.ColoredInners[(int)NodeColor];
            //ColorSurface.SetActive(true);
            //DarkSurface.SetActive(false);
            //ColorSurface.SetActive(true);
            LightColored.DOFade(1, 1.0f).SetEase(Ease.InSine);
            DarkColored.DOFade(1, 1.0f).SetEase(Ease.InSine);
            for (int i = 0; i < Hairs.Length; i++) {
                Hairs[i].DOFade(1, 1.0f).SetEase(Ease.InSine);
            }
            DarkSurface.GetComponent<SpriteRenderer>().DOFade(0, 1.0f).SetEase(Ease.InSine);
        }

        public void DisplayGrayScale() {
            //ColorSurface.SetActive(false);
            //DarkSurface.SetActive(true);
            //DarkSurface.SetActive(true);
            LightColored.DOFade(0, 1.0f).SetEase(Ease.InSine);
            DarkColored.DOFade(0, 1.0f).SetEase(Ease.InSine);
            for (int i = 0; i < Hairs.Length; i++) {
                Hairs[i].DOFade(0, 1.0f).SetEase(Ease.InSine);
            }
            DarkSurface.GetComponent<SpriteRenderer>().DOFade(1, 1.0f).SetEase(Ease.InSine);
        }
    }

}