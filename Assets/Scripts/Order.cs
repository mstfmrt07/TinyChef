using UnityEngine;

namespace TinyChef
{
    [System.Serializable]
    public class Order
    {
        public RecipeData recipe;
        public float duration;
        private OrderState state;

        public OrderState State => state;
    }

    public enum OrderState
    {
        Idle,
        InProgress,
        Expired,
        Finished
    }
}