using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Achievements
{
    public sealed class AchievementProgress
    {
        private readonly Dictionary<string, ConditionProgress> _conditions =
            new Dictionary<string, ConditionProgress>(StringComparer.OrdinalIgnoreCase);

        public AchievementProgress(string id)
        {
            Id = id;
        }

        public string Id { get; }
        public bool IsUnlocked { get; set; }
        public DateTime? UnlockDateUtc { get; set; }

        public IReadOnlyCollection<ConditionProgress> Conditions => _conditions.Values;

        public ConditionProgress GetOrCreateCondition(string conditionId)
        {
            if (string.IsNullOrWhiteSpace(conditionId))
                conditionId = "condition";

            if (!_conditions.TryGetValue(conditionId, out var progress))
            {
                progress = new ConditionProgress(conditionId);
                _conditions[conditionId] = progress;
            }
            return progress;
        }

        public void EnsureConditions(IEnumerable<AchievementConditionSO> conditions)
        {
            if (conditions == null)
                return;

            foreach (var condition in conditions)
            {
                if (condition == null)
                    continue;
                GetOrCreateCondition(condition.ConditionId);
            }
        }

        public void Reset()
        {
            IsUnlocked = false;
            UnlockDateUtc = null;
            foreach (var condition in _conditions.Values)
            {
                condition.Reset();
            }
        }

        public float GetProgress01(AchievementDefinition definition)
        {
            if (definition == null || definition.Conditions == null || definition.Conditions.Count == 0)
                return 0f;

            var values = new List<float>();
            foreach (var condition in definition.Conditions)
            {
                if (condition == null)
                    continue;
                values.Add(condition.GetProgress01(this));
            }

            if (values.Count == 0)
                return 0f;

            if (definition.CompletionMode == AchievementCompletionMode.Any)
                return values.Max();

            var sum = values.Sum();
            return sum / values.Count;
        }
    }
}
