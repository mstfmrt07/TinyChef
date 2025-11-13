using System.Collections.Generic;
using UnityEngine;

namespace TinyChef
{
    public class Level : MonoBehaviour
    {
        [Header("Counter Groups")]
        public List<CounterGroup> counterGroups = new List<CounterGroup>();

        public List<BaseCounter> GetCounters()
        {
            List<BaseCounter> allCounters = new List<BaseCounter>();
            foreach (var group in counterGroups)
            {
                if (group != null)
                {
                    allCounters.AddRange(group.Counters);
                }
            }
            return allCounters;
        }

        public BaseCounter GetCounterAtIndex(int index)
        {
            List<BaseCounter> allCounters = GetCounters();
            if (index >= 0 && index < allCounters.Count)
            {
                return allCounters[index];
            }
            return null;
        }

        public int CounterCount
        {
            get
            {
                int total = 0;
                foreach (var group in counterGroups)
                {
                    if (group != null)
                    {
                        total += group.CounterCount;
                    }
                }
                return total;
            }
        }

        public CounterGroup GetFirstCounterGroup()
        {
            if (counterGroups != null && counterGroups.Count > 0)
            {
                return counterGroups[0];
            }
            return null;
        }

        public List<CounterGroup> GetCounterGroups()
        {
            return new List<CounterGroup>(counterGroups);
        }

        private void Start()
        {
            // Auto-pair portals by color
            PairPortalsByColor();
        }

        private void PairPortalsByColor()
        {
            List<PortalCounter> portals = new List<PortalCounter>();
            
            // Collect all portal counters from all counter groups
            foreach (var group in counterGroups)
            {
                if (group == null) continue;
                
                foreach (var counter in group.Counters)
                {
                    if (counter is PortalCounter portal)
                    {
                        portals.Add(portal);
                    }
                }
            }

            // Group portals by color
            Dictionary<PortalColor, List<PortalCounter>> portalsByColor = new Dictionary<PortalColor, List<PortalCounter>>();
            
            foreach (var portal in portals)
            {
                if (!portalsByColor.ContainsKey(portal.Color))
                {
                    portalsByColor[portal.Color] = new List<PortalCounter>();
                }
                portalsByColor[portal.Color].Add(portal);
            }

            // Pair portals of the same color
            foreach (var colorGroup in portalsByColor.Values)
            {
                if (colorGroup.Count == 2)
                {
                    // Pair the two portals
                    colorGroup[0].SetPairedPortal(colorGroup[1]);
                    colorGroup[1].SetPairedPortal(colorGroup[0]);
                    Debug.Log($"Paired {colorGroup[0].Color} portals: {colorGroup[0].gameObject.name} <-> {colorGroup[1].gameObject.name}");
                }
                else if (colorGroup.Count > 2)
                {
                    Debug.LogWarning($"Found {colorGroup.Count} portals with color {colorGroup[0].Color}. Only 2 portals per color are supported for pairing.");
                }
                else if (colorGroup.Count == 1)
                {
                    Debug.LogWarning($"Found only 1 portal with color {colorGroup[0].Color}. Portal pairing requires 2 portals of the same color.");
                }
            }
        }

        private void OnValidate()
        {
            counterGroups.RemoveAll(group => group == null);
        }
    }
}
