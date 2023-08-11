using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace StudioByStorm.Graph {

    public class Cartesian
    {
        public void Get()
        {
            var listOfLists = new List<List<string>>
            {
                new List<string> { "A1", "A2" },
                new List<string> { "B1", "B2" },
                new List<string> { "C1", "C2" },
                new List<string> { "D1", "D2" }
            };

            foreach (var combination in CartesianProduct(listOfLists))
            {
                Debug.Log(string.Join(", ", combination));
            }
        }

        public static IEnumerable<IEnumerable<T>> CartesianProduct<T>(IEnumerable<IEnumerable<T>> sequences)
        {
            IEnumerable<IEnumerable<T>> tempProduct = new[] { Enumerable.Empty<T>() };

            foreach (var sequence in sequences)
            {
                var s = sequence;
                tempProduct = 
                    from accseq in tempProduct 
                    from item in s 
                    select accseq.Concat(new[] { item });
            }

            return tempProduct;
        }
    }

}