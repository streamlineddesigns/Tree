using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StudioByStorm.Data {

    [System.Serializable]
    public struct TutorialData
    {
        //tutorial message
        public string message;
        //first level the tutorial can be shown
        public int levelID;
    }

}