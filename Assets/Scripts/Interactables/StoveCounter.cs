using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TinyChef
{
    public class StoveCounter : BaseCounter
    {
        [Header("Stove Settings")] public CookingType defaultCookingType = CookingType.Boiled;
        public StoveUI stoveUI;

        public Action<Ingredient> OnItemCooked;
        public Action OnCookingStarted;
        public Action OnCookingCompleted;

        private List<Ingredient> ingredients = new List<Ingredient>();
        private bool isCooking = false;
        private bool isPaused = false; // Paused when all current ingredients are cooked but waiting for more
        private float totalCookingTime = 0f;
        private float currentCookingTime = 0f;
        private float elapsedTimeBeforePause = 0f; // Track time elapsed before pausing
        private Dictionary<Ingredient, CookingTypeDefinition> ingredientCookingDefs = new Dictionary<Ingredient, CookingTypeDefinition>();
        private Dictionary<Ingredient, float> ingredientStartTimes = new Dictionary<Ingredient, float>();

        public List<Ingredient> GetIngredients() => new List<Ingredient>(ingredients);
        public bool IsCooking => isCooking;

        public float GetCookingProgress()
        {
            if (!isCooking || totalCookingTime <= 0f) return 0f;

            // Calculate elapsed time (including paused time)
            float elapsedTime = isPaused ? elapsedTimeBeforePause : currentCookingTime;

            // Calculate progress based on elapsed time vs total time
            // This shows overall progress of all ingredients
            float progress = Mathf.Clamp01(elapsedTime / totalCookingTime);
            return progress;
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
                // Chef has nothing - cannot pick up from stove (needs a plate)
                Debug.Log("StoveCounter: Cannot pick up - chef needs a plate!");
                return;
            }
            else if (chef.CurrentItem is Plate)
            {
                // Chef has a plate - try to pick up ingredients from stove
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
            // If no ingredients at all, make sure cooking is stopped
            if (ingredients.Count == 0)
            {
                if (isCooking)
                {
                    StopCooking();
                }
                return;
            }

            // Check if there are any uncooked ingredients
            bool hasUncookedIngredients = ingredients.Any(i => i.State != IngredientState.Cooked);
            
            if (hasUncookedIngredients)
            {
                // Resume cooking if it was paused
                if (isPaused)
                {
                    ResumeCooking();
                }

                if (!isCooking)
                {
                    StartCooking();
                }

                UpdateCooking();
            }
            else if (isCooking && !isPaused)
            {
                // All current ingredients are cooked, but keep isCooking true and pause
                PauseCooking();
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

            // Calculate start time for this ingredient
            float startTime = 0f;
            
            if (isCooking)
            {
                // If we're cooking (or paused), new ingredient starts after all currently cooking ingredients finish
                float maxEndTime = 0f;
                float currentElapsed = isPaused ? elapsedTimeBeforePause : currentCookingTime;
                
                foreach (var ing in ingredients)
                {
                    if (ingredientCookingDefs.ContainsKey(ing) && ing.State != IngredientState.Cooked)
                    {
                        float ingStartTime = ingredientStartTimes.ContainsKey(ing) ? ingredientStartTimes[ing] : 0f;
                        float ingDuration = ingredientCookingDefs[ing].duration;
                        float ingEndTime = ingStartTime + ingDuration;
                        maxEndTime = Mathf.Max(maxEndTime, ingEndTime);
                    }
                }
                
                // New ingredient starts at the maximum end time, or current elapsed time if nothing is cooking
                startTime = Mathf.Max(maxEndTime, currentElapsed);
            }
            else
            {
                // Calculate start time for this ingredient (cumulative of all previous uncooked ingredients)
                foreach (var ing in ingredients)
                {
                    if (ingredientCookingDefs.ContainsKey(ing) && ing.State != IngredientState.Cooked)
                    {
                        startTime += ingredientCookingDefs[ing].duration;
                    }
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
            else if (isPaused)
            {
                // Resume cooking when new ingredient is added
                ResumeCooking();
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
            float maxEndTime = 0f;
            
            foreach (var ingredient in ingredients)
            {
                // Only count uncooked ingredients for total cooking time
                if (ingredientCookingDefs.ContainsKey(ingredient) && ingredient.State != IngredientState.Cooked)
                {
                    float startTime = ingredientStartTimes.ContainsKey(ingredient) ? ingredientStartTimes[ingredient] : 0f;
                    float duration = ingredientCookingDefs[ingredient].duration;
                    float endTime = startTime + duration;
                    maxEndTime = Mathf.Max(maxEndTime, endTime);
                }
            }
            
            totalCookingTime = maxEndTime;
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
            isPaused = false;
            currentCookingTime = 0f;
            elapsedTimeBeforePause = 0f;
            RecalculateTotalCookingTime();
            OnCookingStarted?.Invoke();
        }

        private void PauseCooking()
        {
            if (!isCooking) return;
            
            isPaused = true;
            elapsedTimeBeforePause = currentCookingTime;
            // Keep isCooking = true, just pause the timer
        }

        private void ResumeCooking()
        {
            if (!isCooking || !isPaused) return;
            
            isPaused = false;
            // currentCookingTime continues from where it was paused
            // No need to reset it
        }

        private void UpdateCooking()
        {
            // Only update timer if not paused
            if (!isPaused)
            {
                currentCookingTime += Time.deltaTime;
            }

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
                    float elapsedTime = isPaused ? elapsedTimeBeforePause : currentCookingTime;

                    if (elapsedTime >= startTime + duration)
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

            // Get the cooking type from the stored definition
            CookingType cookingType = defaultCookingType;
            if (ingredientCookingDefs.ContainsKey(ingredient))
            {
                cookingType = ingredientCookingDefs[ingredient].cookingType;
            }

            // Call Cook with the appropriate cooking type
            ingredient.Cook(cookingType);
            OnItemCooked?.Invoke(ingredient);

            // Keep ingredient in the list (don't remove it) - it stays in the stove until picked up
            // Just remove from cooking tracking dictionaries
            ingredientCookingDefs.Remove(ingredient);
            ingredientStartTimes.Remove(ingredient);

            // Recalculate total time for remaining uncooked ingredients
            RecalculateTotalCookingTime();
            
            // Check if all ingredients are now cooked - Update() will handle pausing
        }

        private void StopCooking()
        {
            // Only stop cooking when there are no more ingredients or all are cooked and picked up
            bool hasUncookedIngredients = ingredients.Any(i => i.State != IngredientState.Cooked);
            if (hasUncookedIngredients) return; // Don't stop if there are uncooked ingredients
            
            isCooking = false;
            isPaused = false;
            currentCookingTime = 0f;
            elapsedTimeBeforePause = 0f;
            totalCookingTime = 0f;
            OnCookingCompleted?.Invoke();

            // Update UI visibility when cooking stops
            UpdateUIVisibility();
        }

        protected override bool TryPickUpItem()
        {
            Chef chef = FindObjectOfType<Chef>();
            if (chef == null) return false;

            if (ingredients.Count == 0) return false;

            // Check if chef has a plate
            if (chef.CurrentItem == null || !(chef.CurrentItem is Plate plate))
            {
                Debug.Log("StoveCounter: Cannot pick up - chef needs a plate to pick up ingredients from stove!");
                return false;
            }

            // Check if plate is clean
            if (plate.IsDirty)
            {
                Debug.Log("StoveCounter: Cannot pick up - plate must be clean!");
                return false;
            }

            // Check if ALL ingredients are cooked (in target state)
            bool allCooked = ingredients.All(i => i.State == IngredientState.Cooked);

            if (!allCooked)
            {
                // Some ingredients are still cooking, cannot pick up yet
                Debug.Log("StoveCounter: Cannot pick up - some ingredients are still cooking!");
                return false;
            }

            // Get LevelData for plate validation
            LevelController levelController = FindObjectOfType<LevelController>();
            LevelData levelData = levelController != null ? levelController.CurrentLevelData : null;

            if (levelData == null)
            {
                Debug.LogWarning("StoveCounter: Cannot find LevelData!");
                return false;
            }

            // Transfer all cooked ingredients to the plate
            // Only transfer ingredients that are fully cooked
            List<Ingredient> ingredientsToTransfer = ingredients.Where(i => i.State == IngredientState.Cooked).ToList();
            
            if (ingredientsToTransfer.Count == 0)
            {
                Debug.Log("StoveCounter: No cooked ingredients to transfer!");
                return false;
            }

            // Ensure ALL ingredients in stove are cooked before transferring
            if (ingredientsToTransfer.Count != ingredients.Count)
            {
                Debug.Log("StoveCounter: Cannot pick up - not all ingredients are cooked yet!");
                return false;
            }

            bool allTransferred = true;

            foreach (var ingredient in ingredientsToTransfer)
            {
                if (plate.TryAddIngredient(ingredient, levelData))
                {
                    // Ingredient is now parented to plate and will move with it
                    ingredients.Remove(ingredient);
                    ingredientCookingDefs.Remove(ingredient);
                    ingredientStartTimes.Remove(ingredient);
                }
                else
                {
                    Debug.Log($"StoveCounter: Could not add {ingredient.data?.name} to plate - doesn't match recipe requirements");
                    allTransferred = false;
                }
            }

            // If all ingredients were transferred, update stove state
            if (ingredients.Count == 0)
            {
                StopCooking();
            }
            else
            {
                // Recalculate if still has ingredients
                RecalculateTotalCookingTime();
                AdjustStartTimes();
                
                bool hasUncookedIngredients = ingredients.Any(i => i.State != IngredientState.Cooked);
                if (!hasUncookedIngredients)
                {
                    StopCooking();
                }
            }

            // Update UI visibility
            UpdateUIVisibility();

            return allTransferred && ingredientsToTransfer.Count > 0;
        }

        protected override bool CanProcess(IItem item)
        {
            // Stove processes automatically when ingredients are placed
            return false;
        }
    }
}