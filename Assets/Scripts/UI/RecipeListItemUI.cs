using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TinyChef
{
    public class RecipeListItemUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Image iconImage;
        [SerializeField] private GameObject lockIcon;
        [SerializeField] private GameObject maskOverlay;
        [SerializeField] private GameObject selectionHighlight;
        [SerializeField] private Button selectButton;

        private RecipeData recipe;
        private bool isUnlocked;
        private Action<RecipeData> onSelected;

        public RecipeData Recipe => recipe;

        private void Awake()
        {
            if (selectButton == null)
            {
                selectButton = GetComponent<Button>();
            }

            if (selectButton != null)
            {
                selectButton.onClick.AddListener(HandleClick);
            }
        }

        private void OnDestroy()
        {
            if (selectButton != null)
            {
                selectButton.onClick.RemoveListener(HandleClick);
            }
        }

        public void SetRecipe(RecipeData recipeData, bool unlocked, Action<RecipeData> onClick)
        {
            recipe = recipeData;
            isUnlocked = unlocked;
            onSelected = onClick;

            if (titleText != null)
            {
                titleText.text = recipeData != null ? recipeData.GetDisplayName() : string.Empty;
            }

            if (iconImage != null)
            {
                iconImage.sprite = recipeData != null ? recipeData.icon : null;
                iconImage.enabled = iconImage.sprite != null;
            }

            if (lockIcon != null)
            {
                lockIcon.SetActive(!unlocked);
            }

            if (maskOverlay != null)
            {
                maskOverlay.SetActive(!unlocked);
            }

            if (selectButton != null)
            {
                selectButton.interactable = unlocked && recipeData != null;
            }
        }

        public void SetPlaceholder(string placeholderText)
        {
            recipe = null;
            isUnlocked = false;
            onSelected = null;

            if (titleText != null)
            {
                titleText.text = placeholderText;
            }

            if (iconImage != null)
            {
                iconImage.sprite = null;
                iconImage.enabled = false;
            }

            if (lockIcon != null)
            {
                lockIcon.SetActive(false);
            }

            if (maskOverlay != null)
            {
                maskOverlay.SetActive(false);
            }

            if (selectButton != null)
            {
                selectButton.interactable = false;
            }
        }

        public void SetSelected(bool isSelected)
        {
            if (selectionHighlight != null)
            {
                selectionHighlight.SetActive(isSelected);
            }
        }

        private void HandleClick()
        {
            if (!isUnlocked || recipe == null)
            {
                return;
            }

            onSelected?.Invoke(recipe);
        }
    }
}

