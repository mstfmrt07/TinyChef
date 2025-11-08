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

        protected override bool CanPlaceItem(IItem item)
        {
            // Only non-empty plates can be served
            if (!IsPlate(item)) return false;

            Plate plate = item.gameObject.GetComponent<Plate>();
            if (plate != null)
            {
                return !plate.IsEmpty;
            }

            return false;
        }

        protected override bool TryPutDownItem()
        {
            Chef chef = FindObjectOfType<Chef>();
            if (chef == null || chef.CurrentItem == null) return false;

            IItem item = chef.CurrentItem;
            
            if (CanPlaceItem(item))
            {
                // Try to serve the dish
                if (orderManager != null)
                {
                    bool success = orderManager.TryServeOrder(item);
                    if (success)
                    {
                        chef.DropItem();
                        // Plate is consumed when served successfully
                        Destroy(item.gameObject);
                        return true;
                    }
                    else
                    {
                        Debug.Log("Wrong order or no matching order!");
                        return false;
                    }
                }
                else
                {
                    // Fallback: just place the item
                    return base.TryPutDownItem();
                }
            }

            return false;
        }

        protected override bool CanProcess(IItem item)
        {
            return false; // Can't process at serving station
        }
    }
}
