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
        private readonly ICurrencyWallet _wallet;
        private readonly Dictionary<string, AchievementProgress> _progressById;

        public event Action<AchievementUnlockResult> AchievementUnlocked;
        public event Action<AchievementProgress> ProgressChanged;

        public AchievementService(IAchievementDefinitionProvider definitionProvider, IAchievementStorage storage, ICurrencyWallet wallet = null)
        {
            _definitionProvider = definitionProvider ?? throw new ArgumentNullException(nameof(definitionProvider));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _wallet = wallet;

            _progressById = new Dictionary<string, AchievementProgress>(StringComparer.OrdinalIgnoreCase);
            LoadProgress();
        }

        public IReadOnlyCollection<AchievementProgress> Progresses => _progressById.Values;

        public bool IsUnlocked(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return false;

            return _progressById.TryGetValue(id, out var progress) && progress.IsUnlocked;
        }

        public AchievementProgress GetProgress(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;

            _progressById.TryGetValue(id, out var progress);
            return progress;
        }

        public void HandleEvent(AchievementEvent evt)
        {
            if (string.IsNullOrWhiteSpace(evt.Id))
                return;

            var anyChanged = false;
            var changedProgress = new List<AchievementProgress>();
            foreach (var definition in _definitionProvider.All ?? Array.Empty<AchievementDefinition>())
            {
                if (definition == null || string.IsNullOrWhiteSpace(definition.Id))
                    continue;

                if (!_progressById.TryGetValue(definition.Id, out var progress))
                {
                    progress = new AchievementProgress(definition.Id);
                    progress.EnsureConditions(definition.Conditions);
                    _progressById[definition.Id] = progress;
                }

                if (progress.IsUnlocked)
                    continue;

                var changed = ApplyEventToDefinition(definition, progress, evt);
                if (changed && IsCompleted(definition, progress))
                {
                    Unlock(definition, progress);
                    anyChanged = true;
                    changedProgress.Add(progress);
                }
                else if (changed)
                {
                    anyChanged = true;
                    changedProgress.Add(progress);
                }
            }

            if (anyChanged)
            {
                SaveProgress();
                foreach (var progress in changedProgress)
                {
                    ProgressChanged?.Invoke(progress);
                }
            }
        }

        public bool TryUnlock(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return false;

            if (!_definitionProvider.TryGet(id, out var definition) || definition == null)
            {
                Debug.LogWarning($"[AchievementService] Unknown achievement id '{id}'.");
                return false;
            }

            if (!_progressById.TryGetValue(id, out var progress))
            {
                progress = new AchievementProgress(id);
                progress.EnsureConditions(definition.Conditions);
                _progressById[id] = progress;
            }

            if (progress.IsUnlocked)
                return false;

            progress.IsUnlocked = true;
            progress.UnlockDateUtc = DateTime.UtcNow;
            foreach (var condition in definition.Conditions ?? Array.Empty<AchievementConditionSO>())
            {
                if (condition == null)
                    continue;
                var conditionProgress = progress.GetOrCreateCondition(condition.ConditionId);
                conditionProgress.IsCompleted = true;
            }

            SaveProgress();
            ApplyReward(definition);
            AchievementUnlocked?.Invoke(new AchievementUnlockResult(definition, progress));
            Debug.Log($"[AchievementService] Achievement unlocked: {definition.Title}");
            return true;
        }

        public void ResetAll()
        {
            foreach (var progress in _progressById.Values)
            {
                progress.Reset();
            }

            _storage.Clear();
        }

        private void LoadProgress()
        {
            var data = _storage.Load() ?? new AchievementProgressStorageData();
            var snapshotLookup = new Dictionary<string, AchievementProgressSnapshot>(StringComparer.OrdinalIgnoreCase);
            foreach (var snapshot in data.Entries)
            {
                if (snapshot == null || string.IsNullOrWhiteSpace(snapshot.Id))
                    continue;
                snapshotLookup[snapshot.Id] = snapshot;
            }

            foreach (var definition in _definitionProvider.All ?? Array.Empty<AchievementDefinition>())
            {
                if (definition == null || string.IsNullOrWhiteSpace(definition.Id))
                    continue;

                var progress = new AchievementProgress(definition.Id);
                progress.EnsureConditions(definition.Conditions);
                if (snapshotLookup.TryGetValue(definition.Id, out var snapshot))
                {
                    ApplySnapshot(progress, snapshot);
                }

                _progressById[definition.Id] = progress;
            }
        }

        private void ApplySnapshot(AchievementProgress progress, AchievementProgressSnapshot snapshot)
        {
            if (progress == null || snapshot == null)
                return;

            progress.IsUnlocked = snapshot.IsUnlocked;
            if (snapshot.UnlockUtcTicks > 0)
            {
                progress.UnlockDateUtc = new DateTime(snapshot.UnlockUtcTicks, DateTimeKind.Utc);
            }

            if (snapshot.Conditions == null)
                return;

            foreach (var conditionSnapshot in snapshot.Conditions)
            {
                if (conditionSnapshot == null || string.IsNullOrWhiteSpace(conditionSnapshot.ConditionId))
                    continue;
                var condition = progress.GetOrCreateCondition(conditionSnapshot.ConditionId);
                condition.CurrentValue = conditionSnapshot.CurrentValue;
                condition.IsCompleted = conditionSnapshot.IsCompleted;
            }
        }

        private void SaveProgress()
        {
            var data = new AchievementProgressStorageData();
            foreach (var progress in _progressById.Values)
            {
                var snapshot = new AchievementProgressSnapshot
                {
                    Id = progress.Id,
                    IsUnlocked = progress.IsUnlocked,
                    UnlockUtcTicks = progress.UnlockDateUtc?.Ticks ?? 0
                };

                foreach (var condition in progress.Conditions)
                {
                    snapshot.Conditions.Add(new ConditionProgressSnapshot
                    {
                        ConditionId = condition.ConditionId,
                        CurrentValue = condition.CurrentValue,
                        IsCompleted = condition.IsCompleted
                    });
                }

                data.Entries.Add(snapshot);
            }

            _storage.Save(data);
        }

        private bool ApplyEventToDefinition(AchievementDefinition definition, AchievementProgress progress, AchievementEvent evt)
        {
            var changed = false;
            if (definition.Conditions == null || definition.Conditions.Count == 0)
                return false;

            foreach (var condition in definition.Conditions)
            {
                if (condition == null)
                    continue;
                if (condition.TryApply(evt, progress))
                {
                    changed = true;
                }
            }

            return changed;
        }

        private bool IsCompleted(AchievementDefinition definition, AchievementProgress progress)
        {
            if (definition == null || definition.Conditions == null || definition.Conditions.Count == 0)
                return false;

            if (definition.CompletionMode == AchievementCompletionMode.Any)
            {
                foreach (var condition in definition.Conditions)
                {
                    if (condition != null && condition.IsSatisfied(progress))
                        return true;
                }
                return false;
            }

            foreach (var condition in definition.Conditions)
            {
                if (condition == null)
                    continue;
                if (!condition.IsSatisfied(progress))
                    return false;
            }

            return true;
        }

        private void Unlock(AchievementDefinition definition, AchievementProgress progress)
        {
            progress.IsUnlocked = true;
            progress.UnlockDateUtc = DateTime.UtcNow;
            ApplyReward(definition);
            AchievementUnlocked?.Invoke(new AchievementUnlockResult(definition, progress));
            ProgressChanged?.Invoke(progress);
            Debug.Log($"[AchievementService] Achievement unlocked: {definition.Title}");
        }

        private void ApplyReward(AchievementDefinition definition)
        {
            if (definition == null || definition.RewardCurrencyAmount <= 0 || _wallet == null)
                return;

            _wallet.Add(definition.RewardCurrencyAmount);
        }
    }
}
