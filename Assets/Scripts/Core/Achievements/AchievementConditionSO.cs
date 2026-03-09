using System;
using UnityEngine;

namespace Game.Achievements
{
    public abstract class AchievementConditionSO : ScriptableObject
    {
        [SerializeField] private string conditionId;

        public string ConditionId => conditionId;

        public abstract bool TryApply(AchievementEvent evt, AchievementProgress progress);
        public abstract bool IsSatisfied(AchievementProgress progress);
        public abstract float GetProgress01(AchievementProgress progress);

        protected ConditionProgress GetOrCreateProgress(AchievementProgress progress)
        {
            if (progress == null)
                throw new ArgumentNullException(nameof(progress));
            return progress.GetOrCreateCondition(conditionId);
        }

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(conditionId))
            {
                conditionId = Guid.NewGuid().ToString("N");
            }
        }
    }
}
