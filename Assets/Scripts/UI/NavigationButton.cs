using UnityEngine;
using TinyChef;

namespace TinyChef.UI
{
    [RequireComponent(typeof(LongPressButton))]
    public class NavigationButton : MonoBehaviour
    {
        [Header("Navigation Settings")]
        [SerializeField] private float continuousNavigationInterval = 0.2f;

        private LongPressButton longPressButton;
        private float lastNavigationTime = 0f;
        private bool isHolding = false;

        private void Awake()
        {
            longPressButton = GetComponent<LongPressButton>();
            if (longPressButton == null)
            {
                Debug.LogError("NavigationButton requires LongPressButton component!");
                return;
            }

            longPressButton.SetLongPressThreshold(0.1f);
            longPressButton.SetTriggerOnRelease(false);

            longPressButton.OnPressStart.AddListener(OnPressStart);
            longPressButton.OnPressEnd.AddListener(OnPressEnd);
        }

        private void Update()
        {
            if (isHolding)
            {
                if (Time.time - lastNavigationTime >= continuousNavigationInterval)
                {
                    TriggerNavigation();
                    lastNavigationTime = Time.time;
                }
            }
        }

        private void OnPressStart()
        {
            isHolding = true;
            lastNavigationTime = Time.time;
            TriggerNavigation();
        }

        private void OnPressEnd()
        {
            isHolding = false;
        }

        private void TriggerNavigation()
        {
            if (InputController.Instance != null)
            {
                InputController.Instance.TriggerNavigation();
            }
        }

        private void OnDestroy()
        {
            if (longPressButton != null)
            {
                longPressButton.OnPressStart.RemoveListener(OnPressStart);
                longPressButton.OnPressEnd.RemoveListener(OnPressEnd);
            }
        }
    }
}
