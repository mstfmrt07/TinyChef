using UnityEngine;
using UnityEngine.Serialization;

namespace TinyChef
{
    [System.Serializable]
    public struct IngredientDefinition
    {
        public IngredientData item;
        public IngredientState targetState;
        public CookingType targetCookingType;
    }
}