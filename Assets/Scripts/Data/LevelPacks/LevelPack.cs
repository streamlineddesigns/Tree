using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StudioByStorm.Data.LevelChapters;

namespace StudioByStorm.Data.LevelPacks {
    
    //just attach to a gameobject, make it a prefab and create chapter references
    public class LevelPack : MonoBehaviour
    {
        public Chapters chapters;
        public NodeColor[] colors;
        public ControlType ControlType;
    }

}