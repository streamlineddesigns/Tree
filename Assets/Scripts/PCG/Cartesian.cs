using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace StudioByStorm.PCG {

    public class Cartesian
    {
        private bool IsDebugging;

        public Cartesian(bool isDebugging = false)
        {
            IsDebugging = isDebugging;
        }

        public IEnumerable<IEnumerable<T>> GetCartesianProduct<T>(IEnumerable<IEnumerable<T>> listOfLists)
        {
            IEnumerable<IEnumerable<T>> combinations = CartesianProduct(listOfLists);

            if (IsDebugging) {
                foreach (var combination in combinations)
                {
                    Debug.Log(string.Join(", ", combination));
                }
            }

            return combinations;
        }

        private IEnumerable<IEnumerable<T>> CartesianProduct<T>(IEnumerable<IEnumerable<T>> sequences)
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