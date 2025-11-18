using UnityEngine;
using TinyChef;

namespace TinyChef.UI
{
    [RequireComponent(typeof(LongPressButton))]
    public class InteractButton : MonoBehaviour
    {
        [Header("Interaction Settings")]
        [SerializeField] private float longPressThreshold = 0.5f;

        private LongPressButton longPressButton;

        private void Awake()
        {
            longPressButton = GetComponent<LongPressButton>();
            if (longPressButton == null)
            {
                Debug.LogError("InteractButton requires LongPressButton component!");
                return;
            }

            longPressButton.SetLongPressThreshold(longPressThreshold);
            longPressButton.SetTriggerOnRelease(true);

            longPressButton.OnShortPress.AddListener(OnShortPress);
            longPressButton.OnLongPress.AddListener(OnLongPress);
        }

        private void OnEnable()
        {
            InputController.OnInteractionStateChanged += OnKeyboardInteractionStateChanged;
        }

        private void OnDisable()
        {
            InputController.OnInteractionStateChanged -= OnKeyboardInteractionStateChanged;
        }

        private void OnKeyboardInteractionStateChanged(bool isPressed)
        {
            if (longPressButton != null)
            {
                longPressButton.SetVisualState(isPressed);
            }
        }

        private void OnShortPress()
        {
            TriggerShortInteract();
        }

        private void OnLongPress()
        {
            TriggerLongInteract();
        }

        private void TriggerShortInteract()
        {
            if (ReferenceManager.Instance.InputController != null)
            {
                ReferenceManager.Instance.InputController.TriggerShortInteract();
            }
        }

        private void TriggerLongInteract()
        {
            if (ReferenceManager.Instance.InputController != null)
            {
                ReferenceManager.Instance.InputController.TriggerLongInteract();
            }
        }

        private void OnDestroy()
        {
            if (longPressButton != null)
            {
                longPressButton.OnShortPress.RemoveListener(OnShortPress);
                longPressButton.OnLongPress.RemoveListener(OnLongPress);
            }
        }
    }
}
