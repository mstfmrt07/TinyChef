using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TinyChef
{
    [CreateAssetMenu(menuName = "Recipes/Recipe", fileName = "New Recipe")]
    public class RecipeData : ScriptableObject
    {
        public new string name;
        [TextArea] public string description;
        public Sprite icon;
        public bool unlockedByDefault = true;
        public List<IngredientDefinition> requiredIngredients;

        public string GetDisplayName()
        {
            return string.IsNullOrWhiteSpace(name) ? base.name : name;
        }

        public string GetId()
        {
            return GetDisplayName();
        }

        public Sprite GetDisplaySprite()
        {
            return icon;
        }

        public bool IsSatisfied(List<Ingredient> ingredients)
        {
            if (ingredients == null || ingredients.Count != requiredIngredients.Count)
                return false;

            // Create a list of required ingredient definitions
            var required = new List<IngredientDefinition>(requiredIngredients);

            foreach (var ingredient in ingredients)
            {
                // Try to find a matching requirement
                var match = required.FirstOrDefault(req => 
                    req.item == ingredient.data &&
                    req.targetState == ingredient.State &&
                    req.targetCookingType == ingredient.CookingType);

                if (match.item == null)
                {
                    // No match found
                    return false;
                }

                // Remove the matched requirement
                required.Remove(match);
            }

            // All requirements satisfied
            return required.Count == 0;
        }
    }
}