using System;
using UnityEngine;

namespace TinyChef
{
    public class StoveCounter : BaseCounter
    {
        [Header("Stove Settings")]
        public CookingType defaultCookingType = CookingType.Boiled;

        public Action<Ingredient> OnItemCooked;

        protected override void ExecuteProcess()
        {
            if (currentItem != null)
            {
                currentItem.Cook(defaultCookingType);
                OnItemCooked?.Invoke(currentItem);
            }
        }

        private void Awake()
        {
            counterType = CounterType.Stove;
            if (itemPlacePoint == null)
            {
                itemPlacePoint = transform;
            }
        }
    }
}
