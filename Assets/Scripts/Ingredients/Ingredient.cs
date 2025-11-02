using UnityEngine;

namespace TinyChef
{
    public class Ingredient : MonoBehaviour
    {
        public IngredientData data;
        public GameObject rawModel;
        public GameObject processedModel;
        public GameObject cookedModel;

        private IngredientState state;
        public IngredientState State => state;

        private void Awake()
        {
            state = IngredientState.Raw;
            rawModel.SetActive(true);
            processedModel.SetActive(false);
            cookedModel.SetActive(false);
        }

        public void Process()
        {
            state = IngredientState.Processed;
            rawModel.SetActive(false);
            processedModel.SetActive(true);
        }

        public void Cook()
        {
            state = IngredientState.Cooked;
            processedModel.SetActive(false);
            cookedModel.SetActive(true);
        }
    }
}