using UnityEngine;

namespace Game.Achievements
{
    public sealed class PlayerPrefsAchievementStorage : IAchievementStorage
    {
        private const string Key = "achievements_progress";

        public AchievementProgressStorageData Load()
        {
            var stored = PlayerPrefs.GetString(Key, string.Empty);
            if (string.IsNullOrEmpty(stored))
                return new AchievementProgressStorageData();

            var data = JsonUtility.FromJson<AchievementProgressStorageData>(stored);
            return data ?? new AchievementProgressStorageData();
        }

        public void Save(AchievementProgressStorageData data)
        {
            var payload = data == null ? string.Empty : JsonUtility.ToJson(data);
            PlayerPrefs.SetString(Key, payload);
            PlayerPrefs.Save();
        }

        public void Clear()
        {
            PlayerPrefs.DeleteKey(Key);
            PlayerPrefs.Save();
        }
    }
}
