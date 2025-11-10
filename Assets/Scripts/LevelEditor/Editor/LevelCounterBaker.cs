using UnityEditor;
using UnityEngine;

namespace TinyChef.LevelEditor.Editor
{
    public static class LevelCounterBaker
    {
        [MenuItem("TinyChef/Counters/Rebuild Counters On Selected Level")]
        private static void RebuildOnSelected()
        {
            var go = Selection.activeGameObject;
            if (go == null)
            {
                Debug.LogWarning("Select a Level GameObject in the hierarchy.");
                return;
            }
            var level = go.GetComponent<TinyChef.Level>();
            if (level == null)
            {
                Debug.LogWarning("Selected object does not have a Level component.");
                return;
            }
            level.SendMessage("RebuildCounters", SendMessageOptions.DontRequireReceiver);
            EditorUtility.SetDirty(level);
        }

        [MenuItem("TinyChef/Counters/Clear Counters On Selected Level")]
        private static void ClearOnSelected()
        {
            var go = Selection.activeGameObject;
            if (go == null)
            {
                Debug.LogWarning("Select a Level GameObject in the hierarchy.");
                return;
            }
            var level = go.GetComponent<TinyChef.Level>();
            if (level == null)
            {
                Debug.LogWarning("Selected object does not have a Level component.");
                return;
            }
            level.SendMessage("ClearCounters", SendMessageOptions.DontRequireReceiver);
            EditorUtility.SetDirty(level);
        }
    }
}


