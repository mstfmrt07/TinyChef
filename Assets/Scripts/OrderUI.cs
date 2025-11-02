using System;
using UnityEngine;
using UnityEngine.UI;

namespace TinyChef
{
    public class OrderUI : MonoBehaviour
    {
        //public Image orderImage;
        public Text orderText;
        public Image timerImage;
        public Image ingredientPrefab;
        public Transform ingredientContainer;

        private Order order;
        private float orderTimer = 0.0f;

        public void SetOrder(Order order)
        {
            this.order = order;
            //orderImage.sprite = order.recipe.icon;
            orderText.text = order.recipe.name;
            foreach (var ingredientData in order.recipe.ingredients)
            {
                var ingredient = Instantiate(ingredientPrefab, ingredientContainer);
                ingredient.sprite = ingredientData.item.icon;
            }
        }

        private void Update()
        {
            if (order == null || order.State == OrderState.Idle)
                return;

            orderTimer += Time.deltaTime;
            timerImage.fillAmount = Mathf.Clamp01(orderTimer / order.duration);
        }
    }
}