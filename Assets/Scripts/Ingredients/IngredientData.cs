using System.Collections.Generic;
using UnityEngine;

namespace TinyChef
{
    [CreateAssetMenu(menuName = "Recipes/Ingredient", fileName = "New Ingredient")]
    public class IngredientData : ScriptableObject
    {
        public new string name;
        public Sprite icon;
        public Ingredient prefab;

        [Header("Cooking Settings")]
        public List<CookingTypeDefinition> allowedCookingTypes = new List<CookingTypeDefinition>();

        /// <summary>
        /// Gets the cooking duration for a specific cooking type, returns -1 if not allowed
        /// </summary>
        public float GetCookingDuration(CookingType cookingType)
        {
            foreach (var def in allowedCookingTypes)
            {
                if (def.cookingType == cookingType)
                {
                    return def.duration;
                }
            }
            return -1f; // Not allowed
        }

        /// <summary>
        /// Checks if a cooking type is allowed for this ingredient
        /// </summary>
        public bool IsCookingTypeAllowed(CookingType cookingType)
        {
            return GetCookingDuration(cookingType) >= 0f;
        }
    }
}