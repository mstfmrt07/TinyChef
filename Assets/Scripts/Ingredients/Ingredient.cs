using UnityEngine;

namespace TinyChef
{
    public class Ingredient : MonoBehaviour, IItem
    {
        public IngredientData data;
        public GameObject rawModel;
        public GameObject processedModel;
        public GameObject cookedModel;

        private IngredientState state;
        public IngredientState State => state;

        private CookingType cookingType = CookingType.Raw;
        public CookingType CookingType => cookingType;

        private void Awake()
        {
            state = IngredientState.Raw;
            cookingType = CookingType.Raw;
            rawModel?.SetActive(true);
            processedModel?.SetActive(false);
            cookedModel?.SetActive(false);
        }

        public void Process()
        {
            if (state == IngredientState.Raw)
            {
                state = IngredientState.Processed;
                rawModel?.SetActive(false);
                processedModel?.SetActive(true);
            }
        }

        public void Cook(CookingType type)
        {
            if (state != IngredientState.Cooked)
            {
                state = IngredientState.Cooked;
                cookingType = type;
                rawModel?.SetActive(false);
                processedModel?.SetActive(false);
                cookedModel?.SetActive(true);
            }
        }
    }
}