using UnityEngine;
using TMPro;

namespace TinyChef
{
    public class LevelInfoPanel : MonoBehaviour
    {
        [Header("Level Info")]
        public TextMeshProUGUI levelNumberText;
        public TextMeshProUGUI levelNameText;
        public TextMeshProUGUI levelDurationText;

        [Header("Star Thresholds")]
        public TextMeshProUGUI oneStarThresholdText;
        public TextMeshProUGUI twoStarThresholdText;
        public TextMeshProUGUI threeStarThresholdText;

        [Header("Panel")]
        public GameObject panelObject;

        private LevelController levelController;

        private void Start()
        {
            levelController = FindObjectOfType<LevelController>();

            if (levelController != null)
            {
                levelController.OnLevelLoaded += OnLevelLoaded;
            }

            // Show panel initially
            if (panelObject != null)
            {
                panelObject.SetActive(true);
            }
        }

        private void OnDestroy()
        {
            if (levelController != null)
            {
                levelController.OnLevelLoaded -= OnLevelLoaded;
            }
        }

        private void OnLevelLoaded(LevelData levelData)
        {
            if (levelData == null) return;

            UpdateLevelInfo(levelData);
            
            // Show panel when level loads
            if (panelObject != null)
            {
                panelObject.SetActive(true);
            }
        }

        private void UpdateLevelInfo(LevelData levelData)
        {
            // Update level number
            if (levelNumberText != null && levelController != null)
            {
                levelNumberText.text = "Level " + (levelController.GetCurrentLevelIndex() + 1).ToString();
            }

            // Update level name
            if (levelNameText != null)
            {
                levelNameText.text = levelData.levelName;
            }

            // Update duration
            if (levelDurationText != null)
            {
                int minutes = Mathf.FloorToInt(levelData.levelDuration / 60f);
                int seconds = Mathf.FloorToInt(levelData.levelDuration % 60f);
                levelDurationText.text = string.Format("Time: {0:00}:{1:00}", minutes, seconds);
            }

            // Update star thresholds
            if (oneStarThresholdText != null)
                oneStarThresholdText.text = levelData.oneStarScore.ToString();
            if (twoStarThresholdText != null)
                twoStarThresholdText.text = levelData.twoStarScore.ToString();
            if (threeStarThresholdText != null)
                threeStarThresholdText.text = levelData.threeStarScore.ToString();
        }

        public void OnPlayButtonClicked()
        {
            // Hide panel
            if (panelObject != null)
            {
                panelObject.SetActive(false);
            }

            // Start the level
            if (levelController != null)
            {
                levelController.StartLevel();
            }
        }
    }
}

