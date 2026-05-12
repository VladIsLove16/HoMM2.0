using System;
using System.Collections.Generic;
using Adventure.Domain.Progression;
using Zenject;

namespace Adventure.Infrastructure.Progression
{
    public sealed class StoryFlagsService : IStoryFlagsService
    {
        private readonly IStoryFlagsStore _store;
        private readonly StoryFlagsRuntimeStateSO _runtimeState;
        private readonly HashSet<string> _flags = new(StringComparer.OrdinalIgnoreCase);

        public StoryFlagsService(IStoryFlagsStore store, [InjectOptional] StoryFlagsRuntimeStateSO runtimeState = null)
        {
            _store = store;
            _runtimeState = runtimeState;
            Load();
            SyncRuntimeState();
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
                SyncRuntimeState();
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
                SyncRuntimeState();
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
                SyncRuntimeState();
            }
        }

        public void ReplaceAll(IEnumerable<string> flagIds)
        {
            var nextFlags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (flagIds != null)
            {
                foreach (var flagId in flagIds)
                {
                    if (!string.IsNullOrWhiteSpace(flagId))
                    {
                        nextFlags.Add(flagId.Trim());
                    }
                }
            }

            if (_flags.SetEquals(nextFlags))
            {
                return;
            }

            _flags.Clear();
            foreach (var flagId in nextFlags)
            {
                _flags.Add(flagId);
            }

            Save();
            SyncRuntimeState();
        }

        public void ResetAll()
        {
            if (_flags.Count == 0)
            {
                _store?.Clear();
                SyncRuntimeState();
                return;
            }

            _flags.Clear();
            _store?.Clear();
            SyncRuntimeState();
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

        private void SyncRuntimeState()
        {
            _runtimeState?.SetSnapshot(_flags);
        }
    }
}
