using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TinyChef
{
    public class StoveUI : MonoBehaviour
    {
        [Header("UI References")] public Transform ingredientIconContainer;
        public GameObject ingredientIconPrefab;
        public Image progressBar;

        private StoveCounter stoveCounter;

        private Camera mainCamera;

        public void Initialize(StoveCounter stove)
        {
            stoveCounter = stove;
            mainCamera = Camera.main;
            UpdateUI();
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        public void UpdateUI()
        {
            if (stoveCounter == null) return;

            // Update ingredient icons
            UpdateIngredientIcons();

            // Update progress bar
            UpdateProgressBar();
        }

        private void UpdateIngredientIcons()
        {
            if (ingredientIconContainer == null || ingredientIconPrefab == null) return;

            // Clear existing icons
            foreach (Transform child in ingredientIconContainer)
            {
                Destroy(child.gameObject);
            }

            // Create icons for each ingredient
            var ingredients = stoveCounter.GetIngredients();
            foreach (var ingredient in ingredients)
            {
                if (ingredient != null && ingredient.data != null && ingredient.data.icon != null)
                {
                    GameObject iconObj = Instantiate(ingredientIconPrefab, ingredientIconContainer);
                    Image iconImage = iconObj.GetComponent<Image>();
                    if (iconImage != null)
                    {
                        iconImage.sprite = ingredient.data.icon;
                    }
                }
            }
        }

        private void UpdateProgressBar()
        {
            if (progressBar == null) return;

            float progress = stoveCounter.GetCookingProgress();
            progressBar.fillAmount = progress;
        }

        private void Update()
        {
            FaceCamera();

            if (stoveCounter != null)
            {
                UpdateUI();
            }
        }

        private void FaceCamera()
        {
            if (mainCamera != null)
            {
                // Make UI face the camera
                Vector3 directionToCamera = mainCamera.transform.position - transform.position;
                if (directionToCamera != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(-directionToCamera);
                }
            }
        }
    }
}