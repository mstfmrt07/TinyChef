using UnityEngine;

namespace TinyChef
{
    public class ServingStation : BaseCounter
    {
        [Header("Plate Settings")]
        public Plate cleanPlatePrefab;

        private void Awake()
        {
            counterType = CounterType.ServingStation;
            if (itemPlacePoint == null)
            {
                itemPlacePoint = transform;
            }
        }

        protected override bool CanPlaceItem(IItem item)
        {
            // Only non-empty, clean plates can be served
            if (!IsPlate(item)) return false;

            Plate plate = item.gameObject.GetComponent<Plate>();
            if (plate != null)
            {
                // Plate must not be empty and must not be dirty
                return !plate.IsEmpty && !plate.IsDirty;
            }

            return false;
        }

        protected override bool TryPutDownItem()
        {
            Chef chef = FindObjectOfType<Chef>();
            if (chef == null || chef.CurrentItem == null)
            {
                Debug.Log("ServingStation: Chef or CurrentItem is null");
                return false;
            }

            IItem item = chef.CurrentItem;
            
            Debug.Log($"ServingStation: Attempting to serve item: {item.gameObject.name}");
            
            if (!CanPlaceItem(item))
            {
                Plate plate = item.gameObject.GetComponent<Plate>();
                if (plate != null)
                {
                    Debug.Log($"ServingStation: Cannot place - IsEmpty: {plate.IsEmpty}, IsDirty: {plate.IsDirty}, IngredientCount: {plate.IngredientCount}");
                }
                else
                {
                    Debug.Log("ServingStation: Item is not a plate");
                }
                return false;
            }
            
            // Try to serve the dish
            if (ReferenceManager.Instance.OrderManager != null)
            {
                Debug.Log("ServingStation: Calling TryServeOrder");
                bool success = ReferenceManager.Instance.OrderManager.TryServeOrder(item);
                
                if (success)
                {
                    Debug.Log("ServingStation: Order served successfully!");
                    chef.DropItem();
                    
                    Plate plate = item.gameObject.GetComponent<Plate>();
                    if (plate != null)
                    {
                        // Destroy all ingredients
                        foreach (var ingredient in plate.Ingredients)
                        {
                            if (ingredient != null)
                            {
                                Destroy(ingredient.gameObject);
                            }
                        }
                        
                        // Destroy the used plate
                        Destroy(plate.gameObject);
                    }
                    
                    // Check if there's a dishwasher in the level
                    LevelController levelController = ReferenceManager.Instance.LevelController;
                    bool hasDishwasher = levelController != null && levelController.HasDishwasher;
                    
                    // Always create a new plate
                    if (cleanPlatePrefab != null)
                    {
                        Plate newPlate = Instantiate(cleanPlatePrefab, itemPlacePoint != null ? itemPlacePoint : transform);
                        newPlate.transform.localPosition = Vector3.zero;
                        newPlate.transform.localRotation = Quaternion.identity;
                        
                        if (hasDishwasher)
                        {
                            // Set new plate as dirty if dishwasher exists
                            newPlate.SetClean(false);
                            Debug.Log("ServingStation: Created new dirty plate (dishwasher available)");
                        }
                        else
                        {
                            // Plate is already clean by default
                            Debug.Log("ServingStation: Created new clean plate");
                        }
                        
                        currentItem = newPlate;
                    }
                    else
                    {
                        Debug.LogWarning("ServingStation: No clean plate prefab assigned!");
                        currentItem = null;
                    }
                    
                    return true;
                }
                else
                {
                    Debug.Log("ServingStation: Wrong order or no matching order!");
                    return false;
                }
            }
            else
            {
                Debug.LogError("ServingStation: OrderManager is null!");
                // Fallback: just place the item
                return base.TryPutDownItem();
            }
        }

        protected override bool CanProcess(IItem item)
        {
            return false; // Can't process at serving station
        }
    }
}
