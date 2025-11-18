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
        [Header("Plate Visual Settings")]
        public MeshRenderer plateRenderer;
        public Material cleanMaterial;
        public Material dirtyMaterial;

        [Header("Stacking Settings")]
        public float stackOffset = 0.05f; // Vertical offset for stacked plates

        private List<Ingredient> ingredients = new List<Ingredient>();
        private bool isDirty = false;
        private List<Plate> stackedPlates = new List<Plate>(); // Plates stacked on top of this one

        public List<Ingredient> Ingredients => new List<Ingredient>(ingredients);
        public int IngredientCount => ingredients.Count;
        public bool IsEmpty => ingredients.Count == 0;
        public bool IsDirty => isDirty;
        public int StackCount => stackedPlates.Count;
        public bool HasStack => stackedPlates.Count > 0;

        /// <summary>
        /// Checks if an ingredient can be added to this plate based on level recipes
        /// </summary>
        public bool CanAddIngredient(Ingredient ingredient, LevelData levelData)
        {
            if (ingredient == null || levelData == null) return false;
            if (ingredients.Contains(ingredient)) return false; // Already in plate
            if (isDirty) return false; // Can't add ingredients to dirty plates

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

        /// <summary>
        /// Makes the plate dirty (after serving)
        /// </summary>
        public void MakeDirty()
        {
            SetClean(false);
        }

        /// <summary>
        /// Cleans the plate (in dishwasher)
        /// </summary>
        public void Clean()
        {
            SetClean(true);
        }

        /// <summary>
        /// Sets the clean/dirty state of the plate
        /// </summary>
        public void SetClean(bool clean)
        {
            isDirty = !clean;
            UpdateVisual();
        }

        /// <summary>
        /// Updates the visual appearance based on dirty state
        /// </summary>
        private void UpdateVisual()
        {
            if (plateRenderer != null)
            {
                if (isDirty && dirtyMaterial != null)
                {
                    plateRenderer.material = dirtyMaterial;
                }
                else if (!isDirty && cleanMaterial != null)
                {
                    plateRenderer.material = cleanMaterial;
                }
            }
        }

        private void Start()
        {
            // Initialize visual on start
            UpdateVisual();
        }

        /// <summary>
        /// Checks if a plate can be stacked on top of this plate
        /// </summary>
        public bool CanStackPlate(Plate otherPlate)
        {
            if (otherPlate == null) return false;
            if (!IsEmpty) return false; // Can only stack on empty plates
            if (!otherPlate.IsEmpty) return false; // Can only stack empty plates
            
            // Both must be the same type (both clean or both dirty)
            return isDirty == otherPlate.IsDirty;
        }

        /// <summary>
        /// Adds a plate to the stack
        /// </summary>
        public bool TryStackPlate(Plate otherPlate)
        {
            if (!CanStackPlate(otherPlate)) return false;

            stackedPlates.Add(otherPlate);
            
            // Position the plate in the stack
            otherPlate.transform.SetParent(transform);
            otherPlate.transform.localPosition = Vector3.up * stackOffset * stackedPlates.Count;
            otherPlate.transform.localRotation = Quaternion.identity;
            
            return true;
        }

        /// <summary>
        /// Removes and returns the top plate from the stack
        /// </summary>
        public Plate TakeTopPlate()
        {
            if (stackedPlates.Count == 0) return null;

            Plate topPlate = stackedPlates[stackedPlates.Count - 1];
            stackedPlates.RemoveAt(stackedPlates.Count - 1);
            
            topPlate.transform.SetParent(null);
            
            return topPlate;
        }

        /// <summary>
        /// Gets the top plate without removing it
        /// </summary>
        public Plate PeekTopPlate()
        {
            if (stackedPlates.Count == 0) return null;
            return stackedPlates[stackedPlates.Count - 1];
        }
    }
}
