using System;
using System.Collections.Generic;
using UnityEngine;

namespace TinyChef
{
    [Serializable]
    public class PortalColorPrefab
    {
        public PortalColor color;
        public GameObject prefab;
    }

    [Serializable]
    public class CookingTypeSprite
    {
        public CookingType cookingType;
        public Sprite sprite;
    }

    [CreateAssetMenu(fileName = "GameSettings", menuName = "TinyChef/Game Settings")]
    public class GameSettings : ScriptableObject
    {
        [Header("Darkness Settings")] [Tooltip("Environment lighting intensity when level is dark")] [Range(0f, 1f)]
        public float darkLevelEnvironmentIntensity = 0f;

        [Tooltip("Environment reflections intensity when level is dark")] [Range(0f, 1f)]
        public float darkLevelReflectionIntensity = 0.5f;

        [Header("Portal Settings")] public List<PortalColorPrefab> portalPrefabs = new List<PortalColorPrefab>();

        [Header("Cooking Type Settings")] public List<CookingTypeSprite> cookingTypeSprites = new List<CookingTypeSprite>();

        [Header("Input Settings")] public KeyCode navigationButton = KeyCode.Q;
        public KeyCode interactionButton = KeyCode.E;
        public float longPressNavigationInterval = 0.2f;
        public float longPressThreshold = 0.5f;

        // Helper method to get portal prefab by color
        public GameObject GetPortalPrefab(PortalColor color)
        {
            foreach (var pair in portalPrefabs)
            {
                if (pair.color == color)
                {
                    return pair.prefab;
                }
            }

            return null;
        }

        // Helper method to get cooking type sprite
        public Sprite GetCookingTypeSprite(CookingType cookingType)
        {
            foreach (var pair in cookingTypeSprites)
            {
                if (pair.cookingType == cookingType)
                {
                    return pair.sprite;
                }
            }

            return null;
        }
    }
}