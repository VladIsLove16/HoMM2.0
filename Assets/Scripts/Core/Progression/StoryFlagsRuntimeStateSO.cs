using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Adventure.Domain.Progression
{
    [CreateAssetMenu(menuName = "Adventure/Progression/Runtime Story Flags", fileName = "RuntimeStoryFlags")]
    public sealed class StoryFlagsRuntimeStateSO : ScriptableObject
    {
        [SerializeField] private bool acceptInspectorChangesInPlayMode = true;
        [SerializeField] private List<string> currentFlags = new();

        private int _lastSyncedHash;

        public IReadOnlyList<string> CurrentFlags => currentFlags;

        public void SetSnapshot(IEnumerable<string> flags)
        {
            currentFlags.Clear();
            currentFlags.AddRange(Normalize(flags));
            _lastSyncedHash = ComputeHash(currentFlags);
        }

        public bool TryConsumeInspectorChanges(out IReadOnlyList<string> flags)
        {
            flags = null;

            if (!acceptInspectorChangesInPlayMode || !Application.isPlaying)
            {
                return false;
            }

            var normalizedFlags = Normalize(currentFlags);
            var currentHash = ComputeHash(normalizedFlags);
            if (currentHash == _lastSyncedHash)
            {
                return false;
            }

            currentFlags.Clear();
            currentFlags.AddRange(normalizedFlags);
            _lastSyncedHash = currentHash;
            flags = currentFlags;
            return true;
        }

        private static IReadOnlyList<string> Normalize(IEnumerable<string> flags)
        {
            if (flags == null)
            {
                return Array.Empty<string>();
            }

            return flags
                .Where(flag => !string.IsNullOrWhiteSpace(flag))
                .Select(flag => flag.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(flag => flag, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static int ComputeHash(IEnumerable<string> flags)
        {
            unchecked
            {
                var hash = 17;
                foreach (var flag in flags)
                {
                    hash = hash * 31 + StringComparer.OrdinalIgnoreCase.GetHashCode(flag);
                }

                return hash;
            }
        }
    }
}
