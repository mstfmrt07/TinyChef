using UnityEngine;

namespace TinyChef
{
    public class HighlightObject : MonoBehaviour
    {
        public GameObject[] graphics;
        public int highlightedLayer;
        public int notHighlightedLayer;

        public void Select()
        {
            foreach (var obj in graphics)
            {
                obj.layer = highlightedLayer;
            }
        }

        public void Deselect()
        {
            foreach (var obj in graphics)
            {
                obj.layer = notHighlightedLayer;
            }
        }
    }
}