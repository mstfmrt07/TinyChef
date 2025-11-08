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

            // Special case: if counter has a plate, we can always try to add ingredients to it
            if (currentItem != null)
            {
                Plate plate = currentItem.gameObject.GetComponent<Plate>();
                if (plate != null)
                {
                    // Allow trying to add ingredient to plate (validation happens in TryPutDownItem)
                    // But only if the item being placed is an Ingredient
                    return item is Ingredient;
                }
            }

            // Check counter-specific rules
            switch (counterType)
            {
                case CounterType.Dishwasher:
                    // Only plates can be placed in dishwasher
                    return IsPlate(item);

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

            // Check if counter has a plate - if so, try to add ingredient to plate
            if (currentItem != null)
            {
                Plate plate = currentItem.gameObject.GetComponent<Plate>();
                if (plate != null && item is Ingredient ingredient)
                {
                    // Try to add ingredient to plate
                    LevelController levelController = FindObjectOfType<LevelController>();
                    LevelData levelData = levelController != null ? levelController.CurrentLevelData : null;

                    if (levelData != null && plate.TryAddIngredient(ingredient, levelData))
                    {
                        chef.DropItem();
                        // Ingredient is now parented to plate, so we don't need to manage it here
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
            StartCoroutine(ProcessCoroutine());
        }

        protected virtual System.Collections.IEnumerator ProcessCoroutine()
        {
            while (processProgress < 1f && currentItem != null)
            {
                processProgress += Time.deltaTime / processTime;
                yield return null;
            }

            if (currentItem != null)
            {
                ExecuteProcess();
            }

            isProcessing = false;
            processProgress = 0f;
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