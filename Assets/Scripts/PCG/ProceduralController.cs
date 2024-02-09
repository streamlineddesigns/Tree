using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using StudioByStorm.Graph;
using StudioByStorm.Config;
using StudioByStorm.Data;
using StudioByStorm.Repositories;

namespace StudioByStorm.PCG {

    public class ProceduralController : MonoBehaviour
    {
        [SerializeField] private LevelConfig LevelConfig;
        [SerializeField] private ObstacleDataRepository ObstacleDataRepository;
        [SerializeField] private  GraphConstructionManager GraphConstructionManager;
        [SerializeField] private  ProceduralLevelGenerator ProceduralLevelGenerator;
        [SerializeField] private  ProceduralObstacleSelector ProceduralObstacleSelector;
        [SerializeField] private  ProceduralObstaclePlacer ProceduralObstaclePlacer;

        public void Create()
        {
            StartCoroutine(CreateRoutine());
        }

        IEnumerator CreateRoutine()
        {
            ProceduralLevelGenerator.DependencyInjection(GraphConstructionManager);
            yield return StartCoroutine(ProceduralLevelGenerator.CreateLevel());

            ProceduralObstacleSelector.DependencyInjection(ObstacleDataRepository, GraphConstructionManager, LevelConfig);
            yield return StartCoroutine(ProceduralObstacleSelector.SelectObstacles());

            ProceduralObstaclePlacer.DependencyInjection(ObstacleDataRepository, GraphConstructionManager);
            yield return StartCoroutine(ProceduralObstaclePlacer.PlaceObstacles());
        }

        public void Save()
        {
            if (! GraphConstructionManager.isGlobalLevelDataSet) {
                GraphConstructionManager.CreateLevelData();
            }

            LevelData LevelData = GraphConstructionManager.GlobalLevelData;

            string dir = Application.persistentDataPath + "/" + LevelConfig.subfolder;
            if (! Directory.Exists(dir)) {
                Directory.CreateDirectory(dir);
            }
            int LevelFileCountInDir = Directory.GetFiles(dir, "*", SearchOption.AllDirectories).Length;
            string LevelSaveFilePath = (dir + LevelConfig.fileNameAppend + LevelFileCountInDir + LevelConfig.fileNamePrepend).ToString();

            Debug.Log(LevelSaveFilePath);

            File.WriteAllText(LevelSaveFilePath, JsonConvert.SerializeObject(LevelData, Formatting.Indented, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            }));

            SceneManager.LoadScene("Graph");
        }

        public void Reload()
        {
            SceneManager.LoadScene("Graph");
        }
    }

}