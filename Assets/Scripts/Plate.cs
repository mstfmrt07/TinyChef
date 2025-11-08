using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TinyChef
{
    /// <summary>
    /// Component for plates that can hold multiple ingredients
    /// </summary>
    public class Plate : MonoBehaviour, IItem
    {
        private List<Ingredient> ingredients = new List<Ingredient>();

        public List<Ingredient> Ingredients => new List<Ingredient>(ingredients);
        public int IngredientCount => ingredients.Count;
        public bool IsEmpty => ingredients.Count == 0;

        /// <summary>
        /// Checks if an ingredient can be added to this plate based on level recipes
        /// </summary>
        public bool CanAddIngredient(Ingredient ingredient, LevelData levelData)
        {
            if (ingredient == null || levelData == null) return false;
            if (ingredients.Contains(ingredient)) return false; // Already in plate

            // Check if ingredient matches any recipe requirement in the level
            if (levelData.availableOrders == null || levelData.availableOrders.Count == 0)
                return false;

            // Check all recipes in the level to see if this ingredient matches any requirement
            foreach (var order in levelData.availableOrders)
            {
                if (order.recipe == null || order.recipe.requiredIngredients == null)
                    continue;

                foreach (var requiredIngredient in order.recipe.requiredIngredients)
                {
                    // Check if ingredient matches this requirement
                    if (requiredIngredient.item == ingredient.data &&
                        requiredIngredient.targetState == ingredient.State &&
                        requiredIngredient.targetCookingType == ingredient.CookingType)
                    {
                        // Check if we already have this exact ingredient (same type, state, cooking type)
                        // In Overcooked, you can have duplicates if recipe requires them
                        // But we should check if adding this would exceed recipe requirements
                        if (CanAddToRecipe(ingredient, order.recipe))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if adding this ingredient would still be valid for the recipe
        /// </summary>
        private bool CanAddToRecipe(Ingredient ingredient, RecipeData recipe)
        {
            if (recipe == null || recipe.requiredIngredients == null) return false;

            // Count how many of this exact ingredient type/state/cooking type are required
            int requiredCount = recipe.requiredIngredients.Count(req =>
                req.item == ingredient.data &&
                req.targetState == ingredient.State &&
                req.targetCookingType == ingredient.CookingType);

            // Count how many we already have
            int currentCount = ingredients.Count(ing =>
                ing.data == ingredient.data &&
                ing.State == ingredient.State &&
                ing.CookingType == ingredient.CookingType);

            // Can add if we haven't reached the required count
            return currentCount < requiredCount;
        }

        /// <summary>
        /// Tries to add an ingredient to the plate, returns true if successful
        /// </summary>
        public bool TryAddIngredient(Ingredient ingredient, LevelData levelData)
        {
            if (!CanAddIngredient(ingredient, levelData))
            {
                return false;
            }

            if (ingredient != null && !ingredients.Contains(ingredient))
            {
                ingredients.Add(ingredient);
                // Attach ingredient to plate
                ingredient.transform.SetParent(transform);
                ingredient.transform.localPosition = Vector3.up * (ingredients.Count - 1) * 0.1f;
                ingredient.transform.localRotation = Quaternion.identity;
                return true;
            }

            return false;
        }

        public void RemoveIngredient(Ingredient ingredient)
        {
            if (ingredients.Contains(ingredient))
            {
                ingredients.Remove(ingredient);
                ingredient.transform.SetParent(null);
            }
        }

        public void Clear()
        {
            foreach (var ingredient in ingredients)
            {
                if (ingredient != null)
                {
                    ingredient.transform.SetParent(null);
                }
            }
            ingredients.Clear();
        }
    }
}
