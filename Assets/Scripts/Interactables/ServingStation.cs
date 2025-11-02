using UnityEngine;

namespace TinyChef
{
    public class ServingStation : BaseCounter
    {
        private OrderManager orderManager;

        private void Awake()
        {
            counterType = CounterType.ServingStation;
            if (itemPlacePoint == null)
            {
                itemPlacePoint = transform;
            }
            orderManager = FindObjectOfType<OrderManager>();
        }

        protected override bool CanPlaceItem(Ingredient ingredient)
        {
            // Only plates can be served
            return IsPlate(ingredient);
        }

        protected override void TryPutDownItem()
        {
            Chef chef = FindObjectOfType<Chef>();
            if (chef == null || chef.CurrentIngredient == null) return;

            Ingredient ingredient = chef.CurrentIngredient;
            
            if (CanPlaceItem(ingredient))
            {
                // Try to serve the dish
                if (orderManager != null)
                {
                    bool success = orderManager.TryServeOrder(ingredient);
                    if (success)
                    {
                        chef.DropItem();
                        // Plate is consumed when served successfully
                        Destroy(ingredient.gameObject);
                    }
                    else
                    {
                        Debug.Log("Wrong order or no matching order!");
                    }
                }
                else
                {
                    // Fallback: just place the item
                    base.TryPutDownItem();
                }
            }
        }

        protected override bool CanProcess(Ingredient ingredient)
        {
            return false; // Can't process at serving station
        }
    }
}
