using System.Collections.Generic;
using UnityEngine;

namespace TinyChef
{
    /// <summary>
    /// Component for plates that can hold multiple ingredients
    /// </summary>
    public class Plate : MonoBehaviour
    {
        private List<Ingredient> ingredients = new List<Ingredient>();

        public List<Ingredient> Ingredients => new List<Ingredient>(ingredients);
        public int IngredientCount => ingredients.Count;
        public bool IsEmpty => ingredients.Count == 0;

        public void AddIngredient(Ingredient ingredient)
        {
            if (ingredient != null && !ingredients.Contains(ingredient))
            {
                ingredients.Add(ingredient);
                // Attach ingredient to plate
                ingredient.transform.SetParent(transform);
                ingredient.transform.localPosition = Vector3.up * (ingredients.Count - 1) * 0.1f;
            }
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
