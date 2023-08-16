using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace StudioByStorm.PCG {

    public class Shuffle
    {
        private System.Random rng;

        public Shuffle()
        {
            rng = new System.Random(); 
        }

        public List<T> FisherYates<T>(List<T> listToShuffle)
        {
            int count = listToShuffle.Count;  
            while (count > 1) {  
                count--;  
                int k = rng.Next(count + 1);  
                T value = listToShuffle[k];  
                listToShuffle[k] = listToShuffle[count];  
                listToShuffle[count] = value;
            }

            return listToShuffle;
        }
    }

}