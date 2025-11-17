using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TinyChef
{
    public class RecipeIngredientUI : MonoBehaviour
    {
        [SerializeField] private Image ingredientIcon;
        [SerializeField] private TextMeshProUGUI countText;
        [SerializeField] private Image cookingTypeIcon;

        public void SetData(IngredientDefinition definition, Sprite ingredientSprite, Sprite cookingSprite, int count)
        {
            if (ingredientIcon != null)
            {
                ingredientIcon.sprite = ingredientSprite;
                ingredientIcon.enabled = ingredientSprite != null;
            }

            if (countText != null)
            {
                countText.text = $"x{Mathf.Max(1, count)}";
            }

            if (cookingTypeIcon != null)
            {
                cookingTypeIcon.sprite = cookingSprite;
                cookingTypeIcon.gameObject.SetActive(cookingSprite != null);
            }
        }
    }
}

