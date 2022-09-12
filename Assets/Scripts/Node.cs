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
            
        }

        public void DisplayColor() {
            ColorSurface.SetActive(true);
            DarkSurface.SetActive(false);
        }
    }

}