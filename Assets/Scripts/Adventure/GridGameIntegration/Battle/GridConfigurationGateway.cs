using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Adventure.Integration.Battle
{
    public sealed class GridConfigurationGateway : MonoBehaviour, IGridConfigurationGateway
    {
        [SerializeField] private GameConfigurationService configurationService;
        [SerializeField] private string battleSceneName = "SampleScene";
        [SerializeField] private int gridWidth = 12;
        [SerializeField] private int gridHeight = 8;
        [SerializeField] private Team playerTeam = Team.Blue;
        [SerializeField] private int playerFrontlineX = 1;
        [SerializeField] private int enemyFrontlineX = 10;
        [SerializeField, Min(1)] private int rowSpacing = 1;

        //public void ConfigureBattle(IReadOnlyList<UnitStackData> playerLineup, IReadOnlyList<UnitStackData> enemyLineup)
        //{
        //    var resolver = new ArmyFormationResolver(playerFrontlineX, enemyFrontlineX, Mathf.Max(1, rowSpacing));
        //    var playerSlots = resolver.ResolveForPlayer(playerLineup ?? Array.Empty<UnitStackData>());
        //    var enemySlots = resolver.ResolveForEnemy(enemyLineup ?? Array.Empty<UnitStackData>());
        //    ApplyConfiguration(playerSlots, enemySlots);
        //}

        public void ApplyConfiguration(ArmyFormation playerSlots, ArmyFormation enemySlots)
        {
            if (configurationService == null)
            {
                Debug.LogError("Configuration service is not assigned", this);
                return;
            }

            var entry = ScriptableObject.CreateInstance<GridContentEntrySO>();
            entry.Width = Mathf.Max(1, gridWidth);
            entry.Height = Mathf.Max(1, gridHeight);
            entry.contents = new List<GridContentEntrySO.UnitContent>();

            entry.Append(ToContents(playerSlots));
            entry.Append(ToContents(enemySlots));

            configurationService.SetAvailableConfigurations(new List<GridContentEntrySO> { entry });
            configurationService.SetSelectedConfiguration(0);
            configurationService.SetTeam(playerTeam);
        }

        public void LoadBattleScene()
        {
            if (string.IsNullOrEmpty(battleSceneName))
            {
                Debug.LogError("Battle scene name is not set", this);
                return;
            }

            SceneManager.LoadScene(battleSceneName, LoadSceneMode.Single);
        }

        private static void Append(GridContentEntrySO entry, IReadOnlyList<GridSlot> slots)
        {
            if (slots == null) return;
            foreach (var slot in slots)
            {
                entry.contents.Add(new GridContentEntrySO.UnitContent
                {
                    unitType = slot.UnitType,
                    Amount = Mathf.Max(1, slot.Amount),
                    X = slot.X,
                    Y = slot.Y,
                    Team = slot.Team
                });
            }
        }
        private GridContentEntrySO.UnitContent ToContent(GridSlot slot)
        {
           return new GridContentEntrySO.UnitContent
            {
                unitType = slot.UnitType,
                Amount = Mathf.Max(1, slot.Amount),
                X = slot.X,
                Y = slot.Y,
                Team = slot.Team
            };
        }
        private GridContentEntrySO.UnitContent[] ToContents(ArmyFormation slots)
        {
            GridContentEntrySO.UnitContent[] result =new GridContentEntrySO.UnitContent[slots.Count];
           for (int i = 0; i < slots.Count; i++)
            {
                result[i] = ToContent(slots[i]);
            }
            return result;
        }
    }
}
