using System;
using UnityEngine;

namespace TinyChef
{
    public class WasteBinCounter : BaseCounter
    {
        public Action<IItem> OnItemDisposed;

        private void Awake()
        {
            counterType = CounterType.WasteBin;
            if (itemPlacePoint == null)
            {
                itemPlacePoint = transform;
            }
        }

        protected override bool CanPlaceItem(IItem item)
        {
            // Waste bin accepts any item
            return item != null;
        }

        protected override bool TryPutDownItem()
        {
            Chef chef = FindObjectOfType<Chef>();
            if (chef == null || chef.CurrentItem == null)
            {
                return false;
            }

            IItem item = chef.CurrentItem;
            
            Debug.Log($"WasteBin: Disposing item: {item.gameObject.name}");
            
            // Drop the item from chef's hands
            chef.DropItem();
            
            // Destroy the item immediately
            OnItemDisposed?.Invoke(item);
            
            // If it's a plate with ingredients, destroy ingredients too
            Plate plate = item.gameObject.GetComponent<Plate>();
            if (plate != null)
            {
                foreach (var ingredient in plate.Ingredients)
                {
                    if (ingredient != null)
                    {
                        Destroy(ingredient.gameObject);
                    }
                }
            }
            
            Destroy(item.gameObject);
            
            Debug.Log("WasteBin: Item destroyed");
            return true;
        }

        protected override bool TryPickUpItem()
        {
            // Can't pick up from waste bin
            return false;
        }

        protected override bool CanProcess(IItem item)
        {
            // Can't process at waste bin
            return false;
        }
    }
}

