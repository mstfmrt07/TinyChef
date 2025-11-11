using System;
using UnityEngine;

namespace TinyChef
{
    public class InputController : MonoBehaviour
    {
        private static InputController _instance;
        public static InputController Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<InputController>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("InputController");
                        _instance = go.AddComponent<InputController>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        [Header("Keyboard Input Settings")]
        public KeyCode navigationButton = KeyCode.Q;
        public KeyCode interactionButton = KeyCode.E;

        [Header("Navigation Settings")]
        public float longPressNavigationInterval = 0.2f;

        [Header("Interaction Settings")]
        public float longPressThreshold = 0.5f;

        public static event Action OnNavigationPressed;
        public static event Action OnShortInteract;
        public static event Action OnLongInteract;
        public static event Action<bool> OnNavigationStateChanged;
        public static event Action<bool> OnInteractionStateChanged;

        private float navigationPressTime = 0f;
        private float interactionPressTime = 0f;
        private float lastNavigationStepTime = 0f;
        private bool isNavigationHeld = false;
        private bool isInteractionHeld = false;
        private bool hasTriggeredLongInteract = false;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            HandleNavigation();
            HandleInteraction();
        }

        private void HandleNavigation()
        {
            if (Input.GetKeyDown(navigationButton))
            {
                navigationPressTime = Time.time;
                isNavigationHeld = true;
                lastNavigationStepTime = Time.time;
                OnNavigationPressed?.Invoke();
                OnNavigationStateChanged?.Invoke(true);
            }

            if (Input.GetKey(navigationButton) && isNavigationHeld)
            {
                if (Time.time - lastNavigationStepTime >= longPressNavigationInterval)
                {
                    OnNavigationPressed?.Invoke();
                    lastNavigationStepTime = Time.time;
                }
            }

            if (Input.GetKeyUp(navigationButton))
            {
                isNavigationHeld = false;
                OnNavigationStateChanged?.Invoke(false);
            }
        }

        private void HandleInteraction()
        {
            if (Input.GetKeyDown(interactionButton))
            {
                interactionPressTime = Time.time;
                isInteractionHeld = true;
                hasTriggeredLongInteract = false;
                OnInteractionStateChanged?.Invoke(true);
            }

            if (Input.GetKey(interactionButton) && isInteractionHeld)
            {
                float holdDuration = Time.time - interactionPressTime;
                if (holdDuration >= longPressThreshold && !hasTriggeredLongInteract)
                {
                    OnLongInteract?.Invoke();
                    hasTriggeredLongInteract = true;
                }
            }

            if (Input.GetKeyUp(interactionButton))
            {
                if (isInteractionHeld && !hasTriggeredLongInteract)
                {
                    float holdDuration = Time.time - interactionPressTime;
                    if (holdDuration < longPressThreshold)
                    {
                        OnShortInteract?.Invoke();
                    }
                }
                isInteractionHeld = false;
                hasTriggeredLongInteract = false;
                OnInteractionStateChanged?.Invoke(false);
            }
        }

        public void TriggerNavigation()
        {
            OnNavigationPressed?.Invoke();
        }

        public void TriggerShortInteract()
        {
            OnShortInteract?.Invoke();
        }

        public void TriggerLongInteract()
        {
            OnLongInteract?.Invoke();
        }

        public Vector2 MovementInput => new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    }
}
