using System;
using System.Collections.Generic;

namespace Game.Achievements
{
    public sealed class EmptyAchievementDefinitionProvider : IAchievementDefinitionProvider
    {
        public IReadOnlyList<AchievementDefinition> All => Array.Empty<AchievementDefinition>();

        public bool TryGet(string id, out AchievementDefinition definition)
        {
            definition = null;
            return false;
        }
    }
}
