using System;
using System.Collections.Generic;
using UnityEngine;

namespace TinyChef.LevelEditor
{
    [CreateAssetMenu(menuName = "TinyChef/Counter Profile", fileName = "CounterProfile")]
    public class CounterProfile : ScriptableObject
    {
        [Header("Geometry")]
        [Min(0.01f)] public float innerRadius = 0.6f;
        [Min(0.02f)] public float outerRadius = 1.0f;
        [Min(0.01f)] public float baseHeight = 0.8f;
        [Min(0.01f)] public float topThickness = 0.05f;
        [Tooltip("Top overhang outward beyond base outer radius")]
        [Min(0f)] public float topOverhangOuter = 0.0f;
        [Tooltip("Top inset inward beyond base inner radius (positive moves inward)")]
        [Min(0f)] public float topOverhangInner = 0.0f;

        [Header("Layout")]
        [Tooltip("Angular gap between adjacent wedges, in degrees")]
        [Min(0f)] public float angleGapDegrees = 0.0f;
        [Tooltip("Radial spacing: shrinks outer by this and grows inner by this")]
        [Min(0f)] public float radialGap = 0.0f;
        [Tooltip("Rotate ring so top is flat (half-step offset)")]
        public bool flatTopOrientation = true;

        [Header("Bevel")]
        [Tooltip("Bevel size applied to top slab outer/inner edges")]
        [Min(0f)] public float bevelSize = 0.0f;
        [Tooltip("Number of segments in the bevel (0 = none, 1 = single chamfer)")]
        [Min(0)] public int bevelSegments = 0;

        [Header("Asset Output")]
        public string outputFolder = "Assets/Meshes/Counters";
        [Tooltip("Duplicate triangles with reversed winding to make mesh double-sided")]
        public bool doubleSided = false;

        [Serializable]
        public class CounterTypeMaterials
        {
            public TinyChef.CounterType type = TinyChef.CounterType.Basic;
            public List<Material> materials = new List<Material>(2); // [0]=base, [1]=top
        }

        [Header("Materials (index 0=Base, 1=Top)")]
        public List<CounterTypeMaterials> typeMaterials = new List<CounterTypeMaterials>()
        {
            new CounterTypeMaterials{ type = TinyChef.CounterType.Basic, materials = new List<Material>{ null, null } }
        };

        public List<Material> GetMaterials(TinyChef.CounterType type)
        {
            for (int i = 0; i < typeMaterials.Count; i++)
            {
                if (typeMaterials[i].type == type) return typeMaterials[i].materials;
            }
            return null;
        }
    }
}


