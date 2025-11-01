using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Achievements
{
    public sealed class AchievementCatalogDefinitionProvider : IAchievementDefinitionProvider
    {
        private readonly IReadOnlyList<AchievementDefinition> _all;
        private readonly Dictionary<string, AchievementDefinition> _lookup;

        public AchievementCatalogDefinitionProvider(AchievementCatalog catalog)
        {
            if (catalog == null)
            {
                Debug.LogWarning("[AchievementCatalogDefinitionProvider] Catalog is not assigned.");
                _all = Array.Empty<AchievementDefinition>();
                _lookup = new Dictionary<string, AchievementDefinition>(StringComparer.OrdinalIgnoreCase);
                return;
            }

            _all = catalog.Achievements?
                .Where(definition => definition != null && !string.IsNullOrWhiteSpace(definition.Id))
                .ToArray() ?? Array.Empty<AchievementDefinition>();

            _lookup = new Dictionary<string, AchievementDefinition>(StringComparer.OrdinalIgnoreCase);
            foreach (var definition in _all)
            {
                _lookup[definition.Id] = definition;
            }
        }

        public IReadOnlyList<AchievementDefinition> All => _all;

        public bool TryGet(string id, out AchievementDefinition definition)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                definition = null;
                return false;
            }

            return _lookup.TryGetValue(id, out definition);
        }
    }
}