using Adventure.Domain.Inventory;
using Adventure.Settings.ViewModel;
using Assets.Scripts.Adventure.Infrastructure.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using Zenject;
using Adventure.Infrastructure.Events;

namespace Adventure.Presentation.Mushroom
{
    public class MushroomBookViewModel : IDisposable, IActiveMenu
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

        private readonly MushroomInventoryModel _mushroomInventoryModel;
        private readonly UnitDefinitionSOCollection _catalog;
        private readonly IGameplayEventBus _gameplayEvents;
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
        public int TotalPages => (_allEntries.Count  + _entriesPerPage -1) / _entriesPerPage;
        public IReadOnlyReactiveProperty<PresentationMode> PresentationMode => _presentationMode;

        public InputMode InputMode => InputMode.Blocked;

        public MushroomBookViewModel(
            MushroomInventoryModel mushroomInventoryModel,
            UnitDefinitionSOCollection catalog,
            IGameplayEventBus gameplayEvents)
        {
            _mushroomInventoryModel = mushroomInventoryModel ?? throw new ArgumentNullException(nameof(mushroomInventoryModel));
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _gameplayEvents = gameplayEvents ?? throw new ArgumentNullException(nameof(gameplayEvents));

            RebuildEntries();
            UpdateCurrentPage();
        }
        public void NextPage() => GoToPage(_currentPageIndex.Value + 1);
        public void PrevPage() => GoToPage(_currentPageIndex.Value - 1);

        public void Toggle()
        {
            UnityLogger.Log("book toggle");
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
        public void Collect(UnitType type)
        {
            if (_mushroomInventoryModel == null)
                return;

            _mushroomInventoryModel.Add(type);
            _gameplayEvents.PublishMushroomCollected(type, _mushroomInventoryModel.Items);
            RebuildEntries();
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

        private void RebuildEntries()
        {
            _allEntries.Clear();
            foreach (var entry in _mushroomInventoryModel.Items)
            {
                if (!_catalog.TryGet(entry.Key, out var definition) || definition == null)
                    continue;

                var viewData = CreateEntryViewData(definition);
                _allEntries.Add(viewData);
            }
        }

        private void UpdateCurrentPage()
        {
            _currentPageEntries.Clear();

            if (TotalPages == 0)
                return;

            var start = _currentPageIndex.Value * _entriesPerPage;
            var end = Mathf.Min(start + _entriesPerPage, _allEntries.Count);

            for (int i = start; i < end; i++)
            {
                _currentPageEntries.Add(_allEntries[i]);
            }
            UnityLogger.Log("new _currentPageEntries " + _currentPageEntries.Count);

        }

        private MushroomBookEntryViewModel CreateEntryViewData(UnitDefinitionSO definition)
        {
            var stats = BuildStats(definition.UnitStats);

            var vm = new MushroomBookEntryViewModel(
                definition.UnitType,
                definition.DisplayName,
                definition.Description,
                definition.Icon,
                definition.HoveredIcon,
                definition.HumanizedIcon,
                definition.HumanizedHoveredIcon,
                stats);
            vm.Amount = _mushroomInventoryModel.GetAmount(definition.UnitType);
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
            switch (statType)
            {
                case UnitStatType.MaxHealth:
                    return "Max Health";
                case UnitStatType.MoveSpeed:
                    return "Move Speed";
                case UnitStatType.AttackRange:
                    return "Attack Range";
                case UnitStatType.CanFly:
                    return "Can Fly";
                default:
                    return statType.ToString();
            }
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
            _currentPageEntries?.Dispose();
            _currentPageIndex?.Dispose();
            _presentationMode?.Dispose();
        }

    }
}










