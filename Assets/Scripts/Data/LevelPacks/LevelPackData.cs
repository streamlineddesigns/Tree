using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StudioByStorm.Data.LevelPacks {

    //just assign a name and add a levelpackdata prefab
    [System.Serializable]
    public struct LevelPackData
    {
        public LevelPackName name;
        public string[] aliasTranslations;
        public GameObject prefab;
        public GameObject UICard;
    }

}