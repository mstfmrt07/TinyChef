using System;
using UnityEngine;

namespace TinyChef
{
    public class ChoppingBoard : BaseCounter
    {
        public Action<Ingredient> OnItemProcessed;

        protected override void ExecuteProcess()
        {
            base.ExecuteProcess();
            OnItemProcessed?.Invoke(currentItem);
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