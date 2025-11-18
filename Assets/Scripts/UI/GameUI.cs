using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TinyChef
{
    public class GameUI : MonoBehaviour
    {
        [Header("Score Display")] public TextMeshProUGUI scoreText;

        [Header("Timer Display")] public TextMeshProUGUI timerText;
        public Color timerNormalColor = Color.green;
        public Color timerWarningColor = Color.yellow;
        public Color timerCriticalColor = Color.red;
        public float warningThreshold = 60f; // seconds
        public float criticalThreshold = 30f; // seconds

        [Header("Level End Panel")] public GameObject levelEndPanel;
        public TextMeshProUGUI levelEndScoreText;

        private LevelController levelController;
        private int currentScore = 0;

        private void Start()
        {
            levelController = ReferenceManager.Instance.LevelController ?? FindObjectOfType<LevelController>();

            // Subscribe to events
            if (levelController != null)
            {
                levelController.OnLevelLoaded += OnLevelLoaded;
                levelController.OnLevelTimeChanged += OnLevelTimeChanged;
                levelController.OnLevelCompleted += OnLevelCompleted;
            }

            if (ReferenceManager.Instance.OrderManager != null)
            {
                ReferenceManager.Instance.OrderManager.OnScoreChanged += OnScoreChanged;
            }

            // Hide level end panel initially
            if (levelEndPanel != null)
            {
                levelEndPanel.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (levelController != null)
            {
                levelController.OnLevelLoaded -= OnLevelLoaded;
                levelController.OnLevelTimeChanged -= OnLevelTimeChanged;
                levelController.OnLevelCompleted -= OnLevelCompleted;
            }

            if (ReferenceManager.Instance.OrderManager != null)
            {
                ReferenceManager.Instance.OrderManager.OnScoreChanged -= OnScoreChanged;
            }
        }

        private void OnLevelLoaded(LevelData levelData)
        {
            if (levelData == null) return;

            // Reset score display
            currentScore = 0;
            UpdateScoreDisplay();
        }

        private void OnScoreChanged(int newScore)
        {
            currentScore = newScore;
            UpdateScoreDisplay();
        }

        private void OnLevelTimeChanged(float timeRemaining)
        {
            UpdateTimerDisplay(timeRemaining);
        }

        private void OnLevelCompleted(int starRating)
        {
            ShowLevelEndScreen(starRating);
        }

        private void UpdateScoreDisplay()
        {
            if (scoreText != null)
            {
                scoreText.text = $"Score:\n{currentScore}";
            }
        }

        private void UpdateTimerDisplay(float timeRemaining)
        {
            if (timerText != null)
            {
                int minutes = Mathf.FloorToInt(timeRemaining / 60f);
                int seconds = Mathf.FloorToInt(timeRemaining % 60f);
                timerText.text = $"Time:\n{minutes:00}:{seconds:00}";

                // Change color based on time remaining
                if (timeRemaining <= criticalThreshold)
                {
                    timerText.color = timerCriticalColor;
                }
                else if (timeRemaining <= warningThreshold)
                {
                    timerText.color = timerWarningColor;
                }
                else
                {
                    timerText.color = timerNormalColor;
                }
            }
        }

        private void ShowLevelEndScreen(int starRating)
        {
            if (levelEndPanel == null) return;

            levelEndPanel.SetActive(true);

            // Display final score
            if (levelEndScoreText != null)
            {
                levelEndScoreText.text = "Final Score: " + currentScore.ToString();
            }
        }

        public void OnRestartButtonClicked()
        {
            if (levelController != null)
            {
                levelController.LoadLevel(levelController.GetCurrentLevelIndex());
                if (levelEndPanel != null)
                {
                    levelEndPanel.SetActive(false);
                }
            }
        }

        public void OnNextLevelButtonClicked()
        {
            if (levelController != null)
            {
                levelController.LoadNextLevel();
                if (levelEndPanel != null)
                {
                    levelEndPanel.SetActive(false);
                }
            }
        }
    }
}