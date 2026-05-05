using Adventure.Domain.Progression;
using UnityEngine;

namespace Adventure.Infrastructure.Progression
{
    public sealed class PlayerPrefsStoryFlagsStore : IStoryFlagsStore
    {
        private const string StorageKey = "story_flags";

        public StoryFlagsSnapshot Load()
        {
            if (!PlayerPrefs.HasKey(StorageKey))
            {
                return new StoryFlagsSnapshot();
            }

            var json = PlayerPrefs.GetString(StorageKey);
            if (string.IsNullOrWhiteSpace(json))
            {
                return new StoryFlagsSnapshot();
            }

            return JsonUtility.FromJson<StoryFlagsSnapshot>(json) ?? new StoryFlagsSnapshot();
        }

        public void Save(StoryFlagsSnapshot snapshot)
        {
            var json = JsonUtility.ToJson(snapshot ?? new StoryFlagsSnapshot());
            PlayerPrefs.SetString(StorageKey, json);
            PlayerPrefs.Save();
        }

        public void Clear()
        {
            if (!PlayerPrefs.HasKey(StorageKey))
            {
                return;
            }

            PlayerPrefs.DeleteKey(StorageKey);
            PlayerPrefs.Save();
        }
    }
}
