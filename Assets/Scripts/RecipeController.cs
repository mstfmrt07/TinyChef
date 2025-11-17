using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TinyChef
{
    public class RecipeController : MonoBehaviour
    {
        public static RecipeController Instance { get; private set; }

        [Header("Recipe Sources")]
        [SerializeField] private List<RecipeData> manualRecipes = new List<RecipeData>();
        [SerializeField] private bool autoPopulateFromResources = true;
        [SerializeField] private string recipesResourceFolder = "Recipes";

        private readonly List<RecipeData> allRecipes = new List<RecipeData>();
        private SaveManager saveManager;

        public event Action<RecipeData> OnRecipeUnlocked;
        public event Action<IReadOnlyList<RecipeData>> OnRecipesRefreshed;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            EnsureSaveManager();
            BuildRecipeCollection();
            EnsureDefaultUnlocks();

            if (saveManager != null)
            {
                saveManager.OnRecipeUnlocked += HandleRecipeUnlocked;
            }
        }

        private void OnDestroy()
        {
            if (saveManager != null)
            {
                saveManager.OnRecipeUnlocked -= HandleRecipeUnlocked;
            }

            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void EnsureSaveManager()
        {
            saveManager = SaveManager.Instance ?? FindObjectOfType<SaveManager>();

            if (saveManager == null)
            {
                GameObject saveManagerObject = new GameObject("SaveManager");
                saveManager = saveManagerObject.AddComponent<SaveManager>();
            }
        }

        private void BuildRecipeCollection()
        {
            allRecipes.Clear();

            if (manualRecipes != null)
            {
                foreach (var recipe in manualRecipes)
                {
                    AddRecipeIfNeeded(recipe);
                }
            }

            if (autoPopulateFromResources)
            {
                RecipeData[] resourcesRecipes = Resources.LoadAll<RecipeData>(recipesResourceFolder);
                foreach (var recipe in resourcesRecipes)
                {
                    AddRecipeIfNeeded(recipe);
                }
            }

            allRecipes.Sort((a, b) =>
            {
                string nameA = a != null ? a.GetDisplayName() : string.Empty;
                string nameB = b != null ? b.GetDisplayName() : string.Empty;
                return string.Compare(nameA, nameB, StringComparison.OrdinalIgnoreCase);
            });

            OnRecipesRefreshed?.Invoke(allRecipes);
        }

        private void AddRecipeIfNeeded(RecipeData recipe)
        {
            if (recipe == null || allRecipes.Contains(recipe))
            {
                return;
            }

            allRecipes.Add(recipe);
        }

        private void EnsureDefaultUnlocks()
        {
            if (saveManager == null)
            {
                return;
            }

            foreach (var recipe in allRecipes)
            {
                if (recipe != null && recipe.unlockedByDefault)
                {
                    saveManager.EnsureRecipeUnlocked(recipe.GetId());
                }
            }
        }

        private void HandleRecipeUnlocked(string recipeId)
        {
            RecipeData recipe = GetRecipeById(recipeId);
            if (recipe != null)
            {
                OnRecipeUnlocked?.Invoke(recipe);
            }
        }

        public IReadOnlyList<RecipeData> GetAllRecipes()
        {
            return allRecipes;
        }

        public IReadOnlyList<RecipeData> GetUnlockedRecipes()
        {
            return allRecipes.Where(IsRecipeUnlocked).ToList();
        }

        public bool IsRecipeUnlocked(RecipeData recipe)
        {
            if (recipe == null)
            {
                return false;
            }

            if (saveManager == null)
            {
                return recipe.unlockedByDefault;
            }

            return saveManager.IsRecipeUnlocked(recipe.GetId());
        }

        public void UnlockRecipe(RecipeData recipe)
        {
            if (recipe == null || saveManager == null)
            {
                return;
            }

            if (!saveManager.IsRecipeUnlocked(recipe.GetId()))
            {
                saveManager.UnlockRecipe(recipe.GetId());
            }
        }

        public RecipeData GetRecipeById(string recipeId)
        {
            if (string.IsNullOrEmpty(recipeId))
            {
                return null;
            }

            return allRecipes.FirstOrDefault(r => r != null && r.GetId() == recipeId);
        }
    }
}

