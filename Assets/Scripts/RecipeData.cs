using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace TinyChef
{
    [CreateAssetMenu(menuName = "Recipes/Recipe", fileName = "New Recipe")]
    public class RecipeData : ScriptableObject
    {
        public new string name;
        public Sprite icon;
        public List<IngredientDefinition> requiredIngredients;

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