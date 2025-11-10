using UnityEditor;
using UnityEngine;

namespace TinyChef.LevelEditor.Editor
{
    // Convert generator tooling into a custom inspector for TinyChef.Level
    [CustomEditor(typeof(TinyChef.Level))]
    public class LevelInspector : UnityEditor.Editor
    {
        private CounterProfile editorProfile;
        private Transform cachedRoot;
        private const string RootName = "CountersRoot (Generated)";
        private string PrefsKey => $"TinyChef_LevelEditor_Profile_{target.GetInstanceID()}";

        private void OnEnable()
        {
            if (editorProfile == null && EditorPrefs.HasKey(PrefsKey))
            {
                string guid = EditorPrefs.GetString(PrefsKey, string.Empty);
                if (!string.IsNullOrEmpty(guid))
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (!string.IsNullOrEmpty(path))
                    {
                        editorProfile = AssetDatabase.LoadAssetAtPath<CounterProfile>(path);
                    }
                }
            }
        }

        private void OnDisable()
        {
            PersistProfile();
        }

        private void PersistProfile()
        {
            if (editorProfile == null)
            {
                EditorPrefs.DeleteKey(PrefsKey);
                return;
            }
            string path = AssetDatabase.GetAssetPath(editorProfile);
            if (!string.IsNullOrEmpty(path))
            {
                string guid = AssetDatabase.AssetPathToGUID(path);
                EditorPrefs.SetString(PrefsKey, guid);
            }
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Counter Generator", EditorStyles.boldLabel);
            var newProfile = (CounterProfile)EditorGUILayout.ObjectField("Profile (Editor Only)", editorProfile, typeof(CounterProfile), false);
            if (newProfile != editorProfile)
            {
                editorProfile = newProfile;
                PersistProfile();
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Rebuild Counters"))
                {
                    RebuildCounters();
                }
                if (GUILayout.Button("Clear Counters"))
                {
                    ClearCounters();
                }
            }
        }

        private void RebuildCounters()
        {
            var level = target as TinyChef.Level;
            if (level == null) return;

            if (editorProfile == null)
            {
                Debug.LogWarning("Assign a CounterProfile in the Level inspector (Editor Only).");
                return;
            }

            var counters = level.GetCounters();
            int n = counters.Count;
            if (n < 3)
            {
                Debug.LogWarning("Need at least 3 counters in Level.counters to form a ring.");
                return;
            }

            // Determine a single CounterType to use: require homogenous list, else fallback to Basic
            TinyChef.CounterType selectedType = TinyChef.CounterType.Basic;
            if (n > 0)
            {
                selectedType = counters[0].counterType;
                for (int i = 1; i < counters.Count; i++)
                {
                    if (counters[i] == null) continue;
                    if (counters[i].counterType != selectedType)
                    {
                        Debug.LogWarning("Counters list has mixed CounterTypes. Using the first one's type for materials.");
                        break;
                    }
                }
            }

            ClearCounters();

            Transform root = GetOrCreateRoot(level.transform);
            var mesh = CounterMeshGenerator.BuildWedge(editorProfile, n);
            if (mesh == null)
            {
                Debug.LogWarning("Failed to build wedge mesh.");
                return;
            }

            // Save mesh asset for persistence/lightmapping/batching
            SaveMeshAsset(editorProfile, mesh, n);

            float step = 360f / n;
            float baseOffset = -90f;
            for (int i = 0; i < n; i++)
            {
                var go = new GameObject($"Counter_Wedge_{i:00}");
                go.transform.SetParent(root, false);
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.Euler(0f, baseOffset + step * i, 0f);
                go.transform.localScale = Vector3.one;

                var mf = go.AddComponent<MeshFilter>();
                mf.sharedMesh = mesh;
                var mr = go.AddComponent<MeshRenderer>();
                // Per-wedge materials based on that counter's type (fallback to first if index >= counters)
                var ct = (i < counters.Count && counters[i] != null) ? counters[i].counterType : selectedType;
                var mats = editorProfile.GetMaterials(ct);
                if (mats == null || mats.Count < 2)
                {
                    Debug.LogWarning($"Profile missing materials for {ct}. Using Basic mapping.");
                    mats = editorProfile.GetMaterials(TinyChef.CounterType.Basic);
                }
                mr.sharedMaterials = new Material[] { (mats != null && mats.Count > 0) ? mats[0] : null,
                                                       (mats != null && mats.Count > 1) ? mats[1] : null };
            }

            EditorUtility.SetDirty(level.gameObject);
        }

        private void SaveMeshAsset(CounterProfile profile, Mesh mesh, int n)
        {
            if (mesh == null) return;
            string folder = string.IsNullOrEmpty(profile.outputFolder) ? "Assets/Meshes/Counters" : profile.outputFolder;

            // Ensure folder exists
            if (!AssetDatabase.IsValidFolder(folder))
            {
                string[] parts = folder.Split('/');
                string path = "";
                for (int i = 0; i < parts.Length; i++)
                {
                    if (string.IsNullOrEmpty(parts[i])) continue;
                    if (i == 0)
                    {
                        path = parts[i];
                        continue;
                    }
                    string parent = path;
                    path = $"{parent}/{parts[i]}";
                    if (!AssetDatabase.IsValidFolder(path))
                    {
                        AssetDatabase.CreateFolder(parent, parts[i]);
                    }
                }
            }

            string name = $"Counter_Wedge_N{n}_IR{profile.innerRadius:0.###}_OR{profile.outerRadius:0.###}.asset";
            string assetPath = $"{folder}/{name}";
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(Object.Instantiate(mesh), assetPath);
            }
            else
            {
                // Overwrite by copying data
                existing.Clear();
                existing.vertices = mesh.vertices;
                existing.triangles = mesh.triangles;
                existing.subMeshCount = mesh.subMeshCount;
                for (int i = 0; i < mesh.subMeshCount; i++)
                {
                    existing.SetTriangles(mesh.GetTriangles(i), i);
                }
                existing.uv = mesh.uv;
                existing.RecalculateBounds();
                existing.RecalculateNormals();
                EditorUtility.SetDirty(existing);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void ClearCounters()
        {
            var level = target as TinyChef.Level;
            if (level == null) return;
            Transform root = FindRoot(level.transform);
            if (root == null) return;
            for (int i = root.childCount - 1; i >= 0; i--)
            {
                var child = root.GetChild(i);
                Object.DestroyImmediate(child.gameObject);
            }
        }

        private Transform FindRoot(Transform parent)
        {
            if (cachedRoot != null) return cachedRoot;
            var t = parent.Find(RootName);
            cachedRoot = t;
            return cachedRoot;
        }

        private Transform GetOrCreateRoot(Transform parent)
        {
            var root = FindRoot(parent);
            if (root == null)
            {
                var go = new GameObject(RootName);
                go.transform.SetParent(parent, false);
                root = go.transform;
                cachedRoot = root;
            }
            return root;
        }
    }
    }


