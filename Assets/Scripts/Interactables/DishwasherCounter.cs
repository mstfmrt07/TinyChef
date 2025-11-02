using UnityEngine;

namespace TinyChef
{
    public class DishwasherCounter : BaseCounter
    {
        private void Awake()
        {
            counterType = CounterType.Dishwasher;
            if (itemPlacePoint == null)
            {
                itemPlacePoint = transform;
            }
        }

        protected override void ExecuteProcess()
        {
            // Wash the plate (you may want to add a "clean" state" or similar)
            Debug.Log("Plate washed in dishwasher");
            // Optionally destroy the plate or mark it as clean
        }
    }
}
