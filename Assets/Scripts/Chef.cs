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
        private TwoButtonInputHandler inputHandler;
        private LevelController levelController;

        private List<BaseCounter> AvailableCounters
        {
            get
            {
                if (levelController != null)
                {
                    return levelController.LevelCounters;
                }

                return new List<BaseCounter>();
            }
        }

        private void Start()
        {
            interactTimer = interactCooldown;
            canInteract = true;
            inputHandler = GetComponent<TwoButtonInputHandler>();
            if (inputHandler == null)
            {
                inputHandler = gameObject.AddComponent<TwoButtonInputHandler>();
            }

            inputHandler.OnNavigationPressed += NavigateToNextCounter;
            inputHandler.OnShortInteract += HandleShortInteract;
            inputHandler.OnLongInteract += HandleLongInteract;

            levelController = FindObjectOfType<LevelController>();

            // Subscribe to level loaded event to reset counter index
            if (levelController != null)
            {
                levelController.OnLevelLoaded += OnLevelLoaded;
            }
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
            currentItem.transform.SetParent(null);
            currentItem = null;
        }

        private void OnDestroy()
        {
            if (levelController != null)
            {
                levelController.OnLevelLoaded -= OnLevelLoaded;
            }
        }
    }
}