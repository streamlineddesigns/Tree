using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace StudioByStorm.Data {

    [System.Serializable]
    public struct TextData
    {
        public TMP_Text textField;
        public TMP_Text[] textFields;
        public string[] translations;
    }

}