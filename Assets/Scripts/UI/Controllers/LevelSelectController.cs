using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace StudioByStorm.UI.Controllers {

    public class LevelSelectController : Controller
    {
        public GameObject LevelSelectButtonGO;
        public GameObject LevelButtonSpawnLocation;
        
        void OnEnable()
        {
            StartCoroutine(DelayedEnable());
        }

        IEnumerator DelayedEnable()
        {
            yield return 0;
            string dir = Application.persistentDataPath;
            int LevelFileCountInDir = Directory.GetFiles(dir, "*", SearchOption.AllDirectories).Length;

            for (int i = 0; i < LevelFileCountInDir; i++) {
                GameObject go = Instantiate(LevelSelectButtonGO, LevelButtonSpawnLocation.transform);
                LevelSelectButtonView LevelSelectButtonView = go.GetComponent<LevelSelectButtonView>();
                LevelSelectButtonView.GetComponent<Button>().onClick.AddListener(delegate { LevelSelectButtonClick(LevelSelectButtonView.ID); });
                for (int j = 0; j < LevelSelectButtonView.levelNumberText.Length; j++) {
                    LevelSelectButtonView.ID = i;
                    LevelSelectButtonView.levelNumberText[j].text = (i + 1).ToString();
                }
            }
        }

        public void LevelSelectButtonClick(int ID)
        {
            GameManager.Singleton.LevelManager.currentLevelID = ID;
            StartController StartController = GameManager.Singleton.ControllerRegistry.TryGetValue(ViewName.StartView) as StartController;
            GameManager.Singleton.UIController.Back();
            StartController.PlayButtonClick();
        }
    }

}