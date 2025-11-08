using UnityEngine;

namespace TinyChef
{
    /// <summary>
    /// Interface for items that can be picked up and placed on counters
    /// </summary>
    public interface IItem
    {
        Transform transform { get; }
        GameObject gameObject { get; }
    }
}
