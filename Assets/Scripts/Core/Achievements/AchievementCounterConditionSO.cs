using UnityEngine;

namespace Game.Achievements
{
    [CreateAssetMenu(menuName = "Game/Achievements/Conditions/Counter", fileName = "AchievementCounterCondition")]
    public sealed class AchievementCounterConditionSO : AchievementConditionSO
    {
        [SerializeField] private string eventId;
        [SerializeField] private int targetValue = 1;
        [SerializeField] private bool useMaxValueFromEvent;

        public string EventId => eventId;
        public int TargetValue => targetValue;
        public bool UseMaxValueFromEvent => useMaxValueFromEvent;

        public override bool TryApply(AchievementEvent evt, AchievementProgress progress)
        {
            if (string.IsNullOrWhiteSpace(eventId))
                return false;
            if (!evt.Matches(eventId))
                return false;

            var condition = GetOrCreateProgress(progress);
            if (condition.IsCompleted)
                return false;

            var amount = Mathf.Max(0, evt.Amount);
            if (useMaxValueFromEvent)
            {
                if (amount <= condition.CurrentValue)
                    return false;
                condition.CurrentValue = amount;
            }
            else
            {
                if (amount == 0)
                    amount = 1;
                condition.CurrentValue += amount;
            }

            if (TargetValue <= 0 || condition.CurrentValue >= TargetValue)
            {
                condition.IsCompleted = true;
            }
            return true;
        }

        public override bool IsSatisfied(AchievementProgress progress)
        {
            return GetOrCreateProgress(progress).IsCompleted;
        }

        public override float GetProgress01(AchievementProgress progress)
        {
            if (TargetValue <= 0)
                return 1f;
            var current = GetOrCreateProgress(progress).CurrentValue;
            return Mathf.Clamp01(current / (float)TargetValue);
        }
    }
}
