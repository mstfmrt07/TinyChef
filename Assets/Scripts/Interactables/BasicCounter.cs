using UnityEngine;

namespace TinyChef
{
    /// <summary>
    /// A basic counter that can hold items but doesn't process them
    /// </summary>
    public class BasicCounter : BaseCounter
    {
        private void Awake()
        {
            counterType = CounterType.Basic;
            if (itemPlacePoint == null)
            {
                itemPlacePoint = transform;
            }
        }

        protected override bool CanProcess(Ingredient ingredient)
        {
            // Basic counters can't process items
            return false;
        }
    }
}
