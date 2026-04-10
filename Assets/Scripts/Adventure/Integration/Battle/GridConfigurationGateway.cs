using System.Collections.Generic;
using Adventure.Infrastructure.State;
using Unity.Netcode;
using UnityEngine;

namespace Adventure.Integration.Battle
{
    public sealed class GridConfigurationGateway : MonoBehaviour, IGridConfigurationGateway
    {
        private static GridConfigurationGateway _instance;

        [SerializeField] private GameConfigurationService configurationService;
        [SerializeField] private SceneLoader.Scene battleScene = SceneLoader.Scene.GridFight;
        [SerializeField] private int gridWidth = 10;
        [SerializeField] private int gridHeight = 10;
        [SerializeField] private Team playerTeam = Team.Blue;

        private GridContentEntrySO _runtimeContent;
        private BattleSetupPayload _lastPayload;

        public SceneLoader.Scene BattleScene => battleScene;
        public int GridWidth => gridWidth;
        public int GridHeight => gridHeight;
        public Team PlayerTeam => playerTeam;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            EnsureRuntimeContent();
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }

        public void PrepareBattle(BattleSetupPayload payload, ArmyFormationResolver formationResolver)
        {
            if (configurationService == null)
            {
                Debug.LogError("[GridConfigurationGateway] ConfigurationService is not assigned.", this);
                return;
            }

            EnsureRuntimeContent();
            _lastPayload = payload;

            _runtimeContent.Width = Mathf.Max(1, gridWidth);
            _runtimeContent.Height = Mathf.Max(1, gridHeight);
            _runtimeContent.contents = BuildUnitContents(payload, formationResolver);

            configurationService.SetAvailableConfigurations(new List<GridContentEntrySO> { _runtimeContent });
            configurationService.SetSelectedConfiguration(0);
            configurationService.SetGameMode(NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening
                ? GameMode.Multiplayer
                : configurationService.CurrentGameMode);
            configurationService.SetTeam(payload.LocalTeam != Team.None ? payload.LocalTeam : ResolveConfiguredPlayerTeam());
            configurationService.SetBattlefieldBottomTeam(payload.BattlefieldBottomTeam != Team.None
                ? payload.BattlefieldBottomTeam
                : ResolveConfiguredPlayerTeam());

            BattleStateCache.SetReturnScene(payload.ReturnScene);
        }

        private List<GridContentEntrySO.UnitContent> BuildUnitContents(BattleSetupPayload payload, ArmyFormationResolver resolver)
        {
            var contents = new List<GridContentEntrySO.UnitContent>();
            var bottomTeam = payload.BattlefieldBottomTeam != Team.None ? payload.BattlefieldBottomTeam : ResolveConfiguredPlayerTeam();
            var topTeam = ResolveEnemyTeam(bottomTeam);
            if (resolver != null)
            {
                contents.AddRange(OverrideTeam(resolver.CreatePlayerFormation(payload.Armies.PlayerUnits, gridWidth), bottomTeam));
                contents.AddRange(OverrideTeam(resolver.CreateEnemyFormation(payload.Armies.EnemyUnits, gridWidth), topTeam));
            }
            else
            {
                contents.AddRange(CreateFlatFormation(payload.Armies.PlayerUnits, gridWidth, 0, bottomTeam));
                contents.AddRange(CreateFlatFormation(payload.Armies.EnemyUnits, gridWidth, gridHeight - 1, topTeam));
            }

            return contents;
        }

        private IEnumerable<GridContentEntrySO.UnitContent> CreateFlatFormation(
            IReadOnlyList<UnitStackData> stacks,
            int width,
            int y,
            Team team)
        {
            if (stacks == null)
                yield break;

            var x = 0;
            foreach (var stack in stacks)
            {
                if (stack.Amount <= 0)
                    continue;

                yield return new GridContentEntrySO.UnitContent
                {
                    Team = team,
                    unitType = stack.UnitType,
                    Amount = Mathf.Max(1, stack.Amount),
                    X = Mathf.Clamp(x, 0, Mathf.Max(0, width - 1)),
                    Y = Mathf.Clamp(y, 0, Mathf.Max(0, gridHeight - 1))
                };

                x = (x + 1) % Mathf.Max(1, width);
            }
        }

        private void EnsureRuntimeContent()
        {
            if (_runtimeContent != null)
                return;

            _runtimeContent = ScriptableObject.CreateInstance<GridContentEntrySO>();
            _runtimeContent.name = "RuntimeBattleConfig";
        }

        private Team ResolveConfiguredPlayerTeam()
        {
            if (configurationService != null && configurationService.Team != Team.None)
                return configurationService.Team;

            return playerTeam != Team.None ? playerTeam : Team.Blue;
        }

        private static Team ResolveEnemyTeam(Team localTeam)
        {
            return localTeam switch
            {
                Team.Red => Team.Blue,
                Team.Green => Team.Yellow,
                Team.Yellow => Team.Green,
                _ => Team.Red
            };
        }

        private static IEnumerable<GridContentEntrySO.UnitContent> OverrideTeam(
            IEnumerable<GridContentEntrySO.UnitContent> contents,
            Team team)
        {
            if (contents == null)
                yield break;

            foreach (var content in contents)
            {
                content.Team = team;
                yield return content;
            }
        }
    }
}
