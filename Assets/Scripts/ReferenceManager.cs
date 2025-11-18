using UnityEngine;
using UnityEngine.Rendering;

namespace TinyChef
{
    /// <summary>
    /// Central singleton manager for all important scene and game references.
    /// </summary>
    public class ReferenceManager : MonoBehaviour
    {
        private static ReferenceManager _instance;

        public static ReferenceManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<ReferenceManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("ReferenceManager");
                        _instance = go.AddComponent<ReferenceManager>();
                        DontDestroyOnLoad(go);
                    }
                }

                return _instance;
            }
        }

        [Header("Game Settings")] [SerializeField]
        private GameSettings gameSettings;

        [Header("Scene References")] [SerializeField]
        private Light directionalLight;

        [SerializeField] private Camera mainCamera;
        [SerializeField] private Chef chef;
        [SerializeField] private LevelController levelController;

        [Header("Managers")] [SerializeField] private SaveManager saveManager;
        [SerializeField] private RecipeController recipeController;
        [SerializeField] private InputController inputController;
        [SerializeField] private OrderManager orderManager;

        // Public accessors
        public GameSettings GameSettings => gameSettings;
        public Light DirectionalLight => directionalLight;
        public Camera MainCamera => mainCamera;
        public Chef Chef => chef;
        public LevelController LevelController => levelController;
        public SaveManager SaveManager => saveManager;
        public RecipeController RecipeController => recipeController;
        public InputController InputController => inputController;
        public OrderManager OrderManager => orderManager;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeReferences();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void InitializeReferences()
        {
            // Auto-find references if not set
            if (mainCamera == null)
                mainCamera = Camera.main;

            if (directionalLight == null)
                directionalLight = FindObjectOfType<Light>();

            if (saveManager == null)
                saveManager = FindObjectOfType<SaveManager>();

            if (recipeController == null)
                recipeController = FindObjectOfType<RecipeController>();

            if (inputController == null)
                inputController = FindObjectOfType<InputController>();
        }

        /// <summary>
        /// Updates scene references that change per scene/level
        /// </summary>
        public void RefreshSceneReferences()
        {
            if (chef == null)
                chef = FindObjectOfType<Chef>();

            if (levelController == null)
                levelController = FindObjectOfType<LevelController>();

            if (orderManager == null)
                orderManager = FindObjectOfType<OrderManager>();

            if (directionalLight == null)
                directionalLight = FindObjectOfType<Light>();

            if (mainCamera == null)
                mainCamera = Camera.main;
        }


        /// <summary>
        /// Sets a specific reference (useful for dynamic assignments)
        /// </summary>
        public void SetChef(Chef newChef) => chef = newChef;

        public void SetLevelController(LevelController controller) => levelController = controller;
        public void SetOrderManager(OrderManager manager) => orderManager = manager;
        public void SetDirectionalLight(Light light) => directionalLight = light;
        public void SetMainCamera(Camera cam) => mainCamera = cam;
    }
}