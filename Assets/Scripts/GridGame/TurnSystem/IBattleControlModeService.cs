using System;
using UniRx;

public interface IBattleControlModeService
{
    IReadOnlyReactiveProperty<bool> IsFastResolveActive { get; }
    IObservable<Team> TeamModeChanged { get; }

    BattleControlMode GetMode(Team team);
    bool IsAiControlled(Team team);

    void SetMode(Team team, BattleControlMode mode);
    void SetLocalTeamMode(BattleControlMode mode);
    void SetEnemyTeamsMode(BattleControlMode mode);
    void SetAllCombatTeamsMode(BattleControlMode mode);

    void EnableFastResolve();
    void DisableFastResolve();
}
