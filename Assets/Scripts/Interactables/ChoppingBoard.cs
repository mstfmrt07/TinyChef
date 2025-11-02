using System;
using UnityEngine;

namespace TinyChef
{
    public class ChoppingBoard : MonoBehaviour, IInteractable
    {
        public HighlightObject highlightObject;
        public Transform spawnPoint;

        public Action<Ingredient> OnItemProcessed;

        private Ingredient currentIngredient;

        public void Select()
        {
            highlightObject.Select();
        }

        public void Deselect()
        {
            highlightObject.Deselect();
        }

        public void Interact()
        {
            if (currentIngredient != null)
            {
                ProcessItem();
            }
            else
            {
                PlaceItem();
            }
        }

        private void PlaceItem()
        {
            var ingredient = FindObjectOfType<Chef>().CurrentIngredient;
            if (ingredient == null)
                return;

            currentIngredient = ingredient;
            FindObjectOfType<Chef>().DropItem();
            currentIngredient.transform.SetParent(spawnPoint);
            currentIngredient.transform.localPosition = Vector3.zero;
        }

        private void ProcessItem()
        {
            Debug.Log("Item Processed");
            currentIngredient.Process();
            OnItemProcessed?.Invoke(currentIngredient);
        }
    }
}