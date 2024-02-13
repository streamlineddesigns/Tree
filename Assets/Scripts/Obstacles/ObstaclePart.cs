using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StudioByStorm.Obstacles {

    public class ObstaclePart : MonoBehaviour
    {
        public ColorType colorType;

        protected void Start()
        {
            if (colorType == ColorType.Light) {
                gameObject.GetComponent<SpriteRenderer>().material = GameManager.Singleton.PlayerController.glowMaterial;
            }
        }
    }

}