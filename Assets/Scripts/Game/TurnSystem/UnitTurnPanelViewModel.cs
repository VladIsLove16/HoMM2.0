using System;
using System.Collections.Generic;
using UniRx;

public class UnitTurnPanelViewModel : IDisposable
{
    private readonly ITurnStateViewModel _turnState;
    private readonly CompositeDisposable _disposables = new();

    public ReactiveCollection<UnitTurnInfo> TurnQueue { get; } = new();
    public ReactiveProperty<ICombatObject> ActiveUnit { get; } = new();
    public ReactiveProperty<int> TurnNumber { get; } = new(0);

    public event Action<UnitTurnInfo> UnitAdded;

    public UnitTurnPanelViewModel(ITurnStateViewModel turnState)
    {
        _turnState = turnState ?? throw new ArgumentNullException(nameof(turnState));

        ActiveUnit.Value = _turnState.ActiveObject.Value;
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
    }

    private void OnActiveObjectChanged(ICombatObject combatObject)
    {
        ActiveUnit.Value = combatObject;
        if (TurnQueue.Count > 0 && TurnQueue[0].Unit == combatObject)
        {
            TurnQueue.RemoveAt(0);
        }
    }

    private void OnTurnNumberChanged(int number)
    {
        TurnNumber.Value = number;
    }

    private void OnCombatUnitAdded(UnitTurnInfo info)
    {
        TurnQueue.Add(info);
        UnitAdded?.Invoke(info);
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}
