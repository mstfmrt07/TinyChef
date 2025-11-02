using System.Collections.Generic;
using UnityEngine;

namespace TinyChef
{
    public class TwoButtonInputHandler : MonoBehaviour
    {
        [Header("Input Settings")]
        public KeyCode navigationButton = KeyCode.Q; // Button 1
        public KeyCode interactionButton = KeyCode.E; // Button 2
        
        [Header("Navigation Settings")]
        public float longPressNavigationInterval = 0.2f; // Time between navigation steps when holding
        
        [Header("Interaction Settings")]
        public float longPressThreshold = 0.5f; // Time to hold for long press

        private float navigationPressTime = 0f;
        private float interactionPressTime = 0f;
        private float lastNavigationStepTime = 0f;
        private bool isNavigationHeld = false;
        private bool isInteractionHeld = false;

        // Events
        public System.Action OnNavigationPressed;
        public System.Action OnShortInteract;
        public System.Action OnLongInteract;

        private void Update()
        {
            HandleNavigationButton();
            HandleInteractionButton();
        }

        private void HandleNavigationButton()
        {
            if (Input.GetKeyDown(navigationButton))
            {
                navigationPressTime = Time.time;
                isNavigationHeld = true;
                lastNavigationStepTime = Time.time;
                OnNavigationPressed?.Invoke();
            }

            if (Input.GetKey(navigationButton) && isNavigationHeld)
            {
                // Long press - continuously navigate every interval
                if (Time.time - lastNavigationStepTime >= longPressNavigationInterval)
                {
                    OnNavigationPressed?.Invoke();
                    lastNavigationStepTime = Time.time;
                }
            }

            if (Input.GetKeyUp(navigationButton))
            {
                isNavigationHeld = false;
            }
        }

        private void HandleInteractionButton()
        {
            if (Input.GetKeyDown(interactionButton))
            {
                interactionPressTime = Time.time;
                isInteractionHeld = true;
            }

            if (Input.GetKey(interactionButton) && isInteractionHeld)
            {
                float holdDuration = Time.time - interactionPressTime;
                if (holdDuration >= longPressThreshold)
                {
                    // Long press detected
                    if (!isInteractionHeld || holdDuration < longPressThreshold + 0.1f)
                    {
                        // Only trigger once when threshold is crossed
                        OnLongInteract?.Invoke();
                        isInteractionHeld = false; // Prevent multiple triggers
                    }
                }
            }

            if (Input.GetKeyUp(interactionButton))
            {
                if (isInteractionHeld)
                {
                    float holdDuration = Time.time - interactionPressTime;
                    if (holdDuration < longPressThreshold)
                    {
                        // Short press
                        OnShortInteract?.Invoke();
                    }
                }
                isInteractionHeld = false;
            }
        }
    }
}
