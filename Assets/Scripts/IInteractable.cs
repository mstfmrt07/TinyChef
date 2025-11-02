using UnityEngine;

namespace TinyChef
{
    public interface IInteractable
    {
        void Select();
        void Deselect();
        void Interact();
    }
}