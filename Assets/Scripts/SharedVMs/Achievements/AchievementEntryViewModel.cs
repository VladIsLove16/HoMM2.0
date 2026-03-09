using System;
using UniRx;
using UnityEngine;
using UnityEngine.Localization;

namespace Game.Achievements
{
    public sealed class AchievementEntryViewModel : IDisposable
    {
        private readonly AchievementDefinition _definition;
        private readonly ReactiveProperty<string> _title = new(string.Empty);
        private readonly ReactiveProperty<string> _description = new(string.Empty);
        private readonly ReactiveProperty<string> _progressText = new(string.Empty);
        private readonly ReactiveProperty<float> _progress01 = new(0f);
        private readonly ReactiveProperty<bool> _isUnlocked = new(false);
        private readonly ReactiveProperty<bool> _isSecretLocked = new(false);
        private readonly ReactiveProperty<string> _category = new(string.Empty);

        public AchievementEntryViewModel(AchievementDefinition definition)
        {
            _definition = definition ?? throw new ArgumentNullException(nameof(definition));
            Icon = definition.Icon;
            RewardCurrency = definition.RewardCurrencyAmount;
            _category.Value = definition.Category.ToString();
            _title.Value = ResolveLocalized(definition.TitleLocalized, definition.Title);
            _description.Value = ResolveLocalized(definition.DescriptionLocalized, definition.Description);
        }

        public string Id => _definition.Id;
        public Sprite Icon { get; }
        public int RewardCurrency { get; }
        public IReadOnlyReactiveProperty<string> Title => _title;
        public IReadOnlyReactiveProperty<string> Description => _description;
        public IReadOnlyReactiveProperty<string> ProgressText => _progressText;
        public IReadOnlyReactiveProperty<float> Progress01 => _progress01;
        public IReadOnlyReactiveProperty<bool> IsUnlocked => _isUnlocked;
        public IReadOnlyReactiveProperty<bool> IsSecretLocked => _isSecretLocked;
        public IReadOnlyReactiveProperty<string> Category => _category;

        public void RefreshText()
        {
            _title.Value = ResolveLocalized(_definition.TitleLocalized, _definition.Title);
            _description.Value = ResolveLocalized(_definition.DescriptionLocalized, _definition.Description);
            ApplySecretMask();
        }

        public void UpdateProgress(AchievementProgress progress)
        {
            if (progress == null)
                return;

            _isUnlocked.Value = progress.IsUnlocked;
            _isSecretLocked.Value = _definition.IsSecret && !progress.IsUnlocked;

            if (_isSecretLocked.Value)
            {
                _progress01.Value = 0f;
                _progressText.Value = "???";
                ApplySecretMask();
                return;
            }

            var progress01 = progress.GetProgress01(_definition);
            _progress01.Value = progress01;

            _progressText.Value = BuildProgressLabel(progress, progress01);
            ApplySecretMask();
        }

        private string BuildProgressLabel(AchievementProgress progress, float progress01)
        {
            if (_definition.Conditions != null && _definition.Conditions.Count == 1)
            {
                if (_definition.Conditions[0] is AchievementCounterConditionSO counter)
                {
                    var condition = progress.GetOrCreateCondition(counter.ConditionId);
                    var target = counter.TargetValue <= 0 ? 1 : counter.TargetValue;
                    return $"{Mathf.Clamp(condition.CurrentValue, 0, target)}/{target}";
                }
            }

            return $"{Mathf.RoundToInt(progress01 * 100f)}%";
        }

        private void ApplySecretMask()
        {
            if (_isSecretLocked.Value)
            {
                _title.Value = "???";
                _description.Value = "???";
            }
            else
            {
                _title.Value = ResolveLocalized(_definition.TitleLocalized, _definition.Title);
                _description.Value = ResolveLocalized(_definition.DescriptionLocalized, _definition.Description);
            }
        }

        private static string ResolveLocalized(LocalizedString localized, string fallback)
        {
            if (localized != null && !localized.IsEmpty)
            {
                var value = localized.GetLocalizedString();
                if (!string.IsNullOrEmpty(value))
                    return value;
            }

            return fallback ?? string.Empty;
        }

        public void Dispose()
        {
            _title.Dispose();
            _description.Dispose();
            _progressText.Dispose();
            _progress01.Dispose();
            _isUnlocked.Dispose();
            _isSecretLocked.Dispose();
            _category.Dispose();
        }
    }
}
