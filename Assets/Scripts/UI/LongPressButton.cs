using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace TinyChef.UI
{
    public class LongPressButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [Header("Button Settings")]
        [SerializeField] private float longPressThreshold = 0.5f;
        [SerializeField] private bool triggerOnRelease = true;

        [Header("Visual Feedback")]
        [SerializeField] private Image buttonImage;
        [SerializeField] private Sprite normalSprite;
        [SerializeField] private Sprite pressedSprite;

        [Header("Events")]
        public UnityEngine.Events.UnityEvent OnShortPress;
        public UnityEngine.Events.UnityEvent OnLongPress;
        public UnityEngine.Events.UnityEvent OnPressStart;
        public UnityEngine.Events.UnityEvent OnPressEnd;

        private bool isPressed = false;
        private float pressStartTime = 0f;
        private bool hasTriggeredLongPress = false;

        private void Awake()
        {
            if (buttonImage == null)
            {
                buttonImage = GetComponent<Image>();
            }

            if (normalSprite == null && buttonImage != null)
            {
                normalSprite = buttonImage.sprite;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!isPressed)
            {
                isPressed = true;
                pressStartTime = Time.time;
                hasTriggeredLongPress = false;
                UpdateVisual(true);
                OnPressStart?.Invoke();
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (isPressed)
            {
                float holdDuration = Time.time - pressStartTime;

                if (hasTriggeredLongPress)
                {
                }
                else if (triggerOnRelease)
                {
                    if (holdDuration < longPressThreshold)
                    {
                        OnShortPress?.Invoke();
                    }
                }

                UpdateVisual(false);
                OnPressEnd?.Invoke();
                isPressed = false;
                hasTriggeredLongPress = false;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (isPressed)
            {
                UpdateVisual(false);
                OnPressEnd?.Invoke();
                isPressed = false;
                hasTriggeredLongPress = false;
            }
        }

        private void Update()
        {
            if (isPressed && !hasTriggeredLongPress)
            {
                float holdDuration = Time.time - pressStartTime;
                if (holdDuration >= longPressThreshold)
                {
                    hasTriggeredLongPress = true;
                    OnLongPress?.Invoke();
                }
            }
        }

        private void UpdateVisual(bool pressed)
        {
            if (buttonImage != null)
            {
                if (pressed && pressedSprite != null)
                {
                    buttonImage.sprite = pressedSprite;
                }
                else if (!pressed && normalSprite != null)
                {
                    buttonImage.sprite = normalSprite;
                }
            }
        }

        public void SetNormalSprite(Sprite sprite)
        {
            normalSprite = sprite;
            if (!isPressed && buttonImage != null)
            {
                buttonImage.sprite = normalSprite;
            }
        }

        public void SetPressedSprite(Sprite sprite)
        {
            pressedSprite = sprite;
        }

        public void SetLongPressThreshold(float threshold)
        {
            longPressThreshold = threshold;
        }

        public void SetTriggerOnRelease(bool triggerOnRelease)
        {
            this.triggerOnRelease = triggerOnRelease;
        }
    }
}
