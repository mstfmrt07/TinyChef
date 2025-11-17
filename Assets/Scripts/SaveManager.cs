using System;
using System.Collections.Generic;
using UnityEngine;

namespace TinyChef
{
    public class SaveManager : MonoBehaviour
    {
        private const string SaveKey = "TinyChef_SaveData";

        [Serializable]
        private class SaveData
        {
            public List<string> unlockedRecipes = new List<string>();
        }

        public static SaveManager Instance { get; private set; }
        public event Action<string> OnRecipeUnlocked;

        private SaveData saveData = new SaveData();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            Load();
        }

        public bool IsRecipeUnlocked(string recipeId)
        {
            if (string.IsNullOrEmpty(recipeId))
            {
                return false;
            }

            return saveData.unlockedRecipes.Contains(recipeId);
        }

        public void EnsureRecipeUnlocked(string recipeId)
        {
            if (string.IsNullOrEmpty(recipeId))
            {
                return;
            }

            if (!saveData.unlockedRecipes.Contains(recipeId))
            {
                saveData.unlockedRecipes.Add(recipeId);
                Save();
            }
        }

        public void UnlockRecipe(string recipeId, bool silent = false)
        {
            if (string.IsNullOrEmpty(recipeId))
            {
                return;
            }

            if (!saveData.unlockedRecipes.Contains(recipeId))
            {
                saveData.unlockedRecipes.Add(recipeId);
                Save();

                if (!silent)
                {
                    OnRecipeUnlocked?.Invoke(recipeId);
                }
            }
        }

        public void LockRecipe(string recipeId)
        {
            if (string.IsNullOrEmpty(recipeId))
            {
                return;
            }

            if (saveData.unlockedRecipes.Remove(recipeId))
            {
                Save();
            }
        }

        public IReadOnlyList<string> GetUnlockedRecipeIds()
        {
            return saveData.unlockedRecipes.AsReadOnly();
        }

        public void ResetRecipeProgress()
        {
            saveData.unlockedRecipes.Clear();
            Save();
        }

        private void Load()
        {
            if (PlayerPrefs.HasKey(SaveKey))
            {
                string json = PlayerPrefs.GetString(SaveKey);
                if (!string.IsNullOrEmpty(json))
                {
                    saveData = JsonUtility.FromJson<SaveData>(json);
                }
            }

            if (saveData == null)
            {
                saveData = new SaveData();
            }
        }

        private void Save()
        {
            string json = JsonUtility.ToJson(saveData);
            PlayerPrefs.SetString(SaveKey, json);
            PlayerPrefs.Save();
        }
    }
}