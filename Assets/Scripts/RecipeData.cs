using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace TinyChef
{
    [CreateAssetMenu(menuName = "Recipes/Recipe", fileName = "New Recipe")]
    public class RecipeData : ScriptableObject
    {
        public new string name;
        public Sprite icon;
        public List<IngredientDefinition> ingredients;

        public bool IsSatisfied(List<IngredientData> ingredients)
        {
            throw new NotImplementedException();
        }
    }
}