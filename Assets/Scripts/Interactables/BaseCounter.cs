using System;
using UnityEngine;

namespace TinyChef
{
    public abstract class BaseCounter : MonoBehaviour, IInteractable
    {
        [Header("Counter Settings")]
        public CounterType counterType;
        public HighlightObject highlightObject;
        public Transform itemPlacePoint;
        
        [Header("Processing Settings")]
        public float processTime = 2f;
        public bool isProcessing = false;
        public float processProgress = 0f;

        protected Ingredient currentItem;
        public Ingredient CurrentItem => currentItem;

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

        protected virtual bool CanPlaceItem(Ingredient ingredient)
        {
            if (ingredient == null) return false;

            // Check counter-specific rules
            switch (counterType)
            {
                case CounterType.Dishwasher:
                    // Only plates can be placed in dishwasher
                    return IsPlate(ingredient);

                case CounterType.Stove:
                    // Only non-cooked items can be placed (can't re-cook cooked items)
                    return ingredient.State != IngredientState.Cooked;

                case CounterType.CuttingBoard:
                    // Can't place already processed/cooked items
                    return ingredient.State == IngredientState.Raw;

                case CounterType.ServingStation:
                    // Only plates with items can be served
                    return IsPlate(ingredient);

                case CounterType.Basic:
                case CounterType.IngredientSupply:
                    return true;

                default:
                    return true;
            }
        }

        protected virtual bool CanProcess(Ingredient ingredient)
        {
            if (ingredient == null) return false;

            switch (counterType)
            {
                case CounterType.CuttingBoard:
                    // Can only process raw ingredients
                    return ingredient.State == IngredientState.Raw;

                case CounterType.Stove:
                    // Can cook processed or raw ingredients (but not already cooked)
                    return ingredient.State != IngredientState.Cooked;

                case CounterType.Dishwasher:
                    // Can wash plates (assuming plates can be dirty)
                    return IsPlate(ingredient);

                default:
                    return false;
            }
        }

        protected virtual bool TryPickUpItem()
        {
            Chef chef = FindObjectOfType<Chef>();
            if (chef == null || chef.CurrentIngredient != null) return false;

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
            if (chef == null || chef.CurrentIngredient == null) return false;

            Ingredient ingredient = chef.CurrentIngredient;
            
            if (CanPlaceItem(ingredient))
            {
                chef.DropItem();
                currentItem = ingredient;
                if (itemPlacePoint != null)
                {
                    currentItem.transform.SetParent(itemPlacePoint);
                    currentItem.transform.localPosition = Vector3.zero;
                }
                return true;
            }
            else
            {
                Debug.Log($"Cannot place {ingredient.data.name} on {counterType}");
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
            if (currentItem == null) return;

            switch (counterType)
            {
                case CounterType.CuttingBoard:
                    currentItem.Process();
                    break;

                case CounterType.Stove:
                    // Default cooking type, can be overridden in StoveCounter
                    currentItem.Cook(CookingType.Boiled);
                    break;

                case CounterType.Dishwasher:
                    // Wash plate logic (you may need to add a Wash method to Ingredient)
                    Debug.Log("Plate washed");
                    break;
            }
        }

        protected virtual bool IsPlate(Ingredient ingredient)
        {
            if (ingredient == null) return false;
            
            // Check if ingredient has a Plate component
            Plate plate = ingredient.GetComponent<Plate>();
            if (plate != null) return true;
            
            // Fallback: check name
            return ingredient.data.name.ToLower().Contains("plate");
        }
    }
}
