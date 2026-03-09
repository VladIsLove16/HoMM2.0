using System;
using System.Collections.Generic;

namespace Game.Achievements
{
    [Serializable]
    public sealed class AchievementProgressSnapshot
    {
        public string Id;
        public bool IsUnlocked;
        public long UnlockUtcTicks;
        public List<ConditionProgressSnapshot> Conditions = new();
    }

    [Serializable]
    public sealed class ConditionProgressSnapshot
    {
        public string ConditionId;
        public int CurrentValue;
        public bool IsCompleted;
    }

    [Serializable]
    public sealed class AchievementProgressStorageData
    {
        public List<AchievementProgressSnapshot> Entries = new();
    }
}
