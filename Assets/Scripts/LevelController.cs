using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TinyChef
{
    public class LevelController : MonoBehaviour
    {
        [Header("Level Settings")] public List<LevelData> levels;
        public Transform levelParent;

        [Header("Progress Settings")] public int currentLevelIndex = 0;

        private Level currentLevelInstance;
        private LevelData currentLevelData;
        private float levelTimeRemaining;
        private bool isLevelActive = false;

        // Events
        public Action<LevelData> OnLevelLoaded;
        public Action OnLevelStarted;
        public Action<int> OnLevelCompleted; // Passes star rating (1-3)
        public Action OnLevelFailed;
        public Action<float> OnLevelTimeChanged; // Passes time remaining

        // Public getters
        public LevelData CurrentLevelData => currentLevelData;
        public Level CurrentLevel => currentLevelInstance;

        public List<BaseCounter> LevelCounters
        {
            get
            {
                if (currentLevelInstance != null)
                {
                    return currentLevelInstance.GetCounters();
                }

                return new List<BaseCounter>();
            }
        }

        public bool HasDishwasher
        {
            get
            {
                if (currentLevelInstance != null)
                {
                    return currentLevelInstance.GetCounters().Any(c => c.counterType == CounterType.Dishwasher);
                }

                return false;
            }
        }

        public float LevelTimeRemaining => levelTimeRemaining;
        public bool IsLevelActive => isLevelActive;

        private void Awake()
        {
            if (levelParent == null)
            {
                levelParent = transform;
            }
        }

        /// <summary>
        /// Applies darkness settings for a level
        /// </summary>
        public void SetDarknessMode(bool isDark)
        {
            Light directionalLight = ReferenceManager.Instance.DirectionalLight;
            Chef chef = ReferenceManager.Instance.Chef;
            GameSettings gameSettings = ReferenceManager.Instance.GameSettings;

            if (directionalLight != null)
            {
                directionalLight.enabled = !isDark;
            }

            if (chef != null && chef.spotlight != null)
            {
                chef.spotlight.enabled = isDark;
            }

            if (isDark && gameSettings != null)
            {
                // Set environment lighting intensity
                RenderSettings.ambientIntensity = gameSettings.darkLevelEnvironmentIntensity;
                
                // Set environment reflections intensity
                RenderSettings.reflectionIntensity = gameSettings.darkLevelReflectionIntensity;
            }
            else
            {
                // Restore default values when not dark
                RenderSettings.ambientIntensity = 1f;
                RenderSettings.reflectionIntensity = 1f;
            }
        }

        private void Update()
        {
            if (isLevelActive && currentLevelData != null)
            {
                levelTimeRemaining -= Time.deltaTime;
                OnLevelTimeChanged?.Invoke(levelTimeRemaining);

                if (levelTimeRemaining <= 0f)
                {
                    levelTimeRemaining = 0f;
                    EndLevel();
                }
            }
        }

        public void LoadLevel(int levelIndex)
        {
            if (levels == null || levelIndex < 0 || levelIndex >= levels.Count)
            {
                Debug.LogError($"Invalid level index: {levelIndex}");
                return;
            }

            // Unload current level
            UnloadCurrentLevel();

            // Load new level
            currentLevelIndex = levelIndex;
            currentLevelData = levels[levelIndex];

            if (currentLevelData.levelPrefab != null)
            {
                GameObject levelGameObject = Instantiate(currentLevelData.levelPrefab, levelParent);
                currentLevelInstance = levelGameObject.GetComponent<Level>();

                if (currentLevelInstance == null)
                {
                    Debug.LogError($"Level prefab '{currentLevelData.levelName}' does not have a Level component!");
                    Destroy(levelGameObject);
                    return;
                }

                int counterCount = currentLevelInstance.CounterCount;
                Debug.Log($"Level loaded: {currentLevelData.levelName} with {counterCount} counters");
            }
            else
            {
                Debug.LogWarning($"Level prefab is null for level: {currentLevelData.levelName}");
                currentLevelInstance = null;
            }

            // Set level data in order manager
            if (ReferenceManager.Instance.OrderManager != null)
            {
                ReferenceManager.Instance.OrderManager.SetLevelData(currentLevelData);
            }

            // Initialize level timer but don't start yet
            levelTimeRemaining = currentLevelData.levelDuration;
            isLevelActive = false; // Wait for StartLevel() to be called

            OnLevelLoaded?.Invoke(currentLevelData);
        }

        public void StartLevel()
        {
            if (currentLevelData == null) return;

            // Start the level
            isLevelActive = true;
            levelTimeRemaining = currentLevelData.levelDuration;
            OnLevelStarted?.Invoke();
        }

        private void UnloadCurrentLevel()
        {
            if (currentLevelInstance != null)
            {
                Destroy(currentLevelInstance.gameObject);
                currentLevelInstance = null;
            }
        }

        public void CheckLevelCompletion(int currentScore)
        {
            // This method is now just for checking thresholds during gameplay
            // Actual level completion is handled by EndLevel when timer runs out
            if (currentLevelData == null) return;

            int starRating = GetStarRating(currentScore);

            // Just notify about progress, don't end level yet
            // Level ends when timer runs out
        }

        private void EndLevel()
        {
            if (!isLevelActive) return;

            isLevelActive = false;

            // Get final score from OrderManager
            int finalScore = 0;
            if (ReferenceManager.Instance.OrderManager != null)
            {
                finalScore = ReferenceManager.Instance.OrderManager.GetTotalScore();
            }

            int starRating = GetStarRating(finalScore);

            Debug.Log($"Level ended! Final score: {finalScore}, Stars: {starRating}");
            OnLevelCompleted?.Invoke(starRating);
        }

        public int GetStarRating(int score)
        {
            if (currentLevelData == null) return 0;

            if (score >= currentLevelData.threeStarScore)
                return 3;
            else if (score >= currentLevelData.twoStarScore)
                return 2;
            else if (score >= currentLevelData.oneStarScore)
                return 1;
            else
                return 0;
        }

        public void LoadNextLevel()
        {
            if (currentLevelIndex + 1 < levels.Count)
            {
                LoadLevel(currentLevelIndex + 1);
            }
            else
            {
                Debug.Log("All levels completed!");
            }
        }

        public LevelData GetCurrentLevelData() => currentLevelData;
        public int GetCurrentLevelIndex() => currentLevelIndex;

        public BaseCounter GetCounterAtIndex(int index)
        {
            if (currentLevelInstance != null)
            {
                return currentLevelInstance.GetCounterAtIndex(index);
            }

            return null;
        }
    }
}