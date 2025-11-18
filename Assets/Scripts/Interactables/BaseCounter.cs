using System;
using UnityEngine;

namespace TinyChef
{
    public abstract class BaseCounter : MonoBehaviour, IInteractable
    {
        [Header("Counter Settings")] public CounterType counterType;
        public HighlightObject highlightObject;
        public Transform itemPlacePoint;

        [Header("Processing Settings")] public float processTime = 2f;
        public bool isProcessing = false;
        public float processProgress = 0f;
        public ProcessingUI processingUI;

        protected IItem currentItem;
        public IItem CurrentItem => currentItem;

        public virtual void Select()
        {
            highlightObject?.Select();
        }

        public virtual void Deselect()
        {
            highlightObject?.Deselect();
        }

        public virtual void Interact()
        {
            // Short press - pick up or put down
            if (currentItem != null)
            {
                // Counter has item - try to pick it up
                // If picking up fails, try to put down item instead
                if (!TryPickUpItem())
                {
                    TryPutDownItem();
                }
            }
            else
            {
                // Counter is empty - try to put down what chef is holding
                TryPutDownItem();
            }
        }

        public virtual void Process()
        {
            // Long press - process the item
            if (currentItem != null && CanProcess(currentItem))
            {
                StartProcessing();
            }
        }

        protected virtual bool CanPlaceItem(IItem item)
        {
            if (item == null) return false;

            // Check if item is a dirty plate - only dishwasher can accept dirty plates
            if (IsPlate(item))
            {
                Plate plate = item.gameObject.GetComponent<Plate>();
                if (plate != null && plate.IsDirty && counterType != CounterType.Dishwasher)
                {
                    return false; // Dirty plates can only go to dishwasher
                }
            }

            // Special case: if counter has a plate, check what we're trying to place
            if (currentItem != null)
            {
                Plate existingPlate = currentItem.gameObject.GetComponent<Plate>();
                if (existingPlate != null)
                {
                    // If trying to place another plate, check if stacking is allowed
                    if (IsPlate(item))
                    {
                        Plate incomingPlate = item.gameObject.GetComponent<Plate>();
                        return existingPlate.CanStackPlate(incomingPlate);
                    }

                    // Allow trying to add ingredient to plate (validation happens in TryPutDownItem)
                    // But only if the item being placed is an Ingredient
                    return item is Ingredient;
                }
                else
                {
                    return false; // Can't place anything on non-plate items
                }
            }

            // Check counter-specific rules
            switch (counterType)
            {
                case CounterType.Dishwasher:
                    // Only plates can be placed in dishwasher
                    if (!IsPlate(item)) return false;

                    // Dishwasher accepts both clean and dirty plates
                    return true;

                case CounterType.Stove:
                    // Only non-cooked ingredients can be placed (can't re-cook cooked items)
                    if (item is Ingredient ingredient)
                    {
                        return ingredient.State != IngredientState.Cooked;
                    }

                    return false;

                case CounterType.CuttingBoard:
                    // Can't place already processed/cooked items
                    if (item is Ingredient ingredient2)
                    {
                        return ingredient2.State == IngredientState.Raw;
                    }

                    return false;

                case CounterType.ServingStation:
                    // Only non-empty plates can be served
                    if (IsPlate(item))
                    {
                        Plate plate = item.gameObject.GetComponent<Plate>();
                        return plate != null && !plate.IsEmpty;
                    }

                    return false;

                case CounterType.Basic:
                case CounterType.IngredientSupply:
                    return true;

                default:
                    return true;
            }
        }

        protected virtual bool CanProcess(IItem item)
        {
            if (item == null) return false;

            switch (counterType)
            {
                case CounterType.CuttingBoard:
                    // Can only process raw ingredients
                    if (item is Ingredient ingredient)
                    {
                        return ingredient.State == IngredientState.Raw;
                    }

                    return false;

                case CounterType.Stove:
                    // Can cook processed or raw ingredients (but not already cooked)
                    if (item is Ingredient ingredient2)
                    {
                        return ingredient2.State != IngredientState.Cooked;
                    }

                    return false;

                case CounterType.Dishwasher:
                    // Can wash plates (assuming plates can be dirty)
                    return IsPlate(item);

                default:
                    return false;
            }
        }

        protected virtual bool TryPickUpItem()
        {
            Chef chef = FindObjectOfType<Chef>();
            if (chef == null || chef.CurrentItem != null) return false;

            if (currentItem != null)
            {
                // Check if current item is a plate with a stack
                Plate plate = currentItem.gameObject.GetComponent<Plate>();
                if (plate != null && plate.HasStack)
                {
                    // Take the top plate from the stack instead
                    Plate topPlate = plate.TakeTopPlate();
                    if (topPlate != null)
                    {
                        chef.GrabItem(topPlate);
                        return true;
                    }
                }

                // Normal pickup
                chef.GrabItem(currentItem);
                currentItem = null;
                return true;
            }

            return false;
        }

