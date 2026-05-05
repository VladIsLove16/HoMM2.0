using System;
using System.Collections.Generic;
using Adventure.Domain.Progression;

namespace Adventure.Infrastructure.Progression
{
    public sealed class StoryFlagsService : IStoryFlagsService
    {
        private readonly IStoryFlagsStore _store;
        private readonly HashSet<string> _flags = new(StringComparer.OrdinalIgnoreCase);

        public StoryFlagsService(IStoryFlagsStore store)
        {
            _store = store;
            Load();
        }

        public IReadOnlyCollection<string> Flags => _flags;

        public bool Has(string flagId)
        {
            return !string.IsNullOrWhiteSpace(flagId) && _flags.Contains(flagId);
        }

        public bool HasAll(IEnumerable<string> flagIds)
        {
            if (flagIds == null)
            {
                return true;
            }

            foreach (var flagId in flagIds)
            {
                if (string.IsNullOrWhiteSpace(flagId))
                {
                    continue;
                }

                if (!_flags.Contains(flagId))
                {
                    return false;
                }
            }

            return true;
        }

        public void Set(string flagId)
        {
            if (string.IsNullOrWhiteSpace(flagId))
            {
                return;
            }

            if (_flags.Add(flagId))
            {
                Save();
            }
        }

        public void SetMany(IEnumerable<string> flagIds)
        {
            if (flagIds == null)
            {
                return;
            }

            var changed = false;
            foreach (var flagId in flagIds)
            {
                if (string.IsNullOrWhiteSpace(flagId))
                {
                    continue;
                }

                changed |= _flags.Add(flagId);
            }

            if (changed)
            {
                Save();
            }
        }

        public void Clear(string flagId)
        {
            if (string.IsNullOrWhiteSpace(flagId))
            {
                return;
            }

            if (_flags.Remove(flagId))
            {
                Save();
            }
        }

        public void ResetAll()
        {
            if (_flags.Count == 0)
            {
                _store?.Clear();
                return;
            }

            _flags.Clear();
            _store?.Clear();
        }

        private void Load()
        {
            _flags.Clear();
            var snapshot = _store?.Load();
            if (snapshot?.Flags == null)
            {
                return;
            }

            foreach (var flagId in snapshot.Flags)
            {
                if (!string.IsNullOrWhiteSpace(flagId))
                {
                    _flags.Add(flagId);
                }
            }
        }

        private void Save()
        {
            if (_store == null)
            {
                return;
            }

            var snapshot = new StoryFlagsSnapshot
            {
                Flags = new List<string>(_flags)
            };

            _store.Save(snapshot);
        }
    }
}
