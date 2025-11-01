using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Achievements
{
    [CreateAssetMenu(menuName = "Game/Achievements/Catalog", fileName = "AchievementCatalog")]
    public sealed class AchievementCatalog : ScriptableObject
    {
        [SerializeField] private List<AchievementDefinition> achievements = new();

        private Dictionary<string, AchievementDefinition> _lookup;

        public IReadOnlyList<AchievementDefinition> Achievements => achievements;

        private void OnEnable()
        {
            BuildLookup();
        }

        private void BuildLookup()
        {
            if (_lookup != null && _lookup.Count == achievements.Count)
            {
                return;
            }

            _lookup = new Dictionary<string, AchievementDefinition>(achievements.Count, StringComparer.OrdinalIgnoreCase);
            foreach (var definition in achievements)
            {
                if (definition == null)
                {
                    continue;
                }

                var id = definition.Id;
                if (string.IsNullOrWhiteSpace(id))
                {
                    continue;
                }

                _lookup[id] = definition;
            }
        }

        public bool TryGetById(string id, out AchievementDefinition definition)
        {
            if (_lookup == null)
            {
                BuildLookup();
            }

            return _lookup != null && _lookup.TryGetValue(id, out definition);
        }
    }
}