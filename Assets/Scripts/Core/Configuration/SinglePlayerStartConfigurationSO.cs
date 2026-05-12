using System.Collections.Generic;
using System.Linq;
using Adventure.Infrastructure.State;
using UnityEngine;

[CreateAssetMenu(fileName = "SinglePlayerStartConfiguration", menuName = "Game/Single Player Start Configuration")]
public sealed class SinglePlayerStartConfigurationSO : ScriptableObject
{
    [Header("Flow")]
    [SerializeField] private SinglePlayerStartMode startMode = SinglePlayerStartMode.Adventure;
    [SerializeField] private GameMode currentGameMode = GameMode.SinglePlayer;

    [Header("Scenes")]
    [SerializeField] private SceneLoader.Scene adventureScene = SceneLoader.Scene.Adventure;
    [SerializeField] private SceneLoader.Scene battleScene = SceneLoader.Scene.GridFight;

    [Header("Armies")]
    [SerializeField] private ArmyLineupSO adventurePlayerStartArmy;
    [SerializeField] private ArmyLineupSO directBattlePlayerStartArmy;
    [SerializeField] private ArmyLineupSO directBattleEnemyArmy;
    [SerializeField] private ArmyLineupSO directBattleVictoryReward;
    [SerializeField] private ArmyLineupSO currentPlayerArmy;

    [Header("Battle Grid")]
    [SerializeField, Min(1)] private int gridWidth = 10;
    [SerializeField, Min(1)] private int gridHeight = 10;
    [SerializeField] private int playerFrontlineY = 1;
    [SerializeField] private int enemyFrontlineY = 8;
    [SerializeField, Min(1)] private int columnSpacing = 1;
    [SerializeField] private Team playerTeam = Team.Blue;
    [SerializeField] private Team battlefieldBottomTeam = Team.Blue;

    [Header("Runtime Battle Configuration")]
    [SerializeField] private List<GridContentEntrySO> availableConfigurations = new();
    [SerializeField] private int selectedConfigurationIndex;

    public SinglePlayerStartMode StartMode => startMode;
    public SceneLoader.Scene AdventureScene => adventureScene;
    public SceneLoader.Scene BattleScene => battleScene;
    public ArmyLineupSO AdventurePlayerStartArmy => adventurePlayerStartArmy;
    public ArmyLineupSO DirectBattlePlayerStartArmy => directBattlePlayerStartArmy;
    public ArmyLineupSO PlayerStartArmy => GetPlayerStartArmy(startMode);
    public ArmyLineupSO DirectBattleEnemyArmy => directBattleEnemyArmy;
    public ArmyLineupSO DirectBattleVictoryReward => directBattleVictoryReward;
    public string DirectBattleVictoryRewardId => $"single-player-start:{name}:direct-battle-victory-reward";
    public int GridWidth => Mathf.Max(1, gridWidth);
    public int GridHeight => Mathf.Max(1, gridHeight);
    public int PlayerFrontlineY => Mathf.Clamp(playerFrontlineY, 0, GridHeight - 1);
    public int EnemyFrontlineY => Mathf.Clamp(enemyFrontlineY, 0, GridHeight - 1);
    public int ColumnSpacing => Mathf.Max(1, columnSpacing);
    public Team PlayerTeam => playerTeam == Team.None ? Team.Blue : playerTeam;
    public Team BattlefieldBottomTeam => battlefieldBottomTeam == Team.None ? PlayerTeam : battlefieldBottomTeam;
    public List<GridContentEntrySO> AvailableConfigs => availableConfigurations;
    public GameMode CurrentGameMode => currentGameMode;
    public bool HasConfiguration => availableConfigurations.Count > 0;
    public IReadOnlyList<UnitStackData> GetPlayerArmyData()
    {
        return GetPlayerArmyData(startMode);
    }

    public ArmyLineupSO GetPlayerStartArmy(SinglePlayerStartMode mode)
    {
        return mode == SinglePlayerStartMode.DirectBattle
            ? directBattlePlayerStartArmy
            : adventurePlayerStartArmy;
    }

    public IReadOnlyList<UnitStackData> GetPlayerArmyData(SinglePlayerStartMode mode)
    {
        var army = GetPlayerStartArmy(mode);
        return army != null
            ? army.Convert()
            : System.Array.Empty<UnitStackData>();
    }

    public IReadOnlyList<UnitStackData> GetDirectBattleEnemyArmyData()
    {
        return directBattleEnemyArmy != null
            ? directBattleEnemyArmy.Convert()
            : System.Array.Empty<UnitStackData>();
    }

    public IReadOnlyList<UnitStackData> GetDirectBattleRewardData()
    {
        return directBattleVictoryReward != null
            ? directBattleVictoryReward.Convert()
            : System.Array.Empty<UnitStackData>();
    }

    public IReadOnlyList<GridContentEntrySO> GetAvailableConfigurations()
    {
        return availableConfigurations;
    }

    public GridContentEntrySO GetSelectedConfiguration()
    {
        return selectedConfigurationIndex >= 0 && selectedConfigurationIndex < availableConfigurations.Count
            ? availableConfigurations[selectedConfigurationIndex]
            : null;
    }

    public bool EnsureSelectedBattleConfiguration()
    {
        if (GetSelectedConfiguration() != null)
        {
            EnsureDirectBattlePostBattleContext();
            return true;
        }

        if (startMode != SinglePlayerStartMode.DirectBattle)
        {
            return false;
        }

        ApplyDirectBattleStart();
        return GetSelectedConfiguration() != null;
    }

