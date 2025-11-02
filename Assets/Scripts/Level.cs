using System.Collections.Generic;
using UnityEngine;

namespace TinyChef
{
    public class Level : MonoBehaviour
    {
        [Header("Level Settings")]
        public List<BaseCounter> counters = new List<BaseCounter>();

        /// <summary>
        /// Gets all counters in this level
        /// </summary>
        public List<BaseCounter> GetCounters()
        {
            return new List<BaseCounter>(counters);
        }

        /// <summary>
        /// Gets counter at the specified index
        /// </summary>
        public BaseCounter GetCounterAtIndex(int index)
        {
            if (index >= 0 && index < counters.Count)
            {
                return counters[index];
            }
            return null;
        }

        /// <summary>
        /// Gets the total number of counters in this level
        /// </summary>
        public int CounterCount => counters.Count;

        /// <summary>
        /// Validates that all counters in the list are not null
        /// </summary>
        private void OnValidate()
        {
            // Remove null entries
            counters.RemoveAll(counter => counter == null);
        }
    }
}
