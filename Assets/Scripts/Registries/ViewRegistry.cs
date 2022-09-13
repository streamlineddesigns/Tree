using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StudioByStorm.UI;
using StudioByStorm.Helpers;

namespace StudioByStorm.Registries {

    public class ViewRegistry : Registry<ViewName, View>
    {        
        public View[] Views;

        void Start()
        {        
            for (int i = 0; i < Views.Length; i++) {
                Add(Views[i].ViewName, Views[i]);
            }
        }
        
    }

}