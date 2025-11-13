using System;
using System.Collections.Generic;
using UnityEngine;

namespace TinyChef
{
    public class CounterGroup : MonoBehaviour
    {
        private List<BaseCounter> counters = new List<BaseCounter>();

        public List<BaseCounter> Counters => new List<BaseCounter>(counters);

        public int CounterCount => counters.Count;

        public static event Action<CounterGroup> OnCounterGroupChanged;

        private void Awake()
        {
            RefreshCounters();
        }

        private void OnValidate()
        {
            RefreshCounters();
        }

        private void RefreshCounters()
        {
            counters.Clear();
            BaseCounter[] childCounters = GetComponentsInChildren<BaseCounter>(true);
            counters.AddRange(childCounters);
        }

        public BaseCounter GetCounterAtIndex(int index)
        {
            if (index >= 0 && index < counters.Count)
            {
                return counters[index];
            }
            return null;
        }

        public static void SetActiveGroup(CounterGroup group)
        {
            OnCounterGroupChanged?.Invoke(group);
        }
    }
}

