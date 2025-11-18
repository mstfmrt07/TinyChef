using System.Collections.Generic;
using UnityEngine;

namespace TinyChef
{
    public class Chef : MonoBehaviour
    {
        [Header("Chef Settings")] public Transform mainBody;
        public float interactCooldown;
        public LayerMask interactionLayers;
        public Transform grabPoint;

        [Header("Navigation Settings")] public float rotationSpeed = 5f;

        private float interactTimer = 0f;
        private bool canInteract = true;
        private IInteractable currentSelection;
        private IItem currentItem;
        public IItem CurrentItem => currentItem;

        private int currentCounterIndex = 0;
        private LevelController levelController;
        private CounterGroup activeCounterGroup;

        private List<BaseCounter> AvailableCounters
        {
            get
            {
                if (activeCounterGroup != null)
                {
                    return activeCounterGroup.Counters;
                }

                return new List<BaseCounter>();
            }
        }

        private void Start()
        {
            interactTimer = interactCooldown;
            canInteract = true;

            // Subscribe to global input controller events
            InputController.OnNavigationPressed += NavigateToNextCounter;
            InputController.OnShortInteract += HandleShortInteract;
            InputController.OnLongInteract += HandleLongInteract;

            levelController = ReferenceManager.Instance.LevelController ?? FindObjectOfType<LevelController>();

            // Subscribe to level loaded event to reset counter index
            if (levelController != null)
            {
                levelController.OnLevelLoaded += OnLevelLoaded;
            }

            // Subscribe to counter group changes
            CounterGroup.OnCounterGroupChanged += OnCounterGroupChanged;
        }

        private void Update()
        {
            // Update interact timer
            if (interactTimer < interactCooldown)
            {
                interactTimer += Time.deltaTime;
                canInteract = false;
            }
            else
            {
                canInteract = true;
            }

            UpdateCurrentSelection();
            RotateTowardsCurrentCounter();
        }

        private void OnLevelLoaded(LevelData levelData)
        {
            // Reset to first counter when level loads
            currentCounterIndex = 0;
            
            // Set the first counter group as active if available
            if (levelController != null && levelController.CurrentLevel != null)
            {
                CounterGroup firstGroup = levelController.CurrentLevel.GetFirstCounterGroup();
                if (firstGroup != null)
                {
                    SetActiveCounterGroup(firstGroup);
                }
            }
        }

        private void OnCounterGroupChanged(CounterGroup newGroup)
        {
            if (activeCounterGroup == newGroup) return;

            activeCounterGroup = newGroup;
            
            // Reset counter index when switching groups
            currentCounterIndex = 0;
        }

        private void SetActiveCounterGroup(CounterGroup group)
        {
            if (activeCounterGroup == group) return;

            activeCounterGroup = group;

            // Teleport chef to the new counter group position
            transform.position = activeCounterGroup.transform.position;
            
            // Reset counter index when switching groups
            currentCounterIndex = 0;
            
            // Fire the event to notify that counter group changed
            CounterGroup.SetActiveGroup(group);
        }

        private void RotateTowardsCurrentCounter()
        {
            if (mainBody == null) return;

            List<BaseCounter> counters = AvailableCounters;
            if (counters.Count == 0 || currentCounterIndex < 0 || currentCounterIndex >= counters.Count) return;

            BaseCounter targetCounter = counters[currentCounterIndex];
            if (targetCounter == null) return;

            Vector3 direction = (targetCounter.itemPlacePoint.position - mainBody.position).normalized;
            direction.y = 0; // Keep rotation on horizontal plane

            if (direction.magnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                mainBody.rotation = Quaternion.Slerp(mainBody.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }

        private void UpdateCurrentSelection()
        {
            List<BaseCounter> counters = AvailableCounters;

            if (counters.Count == 0)
            {
                if (currentSelection != null)
                {
                    currentSelection.Deselect();
                    currentSelection = null;
                }

                // Reset index if no counters available
                currentCounterIndex = 0;

                return;
            }

            // Adjust index if it's out of bounds
            if (currentCounterIndex >= counters.Count)
            {
                currentCounterIndex = 0;
            }

            if (currentCounterIndex >= 0 && currentCounterIndex < counters.Count)
            {
                IInteractable nextSelection = counters[currentCounterIndex] as IInteractable;
                if (currentSelection != nextSelection)
                {
                    if (currentSelection != null)
                    {
                        currentSelection.Deselect();
                    }

                    currentSelection = nextSelection;
                    currentSelection?.Select();
                }
            }
        }

        private void NavigateToNextCounter()
        {
            List<BaseCounter> counters = AvailableCounters;

            if (counters.Count == 0) return;

            // Increment index (cyclically)
            currentCounterIndex = (currentCounterIndex + 1) % counters.Count;

            UpdateCurrentSelection();
        }

        private void HandleShortInteract()
        {
            if (!canInteract) return;

            // Don't allow interaction if level is not active
            if (levelController != null && !levelController.IsLevelActive)
            {
                return;
            }

            if (currentSelection != null)
            {
                if (currentSelection is BaseCounter counter)
                {
                    counter.Interact();
                }
                else
                {
                    currentSelection.Interact();
                }

                interactTimer = 0f;
                canInteract = false;
            }
        }

        private void HandleLongInteract()
        {
            // Don't allow interaction if level is not active
            if (levelController != null && !levelController.IsLevelActive)
            {
                return;
            }

            if (currentSelection != null && currentSelection is BaseCounter counter)
            {
                counter.Process();
            }
        }

        public void Interact(IInteractable interactable)
        {
            interactable?.Interact();
        }

        public void GrabItem(IItem item)
        {
            if (item == null) return;

            Debug.Log("Item Grabbed");
            currentItem = item;
            if (grabPoint != null)
            {
                currentItem.transform.SetParent(grabPoint);
                currentItem.transform.localPosition = Vector3.zero;
                currentItem.transform.localRotation = Quaternion.identity;
            }
        }

        public void DropItem()
        {
            if (currentItem == null) return;

            Debug.Log("Item Dropped");
            currentItem = null;
        }

        public void TeleportToPortal(PortalCounter targetPortal, CounterGroup targetGroup)
        {
            if (targetPortal == null || targetGroup == null)
            {
                Debug.LogWarning("Cannot teleport: target portal or group is null!");
                return;
            }

            // Switch to the target counter group
            SetActiveCounterGroup(targetGroup);

            // Find the index of the target portal in the new group's counters
            List<BaseCounter> counters = targetGroup.Counters;
            for (int i = 0; i < counters.Count; i++)
            {
                if (counters[i] == targetPortal)
                {
                    currentCounterIndex = i;
                    break;
                }
            }

            // Update selection immediately
            UpdateCurrentSelection();
        }

        private void OnDestroy()
        {
            // Unsubscribe from input controller events
            InputController.OnNavigationPressed -= NavigateToNextCounter;
            InputController.OnShortInteract -= HandleShortInteract;
            InputController.OnLongInteract -= HandleLongInteract;

            if (levelController != null)
            {
                levelController.OnLevelLoaded -= OnLevelLoaded;
            }

            CounterGroup.OnCounterGroupChanged -= OnCounterGroupChanged;
        }
    }
}