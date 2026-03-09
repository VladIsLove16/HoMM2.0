using UnityEngine;

namespace Game.Achievements
{
    [CreateAssetMenu(menuName = "Game/Achievements/Conditions/Event", fileName = "AchievementEventCondition")]
    public sealed class AchievementEventConditionSO : AchievementConditionSO
    {
        [SerializeField] private string eventId;

        public string EventId => eventId;

        public override bool TryApply(AchievementEvent evt, AchievementProgress progress)
        {
            if (string.IsNullOrWhiteSpace(eventId))
                return false;
            if (!evt.Matches(eventId))
                return false;

            var condition = GetOrCreateProgress(progress);
            if (condition.IsCompleted)
                return false;

            condition.CurrentValue = 1;
            condition.IsCompleted = true;
            return true;
        }

        public override bool IsSatisfied(AchievementProgress progress)
        {
            return GetOrCreateProgress(progress).IsCompleted;
        }

        public override float GetProgress01(AchievementProgress progress)
        {
            return IsSatisfied(progress) ? 1f : 0f;
        }
    }
}
