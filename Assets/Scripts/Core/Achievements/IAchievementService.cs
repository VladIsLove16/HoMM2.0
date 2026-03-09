using System;
using System.Collections.Generic;

namespace Game.Achievements
{
    public interface IAchievementService
    {
        event Action<AchievementUnlockResult> AchievementUnlocked;
        event Action<AchievementProgress> ProgressChanged;

        IReadOnlyCollection<AchievementProgress> Progresses { get; }

        bool IsUnlocked(string id);
        AchievementProgress GetProgress(string id);
        void HandleEvent(AchievementEvent evt);
        bool TryUnlock(string id);
        void ResetAll();
    }
}
