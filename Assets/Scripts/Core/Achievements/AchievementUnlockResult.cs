using System;

namespace Game.Achievements
{
    public readonly struct AchievementUnlockResult
    {
        public AchievementUnlockResult(AchievementDefinition definition, AchievementProgress progress)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            Progress = progress ?? throw new ArgumentNullException(nameof(progress));
        }

        public AchievementDefinition Definition { get; }
        public AchievementProgress Progress { get; }
        public int RewardCurrency => Definition.RewardCurrencyAmount;
    }
}
