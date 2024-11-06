using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StudioByStorm.Data {

    [System.Serializable]
    public struct TutorialData
    {
        //tutorial message
        public string[] messageTranslations;
        //first level the tutorial can be shown
        public int levelID;
        public int chapterID;
        //tutorial specific prefab that will be instantiated for specific tutorials
        public GameObject prefab;
    }

}