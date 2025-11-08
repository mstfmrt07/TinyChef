using System;
using UnityEngine;

namespace TinyChef
{
    public class DishwasherCounter : BaseCounter
    {
        public Action<Plate> OnPlateWashed;

        private void Awake()
        {
            counterType = CounterType.Dishwasher;
            if (itemPlacePoint == null)
            {
                itemPlacePoint = transform;
            }
        }

        private void Start()
        {
            // Initialize processing UI if present
            if (processingUI != null)
            {
                processingUI.Initialize(this);
            }
        }

        protected override bool CanProcess(IItem item)
        {
            if (!IsPlate(item)) return false;
            
            Plate plate = item.gameObject.GetComponent<Plate>();
            if (plate == null) return false;
            
            // Can only wash dirty plates
            return plate.IsDirty;
        }

        protected override void ExecuteProcess()
        {
            if (currentItem != null && IsPlate(currentItem))
            {
                Plate plate = currentItem.gameObject.GetComponent<Plate>();
                if (plate != null && plate.IsDirty)
                {
                    plate.Clean();
                    Debug.Log("Plate washed in dishwasher!");
                    OnPlateWashed?.Invoke(plate);
                }
            }
        }
    }
}
