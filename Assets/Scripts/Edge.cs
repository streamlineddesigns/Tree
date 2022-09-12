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
        }
    }

}