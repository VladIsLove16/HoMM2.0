using Adventure.Domain.Inventory;
using Adventure.Infrastructure.Inventory;
using Adventure.Settings.ViewModel;
using Assets.Scripts.Adventure.Infrastructure.Input;
using CustomEventBus;
using Game.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using Zenject;

namespace Adventure.Presentation.Mushroom
{
    public class MushroomBookViewModel : IDisposable, IAdventureGameActiveMenu
    {
        private int _entriesPerPage = 4;
        private static readonly UnitStatType[] _statOrder =
        {
            UnitStatType.Health,
            UnitStatType.MaxHealth,
            UnitStatType.Damage,
            UnitStatType.SpellPower,
            UnitStatType.Offense,
            UnitStatType.Defense,
            UnitStatType.MoveSpeed,
            UnitStatType.AttackRange
        };

        private static readonly Dictionary<UnitStatType, string> StatLocalizationKeys = new()
        {
            { UnitStatType.Health, "UnitStat.Health" },
            { UnitStatType.MaxHealth, "UnitStat.MaxHealth" },
            { UnitStatType.Damage, "UnitStat.Damage" },
            { UnitStatType.SpellPower, "UnitStat.SpellPower" },
            { UnitStatType.Offense, "UnitStat.Offense" },
            { UnitStatType.Defense, "UnitStat.Defense" },
            { UnitStatType.MoveSpeed, "UnitStat.MoveSpeed" },
            { UnitStatType.AttackRange, "UnitStat.AttackRange" },
            { UnitStatType.CanFly, "UnitStat.CanFly" }
        };

        private readonly MushroomInventoryModel _mushroomInventoryModel;
        private readonly AdventureMushroomAssetMap _definitions;
        private readonly EventBus _gameplayEvents;
        private readonly CompositeDisposable _subscriptions = new CompositeDisposable();
        private readonly List<MushroomBookEntryViewModel> _allEntries = new();

        private readonly ReactiveCollection<MushroomBookEntryViewModel> _currentPageEntries =
            new ReactiveCollection<MushroomBookEntryViewModel>();
        private readonly ReactiveProperty<int> _currentPageIndex = new ReactiveProperty<int>(0);
        private readonly ReactiveProperty<PresentationMode> _presentationMode =
            new ReactiveProperty<PresentationMode>(global::PresentationMode.Normal);
        private readonly ReactiveProperty<bool> _isOpen = new(false);

        public IReadOnlyReactiveProperty<bool> IsOpen => _isOpen;
        public IReadOnlyReactiveCollection<MushroomBookEntryViewModel> CurrentPageEntries => _currentPageEntries;
        public IReadOnlyReactiveProperty<int> CurrentPage => _currentPageIndex;
        public int TotalPages => (_allEntries.Count + _entriesPerPage - 1) / _entriesPerPage;
        public IReadOnlyReactiveProperty<PresentationMode> PresentationMode => _presentationMode;
        public InputMode InputMode => InputMode.Blocked;

        public MushroomBookViewModel(
            MushroomInventoryModel mushroomInventoryModel,
            AdventureMushroomAssetMap definitions,
            EventBus gameplayEvents)
        {
            _mushroomInventoryModel = mushroomInventoryModel ?? throw new ArgumentNullException(nameof(mushroomInventoryModel));
            _definitions = definitions ?? throw new ArgumentNullException(nameof(definitions));
            _gameplayEvents = gameplayEvents ?? throw new ArgumentNullException(nameof(gameplayEvents));

            RebuildEntries();
            UpdateCurrentPage();
            _mushroomInventoryModel.InventoryChanged += OnInventoryChanged;
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        }

        public void NextPage() => GoToPage(_currentPageIndex.Value + 1);
        public void PrevPage() => GoToPage(_currentPageIndex.Value - 1);

        public void Toggle()
        {
            if (_isOpen.Value)
                Close();
            else
                Open();
        }

        public void Open()
        {
            if (_isOpen.Value)
                return;

            _isOpen.SetValueAndForceNotify(true);
        }

        public event Action<UnitType> MushroomDropped;

        public bool Drop(UnitType type) => DropInternal(type, notify: true);

        public void Collect(UnitType type) => Collect(type, 1);

        public void Collect(UnitType type, int amount)
        {
            if (_mushroomInventoryModel == null || amount <= 0)
                return;

            _mushroomInventoryModel.Add(type, amount);
            var totals = _mushroomInventoryModel.Items?.ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value) ?? new Dictionary<string, int>();
            _gameplayEvents.Invoke(new MushroomCollectedCustomEvent(type.ToString(), totals));
        }

        private bool DropInternal(UnitType type, bool notify)
        {
            if (_mushroomInventoryModel == null)
                return false;

            if (!_mushroomInventoryModel.TryConsume(type))
                return false;

            var totals = _mushroomInventoryModel.Items?.ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value) ?? new Dictionary<string, int>();
            if (notify)
            {
                _gameplayEvents.Invoke(new MushroomDropedEvent(type.ToString(), totals));
            }

