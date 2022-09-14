using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StudioByStorm.Registries;

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
        public SpriteRenderer InnerGraphic;
        public Transform[] randomizedTransforms;
        public SpriteRenderer[] randomizedSpriteRenderer;
        public Sprite[] randomSprites;
        public GameObject coloredRing;

        void Start()
        {
            GameManager.Singleton.NodeRegistry.Add(ID, this);
            GameManager.Singleton.AdjacencyList.Add(ID);

            Edge currentEdge = Instantiate(GameManager.Singleton.Edge, gameObject.transform.position, Quaternion.identity, GameManager.Singleton.EdgeRegistry.EdgeParent.transform).GetComponent<Edge>();
            currentEdge.gameObject.transform.name = "Edge-ID" + ID;
            currentEdge.parentNode = this;
            currentEdge.parentID = ID;
            currentEdge.EdgeColor = NodeColor;
            currentEdge.FabrikSolver2D.GetChain(currentEdge.FabrikSolver2D.chainCount).target = GameManager.Singleton.player.transform;
            GameManager.Singleton.EdgeRegistry.Add(currentEdge.parentID, currentEdge);
        }

        void OnEnable()
        {
            StartCoroutine(DelayedEnable());
        }

        void OnDisable() 
        {
            if (coloredRing != null) {
                coloredRing.SetActive(false);
            }
        }

        IEnumerator DelayedEnable()
        {
            yield return 0;
            //randomize rotations of specified transforms
            for (int i = 0; i < randomizedTransforms.Length; i++) {
                Vector3 rotation = randomizedTransforms[i].rotation.eulerAngles;
                rotation.z = UnityEngine.Random.Range(0.0f, 360.0f);
                randomizedTransforms[i].rotation = Quaternion.Euler(rotation);
            }

            //randomize sprites of specifies sprite renderers
            for (int j = 0; j < randomizedSpriteRenderer.Length; j++) {
                randomizedSpriteRenderer[j].sprite = randomSprites[UnityEngine.Random.Range(0, randomSprites.Length)];
            }

            //Either show gray scale or color depending on the NodeColor value
            if (NodeColor == NodeColor.GrayScale) {
                DisplayGrayScale();
            } else {
                DisplayColor();
                DarkColored.color = GameManager.Singleton.ColorModel.darkColor[(int)NodeColor];
                LightColored.color = GameManager.Singleton.ColorModel.lightColor[(int)NodeColor];
            }

            //add a color ring for the parent nodes
            if (NodeType == NodeType.Parent && GameManager.Singleton.ColorModel.coloredRings[(int) NodeColor] != null) {
                coloredRing = GameManager.Singleton.ColorRingPoolRegistry.TryGetValue(NodeColor).Get();//Instantiate(GameManager.Singleton.ColorModel.coloredRings[(int) NodeColor], gameObject.transform);
                coloredRing.transform.SetParent(gameObject.transform);
                coloredRing.transform.position = gameObject.transform.position;
                coloredRing.SetActive(true);
            }
        }

        public void DisplayColor() {
            ColorSurface.SetActive(true);
            DarkSurface.SetActive(false);
        }

        public void DisplayGrayScale() {
            ColorSurface.SetActive(false);
            DarkSurface.SetActive(true);
        }
    }

}