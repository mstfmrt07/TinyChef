using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TinyChef
{
    public class StoveCounter : BaseCounter
    {
        [Header("Stove Settings")]
        public CookingType defaultCookingType = CookingType.Boiled;
        public StoveUI stoveUI;

        public Action<Ingredient> OnItemCooked;
        public Action OnCookingStarted;
        public Action OnCookingCompleted;

        private List<Ingredient> ingredients = new List<Ingredient>();
        private bool isCooking = false;
        private float totalCookingTime = 0f;
        private float currentCookingTime = 0f;
        private Dictionary<Ingredient, CookingTypeDefinition> ingredientCookingDefs = new Dictionary<Ingredient, CookingTypeDefinition>();
        private Dictionary<Ingredient, float> ingredientStartTimes = new Dictionary<Ingredient, float>();

        public List<Ingredient> GetIngredients() => new List<Ingredient>(ingredients);
        public bool IsCooking => isCooking;
        public float GetCookingProgress()
        {
            if (!isCooking || ingredients.Count == 0) return 0f;
            
            // Calculate remaining total time for uncooked ingredients only
            float remainingTime = 0f;
            foreach (var ingredient in ingredients)
            {
                // Only count uncooked ingredients
                if (ingredient.State != IngredientState.Cooked && ingredientCookingDefs.ContainsKey(ingredient))
                {
                    remainingTime += ingredientCookingDefs[ingredient].duration;
                }
            }
            
            if (remainingTime <= 0f || totalCookingTime <= 0f) return 1f;
            
            // Calculate completed time
            float completedTime = totalCookingTime - remainingTime;
            return Mathf.Clamp01(completedTime / totalCookingTime);
        }

        private void Awake()
        {
            counterType = CounterType.Stove;
            if (itemPlacePoint == null)
            {
                itemPlacePoint = transform;
            }
        }

        public override void Interact()
        {
            // Override to use list-based logic instead of single currentItem
            Chef chef = FindObjectOfType<Chef>();
            if (chef == null) return;

            if (chef.CurrentItem == null)
            {
                // Try to pick up an ingredient from the stove
                TryPickUpItem();
            }
            else
            {
                // Try to put down the ingredient the chef is holding
                TryPutDownItem();
            }
        }

        private void Start()
        {
            if (stoveUI != null)
            {
                stoveUI.Initialize(this);
                UpdateUIVisibility();
            }
        }

        private void UpdateUIVisibility()
        {
            if (stoveUI != null)
            {
                // Show UI only when there's at least one ingredient
                bool shouldBeVisible = ingredients.Count > 0;
                stoveUI.SetVisible(shouldBeVisible);
            }
        }

        private void Update()
        {
            // Update cooking if there are uncooked ingredients
            bool hasUncookedIngredients = ingredients.Any(i => i.State != IngredientState.Cooked);
            if (hasUncookedIngredients)
            {
                if (!isCooking)
                {
                    StartCooking();
                }
                UpdateCooking();
            }
        }

        protected override bool TryPutDownItem()
        {
            Chef chef = FindObjectOfType<Chef>();
            if (chef == null || chef.CurrentItem == null) return false;

            IItem item = chef.CurrentItem;
            
            // Stove only accepts ingredients, not plates
            if (!(item is Ingredient ingredient)) return false;

            if (CanPlaceItem(item))
            {
                // Check if ingredient can be cooked with default cooking type
                if (ingredient.data != null && ingredient.data.IsCookingTypeAllowed(defaultCookingType))
                {
                    chef.DropItem();
                    AddIngredient(ingredient);
                    return true;
                }
                else
                {
                    Debug.Log($"Ingredient {ingredient.data.name} cannot be cooked with {defaultCookingType}");
                    return false;
                }
            }

            return false;
        }

        protected override bool CanPlaceItem(IItem item)
        {
            if (item == null) return false;
            
            // Stove only accepts ingredients, not plates
            if (!(item is Ingredient ingredient)) return false;
            
            if (ingredient.data == null) return false;
            
            // Can't place already cooked items
            if (ingredient.State == IngredientState.Cooked) return false;

            // Check if ingredient can be cooked with default cooking type
            return ingredient.data.IsCookingTypeAllowed(defaultCookingType);
        }

        private void AddIngredient(Ingredient ingredient)
        {
            if (ingredient == null || ingredient.data == null) return;

            // Calculate start time for this ingredient (cumulative of all previous ingredients)
            float startTime = 0f;
            foreach (var ing in ingredients)
            {
                if (ingredientCookingDefs.ContainsKey(ing))
                {
                    startTime += ingredientCookingDefs[ing].duration;
                }
            }

            ingredients.Add(ingredient);

            // Get cooking definition for this ingredient
            float duration = ingredient.data.GetCookingDuration(defaultCookingType);
            if (duration > 0f)
            {
                var cookingDef = new CookingTypeDefinition(defaultCookingType, duration);
                ingredientCookingDefs[ingredient] = cookingDef;
                ingredientStartTimes[ingredient] = startTime;
            }

            // Position ingredient
            PositionIngredient(ingredient, ingredients.Count - 1);

            // Update total cooking time and start cooking if needed
            RecalculateTotalCookingTime();
            
            if (!isCooking && ingredients.Count > 0)
            {
                StartCooking();
            }

            // Update UI visibility
            UpdateUIVisibility();
        }

        private void PositionIngredient(Ingredient ingredient, int index)
        {
            if (itemPlacePoint != null)
            {
                ingredient.transform.SetParent(itemPlacePoint);
                Vector3 offset = Vector3.up * (index * 0.2f);
                ingredient.transform.localPosition = offset;
                ingredient.transform.localRotation = Quaternion.identity;
            }
        }

        private void RecalculateTotalCookingTime()
        {
            totalCookingTime = 0f;
            foreach (var ingredient in ingredients)
            {
                // Only count uncooked ingredients for total cooking time
                if (ingredientCookingDefs.ContainsKey(ingredient) && ingredient.State != IngredientState.Cooked)
                {
                    totalCookingTime += ingredientCookingDefs[ingredient].duration;
                }
            }
        }

        private void AdjustStartTimes()
        {
            // Recalculate start times for remaining ingredients based on their order
            float accumulatedTime = 0f;
            foreach (var ingredient in ingredients)
            {
                if (ingredientCookingDefs.ContainsKey(ingredient))
                {
                    ingredientStartTimes[ingredient] = accumulatedTime;
                    accumulatedTime += ingredientCookingDefs[ingredient].duration;
                }
            }
        }

        private void StartCooking()
        {
            if (ingredients.Count == 0) return;

            isCooking = true;
            currentCookingTime = 0f;
            OnCookingStarted?.Invoke();
        }

        private void UpdateCooking()
        {
            // Check if there are any uncooked ingredients
            bool hasUncookedIngredients = ingredients.Any(i => i.State != IngredientState.Cooked);
            
            if (!hasUncookedIngredients)
            {
                // All ingredients are cooked - stop the cooking timer
                if (isCooking)
                {
                    StopCooking();
                    OnCookingCompleted?.Invoke();
                }
                return;
            }

            currentCookingTime += Time.deltaTime;

            // Check which ingredients are done based on their start times
            List<Ingredient> finishedIngredients = new List<Ingredient>();

            foreach (var ingredient in ingredients.ToList())
            {
                // Only check uncooked ingredients
                if (ingredient.State != IngredientState.Cooked && 
                    ingredientCookingDefs.ContainsKey(ingredient) && 
                    ingredientStartTimes.ContainsKey(ingredient))
                {
                    float startTime = ingredientStartTimes[ingredient];
                    float duration = ingredientCookingDefs[ingredient].duration;
                    
                    if (currentCookingTime >= startTime + duration)
                    {
                        // This ingredient is done
                        finishedIngredients.Add(ingredient);
                    }
                }
            }

            // Complete cooking for finished ingredients
            foreach (var ingredient in finishedIngredients)
            {
                CompleteCookingForIngredient(ingredient);
            }
        }

        private void CompleteCookingForIngredient(Ingredient ingredient)
        {
            if (ingredient == null) return;

            CookingType cookingType = defaultCookingType;
            if (ingredientCookingDefs.ContainsKey(ingredient))
            {
                cookingType = ingredientCookingDefs[ingredient].cookingType;
            }

            ingredient.Cook(cookingType);
            OnItemCooked?.Invoke(ingredient);

            // Keep ingredient in the list (don't remove it) - it stays in the stove until picked up
            // Just remove from cooking tracking dictionaries
            ingredientCookingDefs.Remove(ingredient);
            ingredientStartTimes.Remove(ingredient);

            // Recalculate total time for remaining uncooked ingredients
            RecalculateTotalCookingTime();
        }

        private void StopCooking()
        {
            isCooking = false;
            currentCookingTime = 0f;
            totalCookingTime = 0f;
            ingredientStartTimes.Clear();
            
            // Update UI visibility when cooking stops
            UpdateUIVisibility();
        }

        protected override bool TryPickUpItem()
        {
            Chef chef = FindObjectOfType<Chef>();
            if (chef == null || chef.CurrentItem != null) return false;

            // Only pick up cooked ingredients
            if (ingredients.Count > 0)
            {
                // Find cooked ingredients only
                Ingredient itemToPick = ingredients.FirstOrDefault(i => i.State == IngredientState.Cooked);

                if (itemToPick != null)
                {
                    chef.GrabItem(itemToPick);
                    ingredients.Remove(itemToPick);
                    ingredientCookingDefs.Remove(itemToPick);
                    ingredientStartTimes.Remove(itemToPick);
                    
                    // Recalculate if still cooking
                    if (ingredients.Count > 0)
                    {
                        RecalculateTotalCookingTime();
                        // Adjust start times for remaining ingredients
                        AdjustStartTimes();
                    }
                    else
                    {
                        StopCooking();
                    }

                    // Update UI visibility
                    UpdateUIVisibility();
                    
                    return true;
                }
                else
                {
                    // No cooked ingredients available
                    Debug.Log("Ingredients are still cooking, cannot pick up yet.");
                    return false;
                }
            }

            return false;
        }

        protected override bool CanProcess(IItem item)
        {
            // Stove processes automatically when ingredients are placed
            return false;
        }
    }
}
