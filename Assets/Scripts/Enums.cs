using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StudioByStorm {

    public enum NodeType
    {
        Disjoint,
        Parent,
        Child
    }

    public enum NodeColor
    {
        Blue,
        Green,
        Purple,
        White,
        GrayScale
    }

    public enum NodeAction
    {
        TravelNode,
        GetEdge,
        SetEdge,
        TravelEdge
    }
    
}