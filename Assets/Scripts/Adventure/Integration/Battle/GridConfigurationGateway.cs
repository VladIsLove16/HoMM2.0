using System.Collections.Generic;
using Adventure.Infrastructure.State;
using UnityEngine;

namespace Adventure.Integration.Battle
{

    public sealed class GridConfigurationGateway : MonoBehaviour, IGridConfigurationGateway
    {
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
            DontDestroyOnLoad(gameObject);
            if (_runtimeContent == null)
            {
                _runtimeContent = ScriptableObject.CreateInstance<GridContentEntrySO>();
                _runtimeContent.name = "RuntimeBattleConfig";
            }
        }

        public void PrepareBattle(BattleSetupPayload payload, ArmyFormationResolver formationResolver)
        {
            if (configurationService == null)
            {
                Debug.LogError("[GridConfigurationGateway] ConfigurationService is not assigned.", this);
                return;
            }

            if (!_runtimeContent)
            {
                _runtimeContent = ScriptableObject.CreateInstance<GridContentEntrySO>();
                _runtimeContent.name = "RuntimeBattleConfig";
            }

            _lastPayload = payload;

            _runtimeContent.Width = Mathf.Max(1, gridWidth);
            _runtimeContent.Height = Mathf.Max(1, gridHeight);
            _runtimeContent.contents = BuildUnitContents(payload, formationResolver);

            configurationService.SetAvailableConfigurations(new List<GridContentEntrySO> { _runtimeContent });
            configurationService.SetSelectedConfiguration(0);
            configurationService.SetGameMode(GameMode.SinglePlayer);
            configurationService.SetTeam(playerTeam);

            BattleStateCache.SetReturnScene(payload.ReturnScene);
        }

        private List<GridContentEntrySO.UnitContent> BuildUnitContents(BattleSetupPayload payload, ArmyFormationResolver resolver)
        {
            var contents = new List<GridContentEntrySO.UnitContent>();
            if (resolver != null)
            {
                contents.AddRange(resolver.CreatePlayerFormation(payload.Armies.PlayerUnits, gridWidth));
                contents.AddRange(resolver.CreateEnemyFormation(payload.Armies.EnemyUnits, gridWidth));
            }
            else
            {
                contents.AddRange(CreateFlatFormation(payload.Armies.PlayerUnits, gridWidth, 0, Team.Blue));
                contents.AddRange(CreateFlatFormation(payload.Armies.EnemyUnits, gridWidth, gridHeight - 1, Team.Red));
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
    }
}
