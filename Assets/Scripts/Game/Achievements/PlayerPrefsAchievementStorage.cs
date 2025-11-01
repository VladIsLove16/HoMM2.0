using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Achievements
{
    public sealed class PlayerPrefsAchievementStorage : IAchievementStorage
    {
        private const string Key = "achievements_unlocked";
        private const char Separator = '|';

        public IEnumerable<string> Load()
        {
            var stored = PlayerPrefs.GetString(Key, string.Empty);
            if (string.IsNullOrEmpty(stored))
            {
                return Array.Empty<string>();
            }

            return stored.Split(Separator, StringSplitOptions.RemoveEmptyEntries).Distinct(StringComparer.OrdinalIgnoreCase);
        }

        public void Save(IEnumerable<string> unlockedIds)
        {
            var value = unlockedIds == null
                ? string.Empty
                : string.Join(Separator, unlockedIds);

            PlayerPrefs.SetString(Key, value);
            PlayerPrefs.Save();
        }
    }
}