using System;
using System.Collections.Generic;
using System.Linq;
using Adventure.Settings.ViewModel;
using UniRx;
using UnityEngine.Localization.Settings;

namespace Game.Achievements
{
    public sealed class AchievementsViewModel : IActiveMenu, IDisposable
    {
        private readonly IAchievementDefinitionProvider _definitions;
        private readonly IAchievementService _service;
        private readonly ReactiveCollection<AchievementEntryViewModel> _entries = new();
        private readonly ReactiveProperty<bool> _isOpen = new(false);
        private readonly CompositeDisposable _disposables = new();

        public AchievementsViewModel(IAchievementDefinitionProvider definitions, IAchievementService service)
        {
            _definitions = definitions ?? throw new ArgumentNullException(nameof(definitions));
            _service = service ?? throw new ArgumentNullException(nameof(service));

            BuildEntries();
            RefreshAll();

            _service.AchievementUnlocked += OnAchievementUnlocked;
            _service.ProgressChanged += OnProgressChanged;
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        }

        public IReadOnlyReactiveProperty<bool> IsOpen => _isOpen;
        public IReadOnlyReactiveCollection<AchievementEntryViewModel> Entries => _entries;

        public void Open() => _isOpen.SetValueAndForceNotify(true);
        public void Close() => _isOpen.SetValueAndForceNotify(false);
        public void Toggle()
        {
            if (_isOpen.Value)
                Close();
            else
                Open();
        }

        public void ResetAll()
        {
            _service.ResetAll();
            RefreshAll();
        }

        private void BuildEntries()
        {
            _entries.Clear();
            foreach (var definition in _definitions.All ?? Array.Empty<AchievementDefinition>())
            {
                if (definition == null)
                    continue;
                _entries.Add(new AchievementEntryViewModel(definition));
            }
        }

        private void RefreshAll()
        {
            foreach (var entry in _entries)
            {
                if (entry == null)
                    continue;
                entry.RefreshText();
                var progress = _service.GetProgress(entry.Id);
                if (progress != null)
                {
                    entry.UpdateProgress(progress);
                }
            }
        }

        private void OnAchievementUnlocked(AchievementUnlockResult result)
        {
            var progress = result.Progress;
            UpdateEntry(progress);
        }

        private void OnProgressChanged(AchievementProgress progress)
        {
            UpdateEntry(progress);
        }

        private void UpdateEntry(AchievementProgress progress)
        {
            if (progress == null)
                return;

            var entry = _entries.FirstOrDefault(e => e.Id == progress.Id);
            entry?.UpdateProgress(progress);
        }

        private void OnLocaleChanged(UnityEngine.Localization.Locale _)
        {
            foreach (var entry in _entries)
            {
                entry?.RefreshText();
            }
        }

        public void Dispose()
        {
            _service.AchievementUnlocked -= OnAchievementUnlocked;
            _service.ProgressChanged -= OnProgressChanged;
            LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
            foreach (var entry in _entries)
            {
                entry?.Dispose();
            }
            _entries.Clear();
            _disposables.Dispose();
        }
    }
}
