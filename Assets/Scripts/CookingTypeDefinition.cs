using UnityEngine;

namespace TinyChef
{
    [System.Serializable]
    public struct CookingTypeDefinition
    {
        public CookingType cookingType;
        public float duration; // Cooking duration in seconds

        public CookingTypeDefinition(CookingType type, float duration)
        {
            this.cookingType = type;
            this.duration = duration;
        }
    }
}