            MushroomDropped?.Invoke(type);
            return true;
        }

        public void SetPageCapacity(int count)
        {
            _entriesPerPage = count;
            UpdateCurrentPage();
        }

        public void Close()
        {
            if (!_isOpen.Value)
                return;
            _isOpen.SetValueAndForceNotify(false);
        }

        public void GoToPage(int pageIndex)
        {
            var clamped = Mathf.Clamp(pageIndex, 0, TotalPages);
            if (clamped == _currentPageIndex.Value && _currentPageEntries.Count > 0)
                return;

            _currentPageIndex.Value = clamped;
            UpdateCurrentPage();
        }

        public void SetPresentationMode(PresentationMode mode)
        {
            if (_presentationMode.Value == mode)
                return;

            _presentationMode.Value = mode;
        }

        public void SwapCurrentPageEntries(int slotA, int slotB)
        {
            if (slotA == slotB)
                return;
            if (slotA < 0 || slotB < 0)
                return;
            var start = _currentPageIndex.Value * _entriesPerPage;
            if (start + slotA >= _allEntries.Count || start + slotB >= _allEntries.Count)
                return;

            (_allEntries[start + slotA], _allEntries[start + slotB]) = (_allEntries[start + slotB], _allEntries[start + slotA]);
            UpdateCurrentPage();
        }

        private void RebuildEntries()
        {
            _allEntries.Clear();
            foreach (var entry in _mushroomInventoryModel.Items)
            {
                var viewData = CreateEntryViewData(entry.Key, entry.Value);
                if (viewData != null)
                {
                    _allEntries.Add(viewData);
                }
            }
        }

        private void UpdateCurrentPage()
        {
            _currentPageEntries.Clear();

            var maxPageIndex = Mathf.Max(0, TotalPages - 1);
            if (_currentPageIndex.Value > maxPageIndex)
            {
                _currentPageIndex.Value = maxPageIndex;
            }

            if (TotalPages == 0)
                return;

            var start = _currentPageIndex.Value * _entriesPerPage;
            var end = Mathf.Min(start + _entriesPerPage, _allEntries.Count);

            for (int i = start; i < end; i++)
            {
                _currentPageEntries.Add(_allEntries[i]);
            }
        }

        private MushroomBookEntryViewModel CreateEntryViewData(UnitType unitType, int amount)
        {
            if (_definitions == null || !_definitions.TryGetDefinition(unitType, out var definition))
                return null;

            _definitions.TryGetBaseStats(unitType, out var statsForView);
            var shared = definition.SharedData;

            var vm = new MushroomBookEntryViewModel(
                unitType,
                string.IsNullOrWhiteSpace(definition.SharedData.DisplayName) ? unitType.ToString() : definition.SharedData.DisplayName,
                definition.SharedData.Description,
                shared?.Icon,
                shared?.HoveredIcon,
                shared?.HumanizedIcon,
                shared?.HumanizedHoveredIcon,
                BuildStats(statsForView),
                this);
            vm.Amount = amount;
            return vm;
        }

        private IReadOnlyList<MushroomStatViewData> BuildStats(UnitStats stats)
        {
            if (stats == null)
                return Array.Empty<MushroomStatViewData>();

            var result = new List<MushroomStatViewData>();

            foreach (var statType in _statOrder)
            {
                if (TryGetStatValue(statType, stats, out var value))
                {
                    result.Add(new MushroomStatViewData(statType, value, GetLabel(statType)));
                }
            }

            if (stats.CanFly)
            {
                result.Add(new MushroomStatViewData(UnitStatType.CanFly, "Yes", GetLabel(UnitStatType.CanFly)));
            }

            return result;
        }

        private static bool TryGetStatValue(UnitStatType type, UnitStats stats, out string value)
        {
            value = string.Empty;

            switch (type)
            {
                case UnitStatType.Health:
                    value = stats.Health.ToString();
                    return stats.Health != 0;
                case UnitStatType.MaxHealth:
                    value = stats.MaxHealth.ToString();
                    return stats.MaxHealth != 0;
                case UnitStatType.Damage:
                    value = stats.Damage.ToString();
                    return stats.Damage != 0;
                case UnitStatType.SpellPower:
                    value = stats.SpellPower.ToString();
                    return stats.SpellPower != 0;
                case UnitStatType.Offense:
                    value = stats.Offense.ToString();
                    return stats.Offense != 0;
                case UnitStatType.Defense:
                    value = stats.Defense.ToString();
                    return stats.Defense != 0;
                case UnitStatType.MoveSpeed:
                    value = stats.MoveSpeed.ToString();
                    return stats.MoveSpeed != 0;
                case UnitStatType.AttackRange:
                    value = stats.AttackRange.ToString();
                    return stats.AttackRange != 0;
                default:
                    return false;
            }
        }

        private static string GetLabel(UnitStatType statType)
        {
            if (StatLocalizationKeys.TryGetValue(statType, out var key))
            {
                var localized = LocalizationSettings.StringDatabase.GetLocalizedString("MainLocalization", key);
                if (!string.IsNullOrEmpty(localized))
                    return localized;
            }

            return statType.ToString();
        }

        public void Dispose()
        {
            Close();
            _subscriptions.Dispose();
            if (_mushroomInventoryModel != null)
            {
                _mushroomInventoryModel.InventoryChanged -= OnInventoryChanged;
            }

            LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
        }

        private void OnInventoryChanged()
        {
            RebuildEntries();
            UpdateCurrentPage();
        }

        private void OnLocaleChanged(Locale _)
        {
            RebuildEntries();
            UpdateCurrentPage();
        }
    }
}
