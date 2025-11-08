using System;
using UnityEngine;

namespace TinyChef
{
    public class ChoppingBoard : BaseCounter
    {
        public Action<Ingredient> OnItemProcessed;

        protected override void ExecuteProcess()
        {
            if (currentItem is Ingredient ingredient)
            {
                ingredient.Process();
                OnItemProcessed?.Invoke(ingredient);
            }
        }

        private void Awake()
        {
            if (counterType == CounterType.Basic)
            {
                counterType = CounterType.CuttingBoard;
            }
            if (itemPlacePoint == null && transform.childCount > 0)
            {
                itemPlacePoint = transform;
            }
        }
    }
}