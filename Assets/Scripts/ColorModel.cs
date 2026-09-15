using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace StudioByStorm {

    public class ColorModel : MonoBehaviour
    {
        public List<NodeColor> colorsInUse;
        public Sprite[] ColoredTravelers;
        public Sprite[] ColoredSetters;
        public Sprite[] ColoredGetters;
        public Sprite[] ColoredInners;
        public Color[] lightColor;
        public Color[] darkColor;
        public GameObject[] coloredRings;
        public Material litMaterial;
        public Material unlitMaterial;
    }

}