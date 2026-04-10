using System;
using System.Collections.Generic;
using UniRx;

public sealed class TurnStateViewModel : ITurnStateViewModel, IDisposable
{
    private readonly ITurnService _turnService;
    private readonly IBattleControlModeService _battleControlModes;
    private readonly CompositeDisposable _disposables = new();
    private readonly ReactiveProperty<ICombatObject> _activeObject;
    private readonly ReactiveProperty<BattleState> _battleState;
    private readonly ReactiveProperty<int> _turnNumber;

    public TurnStateViewModel(ITurnService turnService, [Zenject.InjectOptional] IBattleControlModeService battleControlModes = null)
    {
        _turnService = turnService ?? throw new ArgumentNullException(nameof(turnService));
        _battleControlModes = battleControlModes;

        _activeObject = new ReactiveProperty<ICombatObject>(turnService.ActiveObject);
        _battleState = new ReactiveProperty<BattleState>(turnService.BattleState);
        _turnNumber = new ReactiveProperty<int>(turnService.TurnNumber);

        _turnService.ActiveObjectStream
            .Subscribe(value => _activeObject.Value = value)
            .AddTo(_disposables);

        _turnService.BattleStateStream
            .Subscribe(value => _battleState.Value = value)
            .AddTo(_disposables);

        _turnService.TurnNumberStream
            .Subscribe(value => _turnNumber.Value = value)
            .AddTo(_disposables);
    }

    public IReadOnlyReactiveProperty<ICombatObject> ActiveObject => _activeObject;
    public IReadOnlyReactiveProperty<BattleState> BattleStateProperty => _battleState;
    public IReadOnlyReactiveProperty<int> TurnNumber => _turnNumber;
    public IObservable<UnitTurnInfo> UnitAddedStream => _turnService.UnitAddedStream;
    public bool IsMyTurn => _activeObject.Value != null &&
                            _activeObject.Value.Team == _turnService.LocalTeam &&
                            (_battleControlModes?.GetMode(_activeObject.Value.Team) ?? BattleControlMode.Manual) == BattleControlMode.Manual;
    public Team LocalTeam => _turnService.LocalTeam;
    public IReadOnlyList<ICombatObject> CombatUnits => _turnService.CombatUnits;

    public void Dispose()
    {
        _disposables.Dispose();
        _activeObject.Dispose();
        _battleState.Dispose();
        _turnNumber.Dispose();
    }
}
