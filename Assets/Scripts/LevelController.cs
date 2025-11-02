using System;
using System.Collections.Generic;
using UnityEngine;

namespace TinyChef
{
    public class LevelController : MonoBehaviour
    {
        [Header("Level Settings")]
        public List<LevelData> levels;
        public Transform levelParent;
        public OrderManager orderManager;

        [Header("Progress Settings")]
        public int currentLevelIndex = 0;

        private Level currentLevelInstance;
        private LevelData currentLevelData;

        // Events
        public Action<LevelData> OnLevelLoaded;
        public Action<int> OnLevelCompleted; // Passes star rating (1-3)
        public Action OnLevelFailed;

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

        private void Awake()
        {
            if (levelParent == null)
            {
                levelParent = transform;
            }

            if (orderManager == null)
            {
                orderManager = FindObjectOfType<OrderManager>();
            }
        }

        private void Start()
        {
            // Load the first level on start
            if (levels != null && levels.Count > 0)
            {
                LoadLevel(currentLevelIndex);
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
            if (orderManager != null)
            {
                orderManager.SetLevelData(currentLevelData);
            }

            OnLevelLoaded?.Invoke(currentLevelData);
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
            if (currentLevelData == null) return;

            int starRating = GetStarRating(currentScore);
            
            if (starRating >= 1)
            {
                OnLevelCompleted?.Invoke(starRating);
                Debug.Log($"Level completed with {starRating} star(s)! Score: {currentScore}");
                
                // Optionally auto-advance to next level
                // LoadNextLevel();
            }
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