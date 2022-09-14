using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StudioByStorm.Registries {

    public class ColorEdgeRegistry : Registry<NodeColor, List<Edge>>
    {        
        public void Add(NodeColor nodeColor, Edge edge)
        {
            if (! _Registry.ContainsKey(nodeColor)) {
                List<Edge> edges = new List<Edge>();
                _Registry[nodeColor] = edges;
                _Registry[nodeColor].Add(edge);
            } else {
                _Registry[nodeColor].Add(edge);
            }
        }
    }

}