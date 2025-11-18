using System;
using UnityEngine;

namespace TinyChef
{
    public class DishwasherCounter : BaseCounter
    {
        [Header("Dishwasher Settings")]
        public Transform cleanPlatePoint;
        public Transform dirtyPlatePoint;

        public Action<Plate> OnPlateWashed;

        private Plate cleanPlateStack; // Bottom plate of clean stack
        private Plate dirtyPlateStack; // Bottom plate of dirty stack (all picked up/dropped together)

        private void Awake()
        {
            counterType = CounterType.Dishwasher;
            
            // Use separate placement points if available
            if (cleanPlatePoint == null)
            {
                cleanPlatePoint = itemPlacePoint;
            }
            
            if (dirtyPlatePoint == null)
            {
                dirtyPlatePoint = itemPlacePoint;
            }
        }

        private void Start()
        {
            // Initialize processing UI if present
            if (processingUI != null)
            {
                processingUI.Initialize(this);
            }
            
            UpdateCurrentItem();
        }

        /// <summary>
        /// Updates currentItem to reflect what's on the counter (prioritizes dirty plates for processing)
        /// </summary>
        private void UpdateCurrentItem()
        {
            // Priority: dirty plates (can be processed)
            if (dirtyPlateStack != null)
            {
                currentItem = dirtyPlateStack;
            }
            // Fallback: clean plates (cannot be processed, but can be picked up)
            else if (cleanPlateStack != null)
            {
                currentItem = cleanPlateStack;
            }
            else
            {
                currentItem = null;
            }
        }

        public override void Interact()
        {
            Chef chef = FindObjectOfType<Chef>();
            if (chef == null) return;

            // Check what chef is holding
            bool chefHasItem = chef.CurrentItem != null;
            bool chefHasDirtyPlate = false;
            bool chefHasCleanPlate = false;

            if (chefHasItem && IsPlate(chef.CurrentItem))
            {
                Plate plate = chef.CurrentItem.gameObject.GetComponent<Plate>();
                if (plate != null)
                {
                    chefHasDirtyPlate = plate.IsDirty;
                    chefHasCleanPlate = !plate.IsDirty;
                }
            }

            // Priority 1: Put down dirty plates if chef has them
            if (chefHasDirtyPlate)
            {
                if (TryPutDownDirtyPlates())
                {
                    UpdateCurrentItem();
                    return;
                }
            }

            // Priority 2: Pick up clean plate (only top one from stack) if chef has empty hands
            if (!chefHasItem && cleanPlateStack != null)
            {
                if (TryPickUpCleanPlate())
                {
                    UpdateCurrentItem();
                    return;
                }
            }

            // Reject clean plates - they don't belong in dishwasher
            if (chefHasCleanPlate)
            {
                Debug.Log("DishwasherCounter: Cannot place clean plates here!");
                return;
            }

            // Otherwise use base counter interaction (shouldn't normally happen)
            base.Interact();
        }

        private bool TryPickUpCleanPlate()
        {
            Chef chef = FindObjectOfType<Chef>();
            if (chef == null || chef.CurrentItem != null) return false;
            if (cleanPlateStack == null) return false;

            // Pick up only the top clean plate
            Plate plateToPickUp;
            
            if (cleanPlateStack.HasStack)
            {
                // Take the top plate from the stack
                plateToPickUp = cleanPlateStack.TakeTopPlate();
            }
            else
            {
                // Take the only plate
                plateToPickUp = cleanPlateStack;
                cleanPlateStack = null;
            }

            if (plateToPickUp != null)
            {
                chef.GrabItem(plateToPickUp);
                Debug.Log("DishwasherCounter: Picked up clean plate");
                return true;
            }

            return false;
        }

        private bool TryPutDownDirtyPlates()
        {
            Chef chef = FindObjectOfType<Chef>();
            if (chef == null || chef.CurrentItem == null) return false;

            Plate plate = chef.CurrentItem.gameObject.GetComponent<Plate>();
            if (plate == null || !plate.IsDirty) return false;

            // Place the dirty plate(s)
            if (dirtyPlateStack == null)
            {
                // First dirty plate
                dirtyPlateStack = plate;
                chef.DropItem();
                plate.transform.SetParent(dirtyPlatePoint);
                plate.transform.localPosition = Vector3.zero;
                plate.transform.localRotation = Quaternion.identity;
                Debug.Log("DishwasherCounter: Placed dirty plate(s)");
                return true;
            }
            else
            {
                // Stack with existing dirty plates
                if (dirtyPlateStack.TryStackPlate(plate))
                {
                    chef.DropItem();
                    Debug.Log("DishwasherCounter: Stacked dirty plate(s)");
                    return true;
                }
            }

            return false;
        }

        protected override bool CanPlaceItem(IItem item)
        {
            // Dishwasher can only accept dirty plates, not clean plates
            if (!IsPlate(item)) return false;
            
            Plate plate = item.gameObject.GetComponent<Plate>();
            if (plate == null) return false;
            
            // Only accept dirty plates
            return plate.IsDirty;
        }

        protected override bool CanProcess(IItem item)
        {
            // Can only process dirty plates
            if (item == null) return false;
            if (!IsPlate(item)) return false;
            
            Plate plate = item.gameObject.GetComponent<Plate>();
            if (plate == null || !plate.IsDirty) return false;
            
            // Check if this is actually our dirty plate stack
            return plate == dirtyPlateStack;
        }

        protected override void ExecuteProcess()
        {
            if (dirtyPlateStack != null && dirtyPlateStack.IsDirty)
            {
                // Clean all dirty plates and transfer them to clean stack
                CleanDirtyPlates();
                UpdateCurrentItem();
            }
        }

        private void CleanDirtyPlates()
        {
            if (dirtyPlateStack == null) return;

            // Collect all dirty plates (bottom + stacked)
            var platesToClean = new System.Collections.Generic.List<Plate>();
            platesToClean.Add(dirtyPlateStack);
            
            while (dirtyPlateStack.HasStack)
            {
                Plate topPlate = dirtyPlateStack.TakeTopPlate();
                if (topPlate != null)
                {
                    platesToClean.Add(topPlate);
                }
            }

            // Clean all plates
            foreach (var plate in platesToClean)
            {
                plate.Clean();
                OnPlateWashed?.Invoke(plate);
                Debug.Log("DishwasherCounter: Cleaned a plate");
            }

            // Move all cleaned plates to clean stack
            foreach (var plate in platesToClean)
            {
                if (cleanPlateStack == null)
                {
                    // First clean plate
                    cleanPlateStack = plate;
                    plate.transform.SetParent(cleanPlatePoint);
                    plate.transform.localPosition = Vector3.zero;
                    plate.transform.localRotation = Quaternion.identity;
                }
                else
                {
                    // Stack on clean plates
                    cleanPlateStack.TryStackPlate(plate);
                }
            }

            // Clear dirty stack
            dirtyPlateStack = null;

            Debug.Log($"DishwasherCounter: Finished cleaning {platesToClean.Count} plate(s)");
        }

        public override void Select()
        {
            base.Select();
        }

        public override void Deselect()
        {
            base.Deselect();
        }
    }
}
