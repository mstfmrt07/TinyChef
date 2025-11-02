using System;
using UnityEngine;

namespace TinyChef
{
    [System.Serializable]
    public class Order
    {
        public RecipeData recipe;
        
        // Backward compatibility: use 'duration' to match existing level assets
        public float duration;
        
        // Computed properties
        public float timeLimit => duration;
        public float timeRemaining;
        private OrderState state = OrderState.Idle;
        private DateTime startTime;

        public OrderState State => state;
        public float TimeRemaining => timeRemaining;
        public float TimeElapsed => timeLimit - timeRemaining;
        public bool IsExpired => state == OrderState.Expired;
        public bool IsFinished => state == OrderState.Finished;

        public void Start()
        {
            state = OrderState.InProgress;
            timeRemaining = timeLimit;
            startTime = DateTime.Now;
        }

        public void Update(float deltaTime)
        {
            if (state != OrderState.InProgress) return;

            timeRemaining -= deltaTime;
            if (timeRemaining <= 0f)
            {
                timeRemaining = 0f;
                state = OrderState.Expired;
            }
        }

        public void Complete()
        {
            if (state == OrderState.InProgress)
            {
                state = OrderState.Finished;
            }
        }

        public int CalculateScore(int baseScore)
        {
            if (state != OrderState.Finished) return 0;

            // Higher score for faster completion (more time remaining)
            float timeBonus = (timeRemaining / timeLimit) * baseScore;
            return Mathf.RoundToInt(baseScore + timeBonus);
        }
    }

    public enum OrderState
    {
        Idle,
        InProgress,
        Expired,
        Finished
    }
}