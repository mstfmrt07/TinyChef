using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace TinyChef
{
    public class IngredientSupply : BaseCounter
    {
        public IngredientData ingredientData;
        public SpriteRenderer ingredientImage;

        public Action<Ingredient> OnItemSupplied;

        private void Awake()
        {
            counterType = CounterType.IngredientSupply;
            if (itemPlacePoint == null)
            {
                itemPlacePoint = transform;
            }
            Initialize();
        }

        void Initialize()
        {
            if (ingredientImage != null && ingredientData != null)
            {
                ingredientImage.sprite = ingredientData.icon;
            }
        }

        public override void Interact()
        {
            Chef chef = FindObjectOfType<Chef>();
            if (chef == null) return;

            if (chef.CurrentItem == null)
            {
                Supply(chef);
            }
            else
            {
                TryPutDownItem();
            }
        }

        private void Supply(Chef chef)
        {
            if (ingredientData == null || ingredientData.prefab == null) return;

            Ingredient newIngredient = Instantiate(ingredientData.prefab, itemPlacePoint != null ? itemPlacePoint : transform);
            newIngredient.transform.localPosition = Vector3.zero;
            
            Debug.Log($"Item Supplied: {newIngredient.data.name}");
            OnItemSupplied?.Invoke(newIngredient);
            chef.GrabItem(newIngredient);
        }

        protected override bool CanPlaceItem(IItem item)
        {
            // Can place items back on supply counter
            return true;
        }
    }
}