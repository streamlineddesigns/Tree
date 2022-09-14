using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StudioByStorm.Registries {

    public class ColorNodeRegistry : Registry<NodeColor, List<Node>>
    {        
        public void Add(NodeColor nodeColor, Node node)
        {
            if (! _Registry.ContainsKey(nodeColor)) {
                List<Node> nodes = new List<Node>();
                _Registry[nodeColor] = nodes;
                
            }

            if (! _Registry[nodeColor].Contains(node)) {
                _Registry[nodeColor].Add(node);
            }
        }
    }

}