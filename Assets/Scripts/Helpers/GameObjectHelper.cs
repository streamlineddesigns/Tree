using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StudioByStorm.Helpers {

	public class GameObjectHelper : MonoBehaviour
    {
        public static List<T> GetComponentsOfType<T>(GameObject go, bool bNeedsToBeActive, List<T> components = null)
        {
            components = (components == null) ? new List<T>() : components;

            if (bNeedsToBeActive && go.activeSelf && go.TryGetComponent<T>(out T a)) {
                components.Add(a);
            }

            for (int i = 0; i < go.transform.childCount; i++) {
                if (go.transform.GetChild(i).childCount > 0) {
                    GetComponentsOfType<T>(go.transform.GetChild(i).gameObject, bNeedsToBeActive, components);
                }
            }
            
            return components;
        }
    }

}