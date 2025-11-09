using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TinyChef;

namespace TinyChef.UI
{
    public class LevelSelectionScreen : MonoBehaviour
    {
        [Header("References")] [SerializeField]
        private Transform container;

        [SerializeField] private GameObject levelButtonPrefab;
        [SerializeField] private ScrollRect scrollRect;

        [Header("Sine Wave Distribution")] [SerializeField]
        private float maxWidth = 400f;

        [SerializeField] private float verticalSpacing = 150f;
        [SerializeField] private float sineWaveFrequency = 0.5f;
        [SerializeField] private float sineWaveAmplitude = 1f;

        [SerializeField] private float itemHeight = 100f;

        private List<LevelButton> levelButtons = new List<LevelButton>();
        private int totalLevelCount = 0;
        private float containerHeight = 0f;

        private LevelController levelController;

        private void Awake()
        {
            if (container == null)
            {
                container = transform;
            }

            if (scrollRect == null)
            {
                scrollRect = GetComponentInParent<ScrollRect>();
            }

            if (levelController == null)
            {
                levelController = FindObjectOfType<LevelController>();
            }
        }

        private void Start()
        {
            InitializeLevels();
            CreateAllButtons();
        }

        private void InitializeLevels()
        {
            if (levelController == null)
            {
                Debug.LogError("LevelController not found!");
                return;
            }

            totalLevelCount = levelController.levels != null ? levelController.levels.Count : 0;

            if (totalLevelCount == 0)
            {
                Debug.LogWarning("No levels found in LevelController!");
                return;
            }

            ResizeContainer();
        }

        private void ResizeContainer()
        {
            containerHeight = (totalLevelCount - 1) * verticalSpacing + itemHeight;

            RectTransform containerRect = container as RectTransform;
            if (containerRect != null)
            {
                Vector2 sizeDelta = containerRect.sizeDelta;
                containerRect.sizeDelta = new Vector2(sizeDelta.x, containerHeight);
            }

            if (scrollRect != null && scrollRect.content != null)
            {
                scrollRect.content.sizeDelta = new Vector2(scrollRect.content.sizeDelta.x, containerHeight);
            }
        }

        private void CreateAllButtons()
        {
            if (levelButtonPrefab == null) return;

            for (int i = 0; i < totalLevelCount; i++)
            {
                CreateLevelButton(i);
            }

            UpdateButtonPositions();
        }

        private void CreateLevelButton(int levelIndex)
        {
            if (levelIndex < 0 || levelIndex >= totalLevelCount) return;
            if (levelButtonPrefab == null) return;

            GameObject buttonObj = Instantiate(levelButtonPrefab, container);
            LevelButton levelButton = buttonObj.GetComponent<LevelButton>();

            if (levelButton == null)
            {
                levelButton = buttonObj.AddComponent<LevelButton>();
            }

            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
            if (buttonRect != null)
            {
                buttonRect.anchorMin = new Vector2(0.5f, 1f);
                buttonRect.anchorMax = new Vector2(0.5f, 1f);
                buttonRect.pivot = new Vector2(0.5f, 1f);
            }

            levelButton.Initialize(levelIndex, OnLevelButtonClicked);
            levelButtons.Add(levelButton);
        }

        private void OnLevelButtonClicked(int levelIndex)
        {
            if (levelController != null)
            {
                levelController.LoadLevel(levelIndex);
            }

            gameObject.SetActive(false);
        }

        private void UpdateButtonPositions()
        {
            foreach (LevelButton button in levelButtons)
            {
                int levelIndex = button.LevelIndex;
                Vector2 position = CalculateButtonPosition(levelIndex);

                RectTransform buttonRect = button.GetComponent<RectTransform>();
                if (buttonRect != null)
                {
                    buttonRect.anchoredPosition = position;
                }
            }
        }

        private Vector2 CalculateButtonPosition(int levelIndex)
        {
            float y = -levelIndex * verticalSpacing;
            float x = CalculateSineWaveOffset(levelIndex);
            return new Vector2(x, y);
        }

        private float CalculateSineWaveOffset(int levelIndex)
        {
            float angle = levelIndex * sineWaveFrequency * Mathf.PI * 2f + Mathf.PI * 0.5f;
            float sineValue = Mathf.Sin(angle);
            return sineValue * maxWidth * 0.5f * sineWaveAmplitude;
        }

        public void SetMaxWidth(float width)
        {
            maxWidth = width;
            UpdateButtonPositions();
        }

        public void SetVerticalSpacing(float spacing)
        {
            verticalSpacing = spacing;
            ResizeContainer();
            UpdateButtonPositions();
        }
    }
}