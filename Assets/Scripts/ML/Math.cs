using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StudioByStorm.ML {

    public class Math : MonoBehaviour
    {

        public static float GetDistance(float p1, float p2)
        {
            float distance = 0;

            if (p1 > p2) {
                distance = p1 - p2;
                
            } else if (p2 > p1) {
                distance = p2 - p1;
            }

            return distance;
        }

        public static float GetDistance(Vector2 p1, Vector2 p2)
        {
            float x = p1.x - p2.x;
            float y = p1.y - p2.y;
            float c = ( x*x + y*y );
            return Mathf.Sqrt(c);
        }

        public static float GetDistance(float[] p1, float[] p2)
        {
            float distance = 0;

            if (p1.Length != p2.Length) {
                Debug.LogError("Input vectors must be of equal length");
            }

            for (int i = 0; i < p1.Length; i++) {
                float a = p1[i] - p2[i];
                distance += (float) (a * a);
            }

            return distance;
        }

        public static float[] GetCentroid(float[][] data)
        {
            if ((data.Select(x => x.Length).Sum() / data.Length != data[0].Length)) {
                Debug.LogError("Input vectors must be of equal length");
            }

            float[] centroid = new float[data[0].Length];
            float[] counter = new float[data[0].Length];

            for (int i = 0; i < data.Length; i++) {
                for (int j = 0; j < centroid.Length; j++) {
                    centroid[j] += data[i][j];
                    counter[j]++;
                }
            }

            for (int k = 0; k < centroid.Length; k++) {
                centroid[k] /= counter[k];
            }

            return centroid;
        }
    }

}