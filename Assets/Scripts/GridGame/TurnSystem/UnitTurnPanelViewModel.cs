using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class UnitTurnPanelViewModel : IDisposable
{
    private readonly ITurnStateViewModel _turnState;
    private readonly GridUnitAssetMap _unitAssets;
    private readonly IGridViewModel _gridViewModel;
    private readonly GameViewModel _gameViewModel;
    private readonly ITeamColorProvider _teamColorProvider;
    private readonly CompositeDisposable _disposables = new();
    private readonly Dictionary<ICombatObject, UnitPortraitViewModel> _portraitLookup = new();
    private UnitPortraitViewModel _hoveredPortrait;

    public ReactiveCollection<UnitPortraitViewModel> TurnQueue { get; } = new();
    public ReactiveProperty<UnitPortraitViewModel> ActivePortrait { get; } = new();
    public ReactiveProperty<int> TurnNumber { get; } = new(0);

    public event Action<UnitPortraitViewModel> PortraitEnqueued;
    public event Action<UnitPortraitViewModel> PortraitDequeued;
    public event Action<UnitPortraitViewModel> PortraitRemoved;

    public UnitTurnPanelViewModel(
        ITurnStateViewModel turnState,
        GridUnitAssetMap unitAssets,
        IGridViewModel gridViewModel,
        GameViewModel gameViewModel,
        ITeamColorProvider teamColorProvider = null)
    {
        _turnState = turnState ?? throw new ArgumentNullException(nameof(turnState));
        _unitAssets = unitAssets;
        _gridViewModel = gridViewModel ?? throw new ArgumentNullException(nameof(gridViewModel));
        _gameViewModel = gameViewModel ?? throw new ArgumentNullException(nameof(gameViewModel));
        _teamColorProvider = teamColorProvider;

        TurnNumber.Value = _turnState.TurnNumber.Value;

        _turnState.ActiveObject
            .Subscribe(OnActiveObjectChanged)
            .AddTo(_disposables);

        _turnState.TurnNumber
            .Subscribe(OnTurnNumberChanged)
            .AddTo(_disposables);

        _turnState.UnitAddedStream
            .Subscribe(OnCombatUnitAdded)
            .AddTo(_disposables);

        _gameViewModel.HoveredUnitChanged += OnHoveredUnitChanged;
    }

    private void OnActiveObjectChanged(ICombatObject combatObject)
    {
        if (combatObject == null)
        {
            ActivePortrait.Value = null;
            return;
        }

        if (!_portraitLookup.TryGetValue(combatObject, out var portrait))
        {
            return;
        }

        if (TurnQueue.Remove(portrait))
        {
            PortraitDequeued?.Invoke(portrait);
        }

        ActivePortrait.Value = portrait;
    }

    private void OnTurnNumberChanged(int number)
    {
        TurnNumber.Value = number;
    }

    private void OnCombatUnitAdded(UnitTurnInfo info)
    {
        if (info.Unit == null)
        {
            return;
        }

        var portrait = GetOrCreatePortrait(info.Unit, info.Turn);
        if (!TurnQueue.Contains(portrait))
        {
            TurnQueue.Add(portrait);
            PortraitEnqueued?.Invoke(portrait);
        }
    }

    private UnitPortraitViewModel GetOrCreatePortrait(ICombatObject combatUnit, int turn)
    {
        if (!_portraitLookup.TryGetValue(combatUnit, out var portrait))
        {
            var icon = ResolveIcon(combatUnit.UnitType);
            portrait = new UnitPortraitViewModel(combatUnit, icon, turn, _gridViewModel, _gameViewModel, _teamColorProvider);
            portrait.UnitRemoved += OnPortraitUnitRemoved;
            _portraitLookup[combatUnit] = portrait;
        }
        else
        {
            portrait.UpdateTurnOrder(turn);
        }

        return portrait;
    }

    private void OnPortraitUnitRemoved(UnitPortraitViewModel portrait)
    {
        if (portrait == null)
        {
            return;
        }

        portrait.UnitRemoved -= OnPortraitUnitRemoved;

        if (TurnQueue.Remove(portrait))
        {
            PortraitRemoved?.Invoke(portrait);
        }

        if (_portraitLookup.TryGetValue(portrait.CombatObject, out var stored) && stored == portrait)
        {
            _portraitLookup.Remove(portrait.CombatObject);
        }

        if (ActivePortrait.Value == portrait)
        {
            ActivePortrait.Value = null;
        }
    }

    private Sprite ResolveIcon(UnitType unitType)
    {
        if (_unitAssets != null && _unitAssets.TryGetShared(unitType, out var shared) && shared != null)
        {
            return shared.Icon;
        }

        Debug.LogWarning($"[UnitTurnPanelViewModel] Icon not found for unit type {unitType}");
        return null;
    }

    public void Dispose()
    {
        _gameViewModel.HoveredUnitChanged -= OnHoveredUnitChanged;
        _disposables.Dispose();
        foreach (var portrait in _portraitLookup.Values)
        {
            portrait.UnitRemoved -= OnPortraitUnitRemoved;
            portrait.Dispose();
        }
        _portraitLookup.Clear();
    }

    private void OnHoveredUnitChanged(UnitViewModel hoveredUnit)
    {
        var next = ResolvePortrait(hoveredUnit);
        if (_hoveredPortrait == next)
            return;

        if (_hoveredPortrait != null)
        {
            _hoveredPortrait.SetHovered(false);
        }

        _hoveredPortrait = next;
        if (_hoveredPortrait != null)
        {
            _hoveredPortrait.SetHovered(true);
        }
    }

    private UnitPortraitViewModel ResolvePortrait(UnitViewModel hoveredUnit)
    {
        if (hoveredUnit == null)
            return null;

        if (hoveredUnit.Model == null)
            return null;

        _portraitLookup.TryGetValue(hoveredUnit.Model, out var portrait);
        return portrait;
    }
}