    public void EnsureDirectBattlePostBattleContext()
    {
        if (startMode != SinglePlayerStartMode.DirectBattle
            || BattleStateCache.HasPendingRegularPostBattleDialog()
            || !BattleStateCache.CanScheduleInitialBattleReturnNpcDialog())
        {
            return;
        }

        BattleStateCache.SetReturnScene(adventureScene);
        if (!BattleStateCache.HasPendingInitialBattleReturnNpcDialog())
        {
            BattleStateCache.ScheduleInitialBattleReturnNpcDialog(directBattleEnemyArmy);
        }

        if (!BattleStateCache.HasPendingBattleReward())
        {
            BattleStateCache.ScheduleBattleReward(GetDirectBattleRewardData(), DirectBattleVictoryRewardId);
        }
    }

    public int GetSelectedConfigurationIndex()
    {
        return selectedConfigurationIndex;
    }

    public ArmyLineupSO GetPlayerArmy()
    {
        return currentPlayerArmy != null ? currentPlayerArmy : PlayerStartArmy;
    }

    public void SetAvailableConfigurations(List<GridContentEntrySO> configs)
    {
        availableConfigurations = configs?.ToList() ?? new List<GridContentEntrySO>();
    }

    public void SetSelectedConfiguration(int index)
    {
        selectedConfigurationIndex = index;
    }

    public void SetPlayerArmy(ArmyLineupSO playerArmy)
    {
        currentPlayerArmy = playerArmy;
    }

    public void SetTeam(Team team)
    {
        playerTeam = team;
    }

    public void SetBattlefieldBottomTeam(Team team)
    {
        battlefieldBottomTeam = team;
    }

    public void SetGameMode(GameMode mode)
    {
        currentGameMode = mode;
    }

    public void ApplyAdventureStart()
    {
        BattleStateCache.ClearClaimedBattleRewards();
        var playerArmy = GetPlayerArmyData(SinglePlayerStartMode.Adventure);
        BattleStateCache.StoreInventorySnapshot(playerArmy);
        BattleStateCache.SetReturnScene(adventureScene);
        BattleStateCache.ClearPendingPostBattleDialog();
        BattleStateCache.ScheduleBattleReward(System.Array.Empty<UnitStackData>());

        SetPlayerArmy(adventurePlayerStartArmy);
        SetGameMode(GameMode.SinglePlayer);
        SetTeam(PlayerTeam);
        SetBattlefieldBottomTeam(BattlefieldBottomTeam);
    }

    public void ApplyDirectBattleStart()
    {
        BattleStateCache.ClearClaimedBattleRewards();
        BattleStateCache.ResetInitialBattleReturnNpcDialogFlow();
        SetPlayerArmy(directBattlePlayerStartArmy);
        SetGameMode(GameMode.SinglePlayer);
        SetTeam(PlayerTeam);
        SetBattlefieldBottomTeam(BattlefieldBottomTeam);

        var gridContent = CreateInstance<GridContentEntrySO>();
        gridContent.name = "RuntimeMainMenuDirectBattle";
        gridContent.Width = GridWidth;
        gridContent.Height = GridHeight;
        gridContent.contents = new List<GridContentEntrySO.UnitContent>();
        gridContent.contents.AddRange(CreateFormation(
            GetPlayerArmyData(SinglePlayerStartMode.DirectBattle),
            PlayerFrontlineY,
            BattlefieldBottomTeam));
        gridContent.contents.AddRange(CreateFormation(
            GetDirectBattleEnemyArmyData(),
            EnemyFrontlineY,
            ResolveEnemyTeam(BattlefieldBottomTeam)));

        SetAvailableConfigurations(new List<GridContentEntrySO> { gridContent });
        SetSelectedConfiguration(0);

        BattleStateCache.StoreInventorySnapshot(GetPlayerArmyData(SinglePlayerStartMode.DirectBattle));
        BattleStateCache.SetReturnScene(adventureScene);
        BattleStateCache.ScheduleInitialBattleReturnNpcDialog(directBattleEnemyArmy);
        BattleStateCache.ScheduleBattleReward(GetDirectBattleRewardData(), DirectBattleVictoryRewardId);
    }

    public void Clear()
    {
        availableConfigurations.Clear();
    }

    private IEnumerable<GridContentEntrySO.UnitContent> CreateFormation(
        IReadOnlyList<UnitStackData> stacks,
        int y,
        Team team)
    {
        if (stacks == null)
            yield break;

        var x = 0;
        var row = y;
        var rowDirection = team == BattlefieldBottomTeam ? 1 : -1;

        foreach (var stack in stacks)
        {
            if (stack.Amount <= 0)
                continue;

            yield return new GridContentEntrySO.UnitContent
            {
                Team = team,
                unitType = stack.UnitType,
                Amount = Mathf.Max(1, stack.Amount),
                X = Mathf.Clamp(x, 0, GridWidth - 1),
                Y = Mathf.Clamp(row, 0, GridHeight - 1)
            };

            x += ColumnSpacing;
            if (x < GridWidth)
                continue;

            x %= GridWidth;
            row += rowDirection;
        }
    }

    private static Team ResolveEnemyTeam(Team playerTeam)
    {
        return playerTeam switch
        {
            Team.Red => Team.Blue,
            Team.Green => Team.Yellow,
            Team.Yellow => Team.Green,
            _ => Team.Red
        };
    }
}
