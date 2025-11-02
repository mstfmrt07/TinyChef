using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TinyChef
{
    [System.Serializable]
    public class Range<T>
    {
        public T min;
        public T max;

        public Range(T min, T max)
        {
            this.min = min;
            this.max = max;
        }
    }
}