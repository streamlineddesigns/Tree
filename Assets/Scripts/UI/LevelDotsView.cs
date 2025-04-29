using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace StudioByStorm.UI {

    public class LevelDotsView : View
    {
        [SerializeField] private Color OnColor;
        [SerializeField] private Color OffColor;
        [SerializeField] private Color bossColor;
        [SerializeField] public List<Image> LevelDotImages = new List<Image>();

        public void SetLevelDots(int dotCount)
        {
            dotCount = (dotCount >= 6) ? 6 : dotCount;

            for (int j = 0; j < dotCount; j++) {
                if (dotCount == 6 && j == 5) {
                    LevelDotImages[j].color = bossColor;
                } else {
                    LevelDotImages[j].color = OnColor;
                }
            }
        }
    }

}