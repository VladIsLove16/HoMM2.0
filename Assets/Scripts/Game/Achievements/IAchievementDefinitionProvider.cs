using System.Collections.Generic;

namespace Game.Achievements
{
    public interface IAchievementDefinitionProvider
    {
        IReadOnlyList<AchievementDefinition> All { get; }
        bool TryGet(string id, out AchievementDefinition definition);
    }
}