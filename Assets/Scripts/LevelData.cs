using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TinyChef
{
    [CreateAssetMenu(menuName = "Levels/Level Data", fileName = "New Level Data")]
    public class LevelData : ScriptableObject
    {
        public float durationBetweenOrders;
        public List<Order> orders;
    }
}