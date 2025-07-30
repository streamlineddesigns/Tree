using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StudioByStorm.Obstacles {

    public class ObstaclePart : MonoBehaviour
    {
        public ColorType colorType;

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
            SetColor();
        }

        protected void SetColor()
        {
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