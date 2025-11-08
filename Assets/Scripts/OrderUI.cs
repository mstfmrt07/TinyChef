using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TinyChef
{
    public class OrderUI : MonoBehaviour
    {
        public Image orderImage;
        public TextMeshProUGUI orderText;
        public Image timerImage;
        public Image ingredientPrefab;
        public Transform ingredientContainer;

        private Order order;

        public void SetOrder(Order order)
        {
            this.order = order;
            if (orderImage != null && order.recipe.icon != null)
            {
                orderImage.sprite = order.recipe.icon;
            }
            if (orderText != null)
            {
                orderText.text = order.recipe.name;
            }
            
            // Clear existing ingredients
            if (ingredientContainer != null)
            {
                foreach (Transform child in ingredientContainer)
                {
                    Destroy(child.gameObject);
                }

                // Create UI for each required ingredient
                if (ingredientPrefab != null && order.recipe.requiredIngredients != null)
                {
                    foreach (var ingredientDef in order.recipe.requiredIngredients)
                    {
                        if (ingredientDef.item != null && ingredientDef.item.icon != null)
                        {
                            Image ingredient = Instantiate(ingredientPrefab, ingredientContainer);
                            ingredient.sprite = ingredientDef.item.icon;
                        }
                    }
                }
            }
        }

        private void Update()
        {
            if (order == null || order.State == OrderState.Idle || timerImage == null)
                return;

            // Update timer fill amount
            float fillAmount = order.TimeRemaining / order.timeLimit;
            timerImage.fillAmount = Mathf.Clamp01(fillAmount);

            // Optionally change color based on time remaining
            if (fillAmount < 0.3f)
            {
                timerImage.color = Color.red;
            }
            else if (fillAmount < 0.6f)
            {
                timerImage.color = Color.yellow;
            }
            else
            {
                timerImage.color = Color.green;
            }
        }
    }
}