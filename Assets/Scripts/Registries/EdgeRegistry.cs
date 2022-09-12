using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StudioByStorm.Helpers;

namespace StudioByStorm.Registries {

    public class EdgeRegistry : Registry<int, Edge>
    {        
        public GameObject EdgeParent;

        void Start()
        {
            StartCoroutine(LateStart());
        }

        IEnumerator LateStart()
        {
            yield return 0;

            List<Edge> Edges = GameObjectHelper.GetComponentsOfType<Edge>(EdgeParent, false);
            
            for (int i = 0; i < Edges.Count; i++) {
                Add(Edges[i].parentID, Edges[i]);
            }
        }
    }

}