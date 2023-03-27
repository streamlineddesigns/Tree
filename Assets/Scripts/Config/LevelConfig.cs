using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StudioByStorm.Config {

    [System.Serializable]
    public class LevelConfig : MonoBehaviour
    {
        public LevelConfig(string fna, string fnp, string sf = "Levels")
        {
            fileNameAppend = fna;
            fileNamePrepend = fnp;
            subfolder = sf;
        }
        public string fileNameAppend = "/LevelID";
        public string fileNamePrepend = ".json";
        public string subfolder = "Levels";
    }

}