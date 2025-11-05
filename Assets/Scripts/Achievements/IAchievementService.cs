using System;
using System.Collections.Generic;

namespace Game.Achievements
{
    public interface IAchievementService
    {
        event Action<AchievementDefinition> AchievementUnlocked;

        IReadOnlyCollection<string> UnlockedIds { get; }

        bool IsUnlocked(string id);
        bool TryUnlock(string id);
    }
}