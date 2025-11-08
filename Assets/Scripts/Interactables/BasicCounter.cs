using UnityEngine;

namespace TinyChef
{
    /// <summary>
    /// A basic counter that can hold items but doesn't process them
    /// </summary>
    public class BasicCounter : BaseCounter
    {
        [Header("Plate Settings")]
        public bool startWithPlate = false;
        public Plate platePrefab;

        private void Awake()
        {
            counterType = CounterType.Basic;
            if (itemPlacePoint == null)
            {
                itemPlacePoint = transform;
            }
        }

        private void Start()
        {
            // Initialize with plate if requested
            if (startWithPlate && platePrefab != null)
            {
                InitializePlate();
            }
        }

        private void InitializePlate()
        {
            if (currentItem != null)
            {
                // Counter already has an item, don't initialize plate
                return;
            }

            // Instantiate plate prefab
            Plate plateInstance = Instantiate(platePrefab, itemPlacePoint != null ? itemPlacePoint : transform);
            plateInstance.transform.localPosition = Vector3.zero;
            plateInstance.transform.localRotation = Quaternion.identity;

            // Plate implements IItem, so we can assign it directly
            currentItem = plateInstance;
        }

        protected override bool CanProcess(IItem item)
        {
            // Basic counters can't process items
            return false;
        }
    }
}
