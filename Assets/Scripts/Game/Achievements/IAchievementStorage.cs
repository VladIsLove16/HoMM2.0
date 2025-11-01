using System.Collections.Generic;

namespace Game.Achievements
{
    public interface IAchievementStorage
    {
        IEnumerable<string> Load();
        void Save(IEnumerable<string> unlockedIds);
    }
}