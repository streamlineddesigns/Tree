using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StudioByStorm.Obstacles {

    public class ObstaclePart : MonoBehaviour
    {
        public NodeColor NodeColor;
        public ColorType colorType;
        public bool isBoid;
        private Material originalMaterial;

        public void RegisterViaNodeID(int nodeID)
        {
            GameManager.Singleton.ObstaclePartRegistry.Add(nodeID, this);
        }

        public void SwapColor()
        {
            colorType = (colorType == ColorType.Light) ? ColorType.Dark : ColorType.Light;
            SetColor();
        }

        protected void Start()
        {
            if (GameManager.Singleton != null) SetColor();
            originalMaterial = gameObject.GetComponent<SpriteRenderer>().material;
        }

        public void DisableGlow()
        {
            gameObject.GetComponent<SpriteRenderer>().material = originalMaterial;
        }

        public void EnableGlow()
        {
            if (NodeColor == GameManager.Singleton.PlayerController.NodeColor) {
                gameObject.GetComponent<SpriteRenderer>().material = GameManager.Singleton.PlayerController.glowMaterial;
            } else {
                gameObject.GetComponent<SpriteRenderer>().material = originalMaterial;
            }
        }

        protected void SetColor()
        {
            int colorIndex = (int) NodeColor;
            gameObject.GetComponent<SpriteRenderer>().color = GameManager.Singleton.ColorModel.lightColor[colorIndex];
            return;
            

            if (colorType == ColorType.Light) {
                gameObject.GetComponent<SpriteRenderer>().color = GameManager.Singleton.PlayerController.lightColor;
                gameObject.GetComponent<SpriteRenderer>().material = GameManager.Singleton.PlayerController.glowMaterial;
            } else {
                gameObject.GetComponent<SpriteRenderer>().color = GameManager.Singleton.PlayerController.darkColor;
                gameObject.GetComponent<SpriteRenderer>().material = GameManager.Singleton.PlayerController.darkMaterial;
            }
        }
    }

}