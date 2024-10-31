using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using StudioByStorm.Data.LevelPacks;
using StudioByStorm.Data.LevelChapters;

namespace StudioByStorm.UI.Controllers {

    public class LevelPackSelectController : Controller
    {
        public List<LevelPackData> levelPacks = new List<LevelPackData>();
        
        public static LevelPackName currentLevelPackName;
        public static string currentLevelPackAlias;
        public static GameObject currentLevelPackGO;
        public static LevelPack currentLevelPack;

        public void HomeButtonClick()
        {
            AudioManager.Singleton.Play(SoundType.ButtonPress);
            GameManager.Singleton.UIController.ShowView(ViewName.StartView);
        }
    
        public void SelectOriginalLevelPack()
        {
            SelectLevelPack(LevelPackName.Original);
        }

        public void SelectSquareLevelPack()
        {
            SelectLevelPack(LevelPackName.Square);
        }

        public void SelectLineLevelPack()
        {
            SelectLevelPack(LevelPackName.Line);
        }

        public void SelectZigZagLevelPack()
        {
            SelectLevelPack(LevelPackName.ZigZag);
        }

        public void SelectShapeLevelPack()
        {
            SelectLevelPack(LevelPackName.Shape);
        }

        public void SelectPolygonLevelPack()
        {
            SelectLevelPack(LevelPackName.Polygon);
        }

        public void SelectLoopLevelPack()
        {
            SelectLevelPack(LevelPackName.Loop);
        }

        public void SelectGroupLevelPack()
        {
            SelectLevelPack(LevelPackName.Group);
        }

        public void SelectAsymetricalLevelPack()
        {
            
        }

        public void SelectRectangleLevelPack()
        {
            SelectLevelPack(LevelPackName.Rectangle);
        }

        private void SelectLevelPack(LevelPackName levelPackName)
        {
            if (currentLevelPackGO != null) {
                Destroy(currentLevelPackGO);
            }

            LevelPackData currentLevelPackData = levelPacks.Where(x => x.name == levelPackName).First();
            currentLevelPackGO = Instantiate(currentLevelPackData.prefab);
            currentLevelPack = currentLevelPackGO.GetComponent<LevelPack>();
            DontDestroyOnLoad(currentLevelPackGO);

            currentLevelPackName = currentLevelPackData.name;
            currentLevelPackAlias = currentLevelPackData.alias;

            AudioManager.Singleton.Play(SoundType.ButtonPress);

            LevelSelectController levelSelectController = GameManager.Singleton.ControllerRegistry.TryGetValue(ViewName.LevelSelectView) as LevelSelectController;
            levelSelectController.Show();
        }
    }

}