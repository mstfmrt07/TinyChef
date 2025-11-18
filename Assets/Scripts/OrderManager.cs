using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TinyChef
{
    public class OrderManager : MonoBehaviour
    {
        [Header("Order Settings")]
        public LevelData currentLevelData;
        public float orderGenerationInterval = 10f;
        public int maxActiveOrders = 5;
        public int baseOrderScore = 100;
        public int penaltyScore = -50;

        [Header("UI Reference")]
        public Transform orderUIContainer;
        public GameObject orderUIPrefab;

        private List<Order> activeOrders = new List<Order>();
        private List<OrderUI> orderUIElements = new List<OrderUI>();
        private float orderGenerationTimer = 0f;
        private int currentOrderIndex = 0;
        private int totalScore = 0;
        private LevelController levelController;

        // Events
        public Action<int> OnScoreChanged;
        public Action<Order> OnOrderCompleted;
        public Action<Order> OnOrderFailed;
        public Action<Order> OnWrongOrderServed;

        private void Awake()
        {
            levelController = ReferenceManager.Instance.LevelController ?? FindObjectOfType<LevelController>();
            if (levelController != null)
            {
                levelController.OnLevelStarted += OnLevelStarted;
            }
        }

        private void Start()
        {
            if (currentLevelData == null)
            {
                Debug.LogWarning("No LevelData assigned to OrderManager!");
            }
        }

        private void Update()
        {
            // Only update orders if level is active
            if (levelController != null && !levelController.IsLevelActive)
            {
                return;
            }

            // Update active orders
            for (int i = activeOrders.Count - 1; i >= 0; i--)
            {
                Order order = activeOrders[i];
                order.Update(Time.deltaTime);

                // Check if order expired
                if (order.IsExpired)
                {
                    HandleOrderExpired(order);
                }
            }

            // Generate new orders
            if (currentLevelData != null && currentLevelData.availableOrders != null && currentLevelData.availableOrders.Count > 0)
            {
                orderGenerationTimer += Time.deltaTime;
                if (orderGenerationTimer >= currentLevelData.durationBetweenOrders && activeOrders.Count < maxActiveOrders)
                {
                    GenerateNewOrder();
                    orderGenerationTimer = 0f;
                }
            }
        }

        public void SetLevelData(LevelData levelData)
        {
            currentLevelData = levelData;
            ClearAllOrders();
            orderGenerationTimer = 0f;
            totalScore = 0;
            OnScoreChanged?.Invoke(totalScore);
        }

        private void GenerateNewOrder()
        {
            if (currentLevelData == null || currentLevelData.availableOrders == null || currentLevelData.availableOrders.Count == 0)
                return;

            // Get order template (cycle through available orders)
            Order template = currentLevelData.availableOrders[currentOrderIndex];
            currentOrderIndex = (currentOrderIndex + 1) % currentLevelData.availableOrders.Count;

            // Create new order instance
            Order newOrder = new Order
            {
                recipe = template.recipe,
                duration = template.duration
            };

            newOrder.Start();
            activeOrders.Add(newOrder);

            // Create UI for order
            CreateOrderUI(newOrder);
        }

        private void CreateOrderUI(Order order)
        {
            if (orderUIPrefab == null || orderUIContainer == null) return;

            GameObject uiObject = Instantiate(orderUIPrefab, orderUIContainer);
            OrderUI orderUI = uiObject.GetComponent<OrderUI>();
            if (orderUI != null)
            {
                orderUI.SetOrder(order);
                orderUIElements.Add(orderUI);
            }
        }

        public bool TryServeOrder(IItem dish)
        {
            if (dish == null)
            {
                Debug.Log("OrderManager: Dish is null");
                return false;
            }

            Debug.Log($"OrderManager: TryServeOrder called with {dish.gameObject.name}");

            // Check if dish is a plate with ingredients
            List<Ingredient> dishIngredients = ExtractIngredientsFromDish(dish);

            if (dishIngredients == null || dishIngredients.Count == 0)
            {
                Debug.Log("OrderManager: No ingredients found on plate");
                // Wrong order - not a valid dish
                ApplyWrongOrderPenalty();
                return false;
            }

            Debug.Log($"OrderManager: Found {dishIngredients.Count} ingredients on plate");
            foreach (var ing in dishIngredients)
            {
                Debug.Log($"  - {ing.data.name}: State={ing.State}, CookingType={ing.CookingType}");
            }

            Debug.Log($"OrderManager: Active orders count: {activeOrders.Count}");

            // Try to find matching order
            Order matchingOrder = activeOrders.FirstOrDefault(order =>
                order.recipe.IsSatisfied(dishIngredients) && !order.IsExpired);

            if (matchingOrder != null)
            {
                // Correct order!
                Debug.Log($"OrderManager: Found matching order for recipe: {matchingOrder.recipe.name}");
                matchingOrder.Complete();
                int score = matchingOrder.CalculateScore(baseOrderScore);
                AddScore(score);
                RemoveOrder(matchingOrder);
                OnOrderCompleted?.Invoke(matchingOrder);
                return true;
            }
            else
            {
                Debug.Log("OrderManager: No matching order found");
                // Log what each active order needs
                foreach (var order in activeOrders)
                {
                    Debug.Log($"  Order {order.recipe.name} requires:");
                    foreach (var req in order.recipe.requiredIngredients)
                    {
                        Debug.Log($"    - {req.item.name}: {req.targetState}, {req.targetCookingType}");
                    }
                }
                
                // Wrong order - doesn't match any active order
                ApplyWrongOrderPenalty();
                OnWrongOrderServed?.Invoke(null);
                return false;
            }
        }

        private List<Ingredient> ExtractIngredientsFromDish(IItem dish)
        {
            if (dish == null) return null;

            // Check if the dish is a plate
            Plate plate = dish.gameObject.GetComponent<Plate>();
            if (plate != null)
            {
                return plate.Ingredients;
            }

            // If not a plate, check if it's a single ingredient
            if (dish is Ingredient ingredient)
            {
                return new List<Ingredient> { ingredient };
            }

            return null;
        }

        private void HandleOrderExpired(Order order)
        {
            RemoveOrder(order);
            AddScore(penaltyScore);
            OnOrderFailed?.Invoke(order);
        }

        private void RemoveOrder(Order order)
        {
            int index = activeOrders.IndexOf(order);
            if (index >= 0 && index < orderUIElements.Count)
            {
                if (orderUIElements[index] != null)
                {
                    Destroy(orderUIElements[index].gameObject);
                }
                orderUIElements.RemoveAt(index);
            }

            activeOrders.Remove(order);
        }

        private void ApplyWrongOrderPenalty()
        {
            AddScore(penaltyScore);
        }

        private void AddScore(int points)
        {
            totalScore += points;
            totalScore = Mathf.Max(0, totalScore); // Don't go below 0
            OnScoreChanged?.Invoke(totalScore);

            // Check level completion
            if (levelController != null)
            {
                levelController.CheckLevelCompletion(totalScore);
            }
        }

        private void OnLevelStarted()
        {
            GenerateNewOrder();
        }

        private void ClearAllOrders()
        {
            foreach (var orderUI in orderUIElements)
            {
                if (orderUI != null)
                {
                    Destroy(orderUI.gameObject);
                }
            }
            orderUIElements.Clear();
            activeOrders.Clear();
        }

        private void OnDestroy()
        {
            if (levelController != null)
            {
                levelController.OnLevelStarted -= OnLevelStarted;
            }
        }

        public int GetTotalScore() => totalScore;
        public int GetActiveOrderCount() => activeOrders.Count;
    }
}
