using UnityEngine;

namespace TinyChef
{
    [CreateAssetMenu(menuName = "Recipes/Ingredient", fileName = "New Ingredient")]
    public class IngredientData : ScriptableObject
    {
        public new string name;
        public Sprite icon;
        public Ingredient prefab;
    }
}