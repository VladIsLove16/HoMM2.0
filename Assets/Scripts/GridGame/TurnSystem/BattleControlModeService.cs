using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;

public sealed class BattleControlModeService : IBattleControlModeService, IDisposable
{
    private readonly ITurnService _turnService;
    private readonly IAnimationSpeedSettings _animationSpeedSettings;
    private readonly BattleAiControlConfigSO _config;
    private readonly Dictionary<Team, BattleControlMode> _modes = new();
    private readonly Subject<Team> _teamModeChanged = new();
    private readonly ReactiveProperty<bool> _isFastResolveActive = new(false);
    private readonly CompositeDisposable _disposables = new();

    private IDisposable _fastResolveHandle;

    public BattleControlModeService(
        ITurnService turnService,
        IAnimationSpeedSettings animationSpeedSettings,
        BattleAiControlConfigSO config)
    {
        _turnService = turnService ?? throw new ArgumentNullException(nameof(turnService));
        _animationSpeedSettings = animationSpeedSettings ?? throw new ArgumentNullException(nameof(animationSpeedSettings));
        _config = config ?? throw new ArgumentNullException(nameof(config));

        _turnService.BattleStateStream
            .Subscribe(state =>
            {
                if (state != BattleState.inProgress)
                {
                    DisableFastResolve();
                }
            })
            .AddTo(_disposables);
    }

    public IReadOnlyReactiveProperty<bool> IsFastResolveActive => _isFastResolveActive;
    public IObservable<Team> TeamModeChanged => _teamModeChanged;

    public BattleControlMode GetMode(Team team)
    {
        if (team == Team.None)
            return BattleControlMode.Manual;

        if (_modes.TryGetValue(team, out var mode))
            return mode;

        return GetDefaultMode(team);
    }

    public bool IsAiControlled(Team team) => GetMode(team) == BattleControlMode.AI;

    public void SetMode(Team team, BattleControlMode mode)
    {
        if (team == Team.None)
            return;

        DisableFastResolveIfNeeded();

        if (GetMode(team) == mode && _modes.ContainsKey(team))
            return;

        _modes[team] = mode;
        _teamModeChanged.OnNext(team);
    }

    public void SetLocalTeamMode(BattleControlMode mode)
    {
        SetMode(_turnService.LocalTeam, mode);
    }

    public void SetEnemyTeamsMode(BattleControlMode mode)
    {
        foreach (var team in GetCombatTeams().Where(team => team != _turnService.LocalTeam))
        {
            SetMode(team, mode);
        }
    }

    public void SetAllCombatTeamsMode(BattleControlMode mode)
    {
        foreach (var team in GetCombatTeams())
        {
            SetMode(team, mode);
        }
    }

    public void EnableFastResolve()
    {
        if (_isFastResolveActive.Value)
            return;

        _isFastResolveActive.Value = true;

        if (_config.FastResolveControlsLocalTeam)
        {
            _modes[_turnService.LocalTeam] = BattleControlMode.AI;
            _teamModeChanged.OnNext(_turnService.LocalTeam);
        }

        if (_config.FastResolveControlsEnemyTeams)
        {
            foreach (var team in GetCombatTeams().Where(team => team != _turnService.LocalTeam))
            {
                _modes[team] = BattleControlMode.AI;
                _teamModeChanged.OnNext(team);
            }
        }

        _fastResolveHandle = _animationSpeedSettings.PushPlaybackOverride(
            _config.FastResolveAnimationSpeedMultiplier,
            _config.FastResolveIsInstant);
    }

    public void DisableFastResolve()
    {
        if (!_isFastResolveActive.Value)
            return;

        _isFastResolveActive.Value = false;
        _fastResolveHandle?.Dispose();
        _fastResolveHandle = null;
    }

    public void Dispose()
    {
        DisableFastResolve();
        _disposables.Dispose();
        _teamModeChanged.Dispose();
        _isFastResolveActive.Dispose();
    }

    private BattleControlMode GetDefaultMode(Team team)
    {
        if (_turnService.Mode != GameMode.SinglePlayer)
            return team == _turnService.LocalTeam ? BattleControlMode.Manual : BattleControlMode.Manual;

        return team == _turnService.LocalTeam
            ? BattleControlMode.Manual
            : BattleControlMode.AI;
    }

    private IEnumerable<Team> GetCombatTeams()
    {
        var teams = _turnService.CombatUnits
            .Where(unit => unit != null)
            .Select(unit => unit.Team)
            .Where(team => team != Team.None)
            .Distinct()
            .ToList();

        if (!teams.Contains(_turnService.LocalTeam))
        {
            teams.Add(_turnService.LocalTeam);
        }

        return teams;
    }

    private void DisableFastResolveIfNeeded()
    {
        if (_isFastResolveActive.Value)
        {
            DisableFastResolve();
        }
    }
}
