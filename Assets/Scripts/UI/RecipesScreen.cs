using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TinyChef
{
    public class RecipesScreen : MonoBehaviour
    {
        [Header("General Settings")]
        [SerializeField] private GameObject screenRoot;
        [SerializeField] private int recipesPerPage = 6;
        [SerializeField] private string emptySlotText = string.Empty;
        [SerializeField] private string noSelectionMessage = "Select a recipe to view its details.";

        [Header("Left Page")]
        [SerializeField] private Transform recipeListContainer;
        [SerializeField] private RecipeListItemUI recipeListItemPrefab;
        [SerializeField] private Button previousPageButton;
        [SerializeField] private Button nextPageButton;
        [SerializeField] private TextMeshProUGUI pageIndicatorText;

        [Header("Right Page")]
        [SerializeField] private TextMeshProUGUI recipeTitleText;
        [SerializeField] private TextMeshProUGUI recipeDescriptionText;
        [SerializeField] private Image recipeImage;
        [SerializeField] private Transform ingredientsContainer;
        [SerializeField] private RecipeIngredientUI ingredientEntryPrefab;

        [Header("Icon Lookups")]
        [SerializeField] private List<CookingTypeIconMapping> cookingTypeIconMappings = new List<CookingTypeIconMapping>();

        [System.Serializable]
        public class CookingTypeIconMapping
        {
            public CookingType cookingType;
            public Sprite icon;
        }

        private readonly Dictionary<CookingType, Sprite> cookingTypeIconLookup = new Dictionary<CookingType, Sprite>();
        private readonly List<RecipeListItemUI> listItemPool = new List<RecipeListItemUI>();

        private RecipeController recipeController;
        private List<RecipeData> orderedRecipes = new List<RecipeData>();
        private RecipeData selectedRecipe;
        private int currentPage;

        private int TotalPages
        {
            get
            {
                if (orderedRecipes.Count == 0 || recipesPerPage <= 0)
                {
                    return 0;
                }

                return Mathf.CeilToInt(orderedRecipes.Count / (float)recipesPerPage);
            }
        }

        private void Awake()
        {
            BuildIconLookups();
            SetupButtons();

            if (screenRoot != null)
            {
                screenRoot.SetActive(false);
            }
        }

        private void OnEnable()
        {
            recipeController = RecipeController.Instance ?? FindObjectOfType<RecipeController>();
            if (recipeController == null)
            {
                Debug.LogWarning("RecipesScreen could not find a RecipeController in the scene.");
                return;
            }

            recipeController.OnRecipeUnlocked += HandleRecipeUnlocked;
            RefreshRecipes();
        }

        private void OnDisable()
        {
            if (recipeController != null)
            {
                recipeController.OnRecipeUnlocked -= HandleRecipeUnlocked;
            }
        }

        private void SetupButtons()
        {
            if (previousPageButton != null)
            {
                previousPageButton.onClick.AddListener(GoToPreviousPage);
            }

            if (nextPageButton != null)
            {
                nextPageButton.onClick.AddListener(GoToNextPage);
            }
        }

        private void BuildIconLookups()
        {
            cookingTypeIconLookup.Clear();
            foreach (var mapping in cookingTypeIconMappings)
            {
                if (mapping != null && mapping.icon != null)
                {
                    cookingTypeIconLookup[mapping.cookingType] = mapping.icon;
                }
            }
        }

        public void Open()
        {
            if (screenRoot != null)
            {
                screenRoot.SetActive(true);
            }

            RefreshRecipes();
        }

        public void Close()
        {
            if (screenRoot != null)
            {
                screenRoot.SetActive(false);
            }
        }

        private void RefreshRecipes()
        {
            if (recipeController == null)
            {
                return;
            }

            orderedRecipes = recipeController
                .GetAllRecipes()
                .Where(r => r != null)
                .OrderBy(r => r.GetDisplayName())
                .ToList();

            currentPage = Mathf.Clamp(currentPage, 0, Mathf.Max(0, TotalPages - 1));

            PopulateRecipeList();

            if (selectedRecipe == null || !recipeController.IsRecipeUnlocked(selectedRecipe))
            {
                selectedRecipe = orderedRecipes.FirstOrDefault(recipeController.IsRecipeUnlocked);
            }

            UpdateNavigationButtons();
            UpdateRightPage();
        }

        private void PopulateRecipeList()
        {
            if (recipeListItemPrefab == null || recipeListContainer == null || recipesPerPage <= 0)
            {
                return;
            }

            int startIndex = currentPage * recipesPerPage;

            for (int i = 0; i < recipesPerPage; i++)
            {
                RecipeListItemUI item = GetOrCreateListItem(i);
                int recipeIndex = startIndex + i;

                if (recipeIndex < orderedRecipes.Count)
                {
                    var recipe = orderedRecipes[recipeIndex];
                    bool unlocked = recipeController.IsRecipeUnlocked(recipe);
                    item.gameObject.SetActive(true);
                    item.SetRecipe(recipe, unlocked, HandleRecipeSelected);
                    item.SetSelected(recipe == selectedRecipe);
                }
                else
                {
                    item.gameObject.SetActive(!string.IsNullOrEmpty(emptySlotText));
                    item.SetPlaceholder(emptySlotText);
                    item.SetSelected(false);
                }
            }
        }

        private RecipeListItemUI GetOrCreateListItem(int index)
        {
            while (listItemPool.Count <= index)
            {
                RecipeListItemUI newItem = Instantiate(recipeListItemPrefab, recipeListContainer);
                listItemPool.Add(newItem);
            }

            return listItemPool[index];
        }

        private void HandleRecipeSelected(RecipeData recipe)
        {
            selectedRecipe = recipe;

            foreach (var item in listItemPool)
            {
                item.SetSelected(item.Recipe == selectedRecipe);
            }

            UpdateRightPage();
        }

        private void HandleRecipeUnlocked(RecipeData recipe)
        {
            if (recipe == null)
            {
                return;
            }

            RefreshRecipes();
        }

        private void UpdateRightPage()
        {
            if (selectedRecipe == null || !recipeController.IsRecipeUnlocked(selectedRecipe))
            {
                ShowEmptyState();
                return;
            }

            if (recipeTitleText != null)
            {
                recipeTitleText.text = selectedRecipe.GetDisplayName();
            }

            if (recipeDescriptionText != null)
            {
                if (string.IsNullOrWhiteSpace(selectedRecipe.description))
                {
                    recipeDescriptionText.text = "No description available for this recipe yet.";
                }
                else
                {
                    recipeDescriptionText.text = selectedRecipe.description;
                }
            }

            if (recipeImage != null)
            {
                Sprite displaySprite = selectedRecipe.GetDisplaySprite();
                recipeImage.sprite = displaySprite;
                recipeImage.enabled = displaySprite != null;
            }

            PopulateIngredients();
        }

        private void PopulateIngredients()
        {
            if (ingredientsContainer == null)
            {
                return;
            }

            foreach (Transform child in ingredientsContainer)
            {
                Destroy(child.gameObject);
            }

            if (ingredientEntryPrefab == null || selectedRecipe == null || selectedRecipe.requiredIngredients == null)
            {
                return;
            }

            var groupedIngredients = selectedRecipe.requiredIngredients
                .GroupBy(def => (def.item, def.targetState, def.targetCookingType));

            foreach (var group in groupedIngredients)
            {
                IngredientDefinition representative = group.First();
                int count = group.Count();

                RecipeIngredientUI entry = Instantiate(ingredientEntryPrefab, ingredientsContainer);
                Sprite ingredientSprite = representative.item != null ? representative.item.icon : null;
                Sprite cookingSprite = cookingTypeIconLookup.TryGetValue(representative.targetCookingType, out var cIcon) ? cIcon : null;

                entry.SetData(representative, ingredientSprite, cookingSprite, count);
            }
        }

        private void ShowEmptyState()
        {
            if (recipeTitleText != null)
            {
                recipeTitleText.text = "Recipes";
            }

            if (recipeDescriptionText != null)
            {
                recipeDescriptionText.text = noSelectionMessage;
            }

            if (recipeImage != null)
            {
                recipeImage.sprite = null;
                recipeImage.enabled = false;
            }

            if (ingredientsContainer != null)
            {
                foreach (Transform child in ingredientsContainer)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        private void UpdateNavigationButtons()
        {
            int totalPages = Mathf.Max(1, TotalPages);

            if (pageIndicatorText != null)
            {
                if (TotalPages == 0)
                {
                    pageIndicatorText.text = "0 / 0";
                }
                else
                {
                    pageIndicatorText.text = $"{currentPage + 1} / {totalPages}";
                }
            }

            if (previousPageButton != null)
            {
                previousPageButton.interactable = currentPage > 0;
            }

            if (nextPageButton != null)
            {
                nextPageButton.interactable = TotalPages > 0 && currentPage < TotalPages - 1;
            }
        }

        private void GoToPreviousPage()
        {
            if (currentPage <= 0)
            {
                return;
            }

            currentPage--;
            PopulateRecipeList();
            UpdateNavigationButtons();
        }

        private void GoToNextPage()
        {
            if (TotalPages == 0 || currentPage >= TotalPages - 1)
            {
                return;
            }

            currentPage++;
            PopulateRecipeList();
            UpdateNavigationButtons();
        }
    }
}