        protected virtual bool TryPutDownItem()
        {
            Chef chef = FindObjectOfType<Chef>();
            if (chef == null || chef.CurrentItem == null) return false;

            IItem item = chef.CurrentItem;

            // Check if counter has a plate - if so, try to add ingredient or stack plate
            if (currentItem != null)
            {
                Plate existingPlate = currentItem.gameObject.GetComponent<Plate>();
                if (existingPlate != null)
                {
                    // Check if trying to stack another plate
                    if (IsPlate(item))
                    {
                        Plate incomingPlate = item.gameObject.GetComponent<Plate>();
                        if (incomingPlate != null && existingPlate.TryStackPlate(incomingPlate))
                        {
                            chef.DropItem();
                            // Plate is now stacked, so we don't need to manage it separately
                            return true;
                        }

                        return false;
                    }

                    // Try to add ingredient to plate
                    if (item is Ingredient ingredient)
                    {
                        LevelController levelController = ReferenceManager.Instance.LevelController;
                        LevelData levelData = levelController != null ? levelController.CurrentLevelData : null;

                        if (levelData != null && existingPlate.TryAddIngredient(ingredient, levelData))
                        {
                            chef.DropItem();
                            // Ingredient is now parented to plate, currentItem remains the plate
                            return true;
                        }
                        else
                        {
                            if (ingredient.data != null)
                            {
                                Debug.Log($"Cannot add {ingredient.data.name} to plate - doesn't match any recipe requirement");
                            }

                            return false;
                        }
                    }
                }

                // Check if currentItem is an ingredient and we're placing a plate on it
                if (currentItem is Ingredient existingIngredient && IsPlate(item))
                {
                    Plate incomingPlate = item.gameObject.GetComponent<Plate>();
                    if (incomingPlate != null)
                    {
                        LevelController levelController = ReferenceManager.Instance.LevelController;
                        LevelData levelData = levelController != null ? levelController.CurrentLevelData : null;

                        if (levelData != null && incomingPlate.TryAddIngredient(existingIngredient, levelData))
                        {
                            chef.DropItem();
                            // Ingredient is now on plate, set plate as currentItem
                            currentItem = incomingPlate;
                            if (itemPlacePoint != null)
                            {
                                incomingPlate.transform.SetParent(itemPlacePoint);
                                incomingPlate.transform.localPosition = Vector3.zero;
                            }

                            return true;
                        }
                        else
                        {
                            Debug.Log($"Cannot add ingredient to plate - doesn't match any recipe requirement");
                            return false;
                        }
                    }
                }
            }

            // Normal placement logic
            if (CanPlaceItem(item))
            {
                chef.DropItem();
                currentItem = item;
                if (itemPlacePoint != null)
                {
                    currentItem.transform.SetParent(itemPlacePoint);
                    currentItem.transform.localPosition = Vector3.zero;
                }

                return true;
            }
            else
            {
                if (item is Ingredient ingredient2 && ingredient2.data != null)
                {
                    Debug.Log($"Cannot place {ingredient2.data.name} on {counterType}");
                }

                return false;
            }
        }

        protected virtual void StartProcessing()
        {
            if (isProcessing) return;
            isProcessing = true;
            processProgress = 0f;

            // Show processing UI
            if (processingUI != null)
            {
                processingUI.SetVisible(true);
                processingUI.UpdateProgress(0f);
            }

            StartCoroutine(ProcessCoroutine());
        }

        protected virtual System.Collections.IEnumerator ProcessCoroutine()
        {
            while (processProgress < 1f && currentItem != null)
            {
                processProgress += Time.deltaTime / processTime;

                // Update processing UI
                if (processingUI != null)
                {
                    processingUI.UpdateProgress(processProgress);
                }

                yield return null;
            }

            if (currentItem != null)
            {
                ExecuteProcess();
            }

            isProcessing = false;
            processProgress = 0f;

            // Hide processing UI
            if (processingUI != null)
            {
                processingUI.SetVisible(false);
            }
        }

        protected virtual void ExecuteProcess()
        {
        }

        protected virtual bool IsPlate(IItem item)
        {
            if (item == null) return false;

            // Check if item has a Plate component
            Plate plate = item.gameObject.GetComponent<Plate>();

            return plate != null;
        }
    }
}