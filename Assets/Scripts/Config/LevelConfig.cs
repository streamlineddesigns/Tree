using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StudioByStorm.Config {

    [System.Serializable]
    public class LevelConfig : MonoBehaviour
    {
        public LevelConfig(string fna, string fnp)
        {
            fileNameAppend = fna;
            fileNamePrepend = fnp;
        }
        public string fileNameAppend = "/LevelID";
        public string fileNamePrepend = ".json";
    }

}