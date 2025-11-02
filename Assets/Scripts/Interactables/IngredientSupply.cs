using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace TinyChef
{
    public class IngredientSupply : MonoBehaviour, IInteractable
    {
        public IngredientData ingredientData;
        public SpriteRenderer ingredientImage;
        public HighlightObject highlightObject;
        public Transform spawnPoint;

        public Action<Ingredient> OnItemSupplied;

        private Ingredient currentSupply;

        private void Awake()
        {
            Initialize();
        }

        void Initialize()
        {
            ingredientImage.sprite = ingredientData.icon;
        }

        public void Interact()
        {
            if (FindObjectOfType<Chef>().CurrentIngredient == null)
            {
                Supply();
            }
            else
            {
                PlaceItem();
            }
        }

        private void Supply()
        {
            if (currentSupply == null)
            {
                currentSupply = Instantiate(ingredientData.prefab, spawnPoint);
                Debug.Log($"Item Supplied: {currentSupply.data.name}");
            }

            OnItemSupplied?.Invoke(currentSupply);
            FindObjectOfType<Chef>().GrabItem(currentSupply);
            currentSupply = null;
        }

        private void PlaceItem()
        {
            var ingredient = FindObjectOfType<Chef>().CurrentIngredient;
            if (ingredient == null)
                return;

            currentSupply = ingredient;
            FindObjectOfType<Chef>().DropItem();
            currentSupply.transform.SetParent(spawnPoint);
            currentSupply.transform.localPosition = Vector3.zero;
        }

        public void Select()
        {
            highlightObject.Select();
        }

        public void Deselect()
        {
            highlightObject.Deselect();
        }
    }
}