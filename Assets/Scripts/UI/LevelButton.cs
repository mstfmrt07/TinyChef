using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TinyChef.UI
{
    [RequireComponent(typeof(Button))]
    public class LevelButton : MonoBehaviour
    {
        private int levelIndex;
        private System.Action<int> onClickCallback;

        private Button button;
        private TextMeshProUGUI levelText;

        public int LevelIndex => levelIndex;

        private void Awake()
        {
            button = GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(OnButtonClicked);
            }

            levelText = GetComponentInChildren<TextMeshProUGUI>();
        }

        public void Initialize(int index, System.Action<int> callback)
        {
            levelIndex = index;
            onClickCallback = callback;

            UpdateButtonText();
        }

        private void UpdateButtonText()
        {
            if (levelText != null)
            {
                levelText.text = (levelIndex + 1).ToString();
            }
        }

        private void OnButtonClicked()
        {
            onClickCallback?.Invoke(levelIndex);
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(OnButtonClicked);
            }
        }
    }
}

