using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Achievements
{
    public sealed class AchievementService : IAchievementService
    {
        private readonly IAchievementDefinitionProvider _definitionProvider;
        private readonly IAchievementStorage _storage;
        private readonly HashSet<string> _unlockedIds;

        public event Action<AchievementDefinition> AchievementUnlocked;

        public AchievementService(IAchievementDefinitionProvider definitionProvider, IAchievementStorage storage)
        {
            _definitionProvider = definitionProvider ?? throw new ArgumentNullException(nameof(definitionProvider));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));

            var persisted = storage.Load() ?? Enumerable.Empty<string>();
            _unlockedIds = new HashSet<string>(persisted, StringComparer.OrdinalIgnoreCase);
        }

        public IReadOnlyCollection<string> UnlockedIds => _unlockedIds;

        public bool IsUnlocked(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return false;

            return _unlockedIds.Contains(id);
        }

        public bool TryUnlock(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return false;

            if (_unlockedIds.Contains(id))
                return false;

            if (!_definitionProvider.TryGet(id, out var definition) || definition == null)
            {
                Debug.LogWarning($"[AchievementService] Unknown achievement id '{id}'.");
                return false;
            }

            _unlockedIds.Add(id);
            _storage.Save(_unlockedIds);
            AchievementUnlocked?.Invoke(definition);
            Debug.Log($"[AchievementService] Achievement unlocked: {definition.Title}");
            return true;
        }
    }
}