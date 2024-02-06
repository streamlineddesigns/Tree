using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace StudioByStorm.UI {

    public class PlayerHeartsView : View
    {
        [SerializeField] private Color onHeartColor;
        private Color offHeartColor;
        [SerializeField] private GameObject heartViewPrefab;
        private List<HeartView> heartViews = new List<HeartView>();
        private int lastHeartIndex;

        void Start()
        {
            offHeartColor = GameManager.Singleton.ColorModel.darkColor[(int) NodeColor.GrayScale];
        }

        public void SetPlayerHearts(int heartCount = 3)
        {
            lastHeartIndex = heartCount - 1;

            for (int i = 0; i < gameObject.transform.childCount; i++) {
                Destroy(gameObject.transform.GetChild(i).gameObject);
            }

            for (int j = 0; j < heartCount; j++) {
                GameObject currentHeartViewGO = Instantiate(heartViewPrefab, gameObject.transform);
                HeartView currentHeartView = currentHeartViewGO.GetComponent<HeartView>();
                currentHeartView.TurnOn(onHeartColor);
                heartViews.Add(currentHeartView);
                currentHeartViewGO.SetActive(true);
            }
        }

        public void LoseHeart()
        {
            if ((lastHeartIndex - 1) >= -1) {
                heartViews[lastHeartIndex].TurnOff(offHeartColor);
                lastHeartIndex--;
            }
        }
    }

}