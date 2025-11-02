using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TinyChef
{
    [CreateAssetMenu(menuName = "Levels/Level Data", fileName = "New Level Data")]
    public class LevelData : ScriptableObject
    {
        [Header("Level Settings")]
        public string levelName;
        public GameObject levelPrefab;
        public float durationBetweenOrders;
        public List<Order> availableOrders;

        [Header("Star Rating Thresholds")]
        public int oneStarScore = 100;
        public int twoStarScore = 250;
        public int threeStarScore = 500;
    }
}