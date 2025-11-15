using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class InGameUI_TurnState_EditModeTests
{
    [Test]
    public void Init_BindsBattleStateAndActiveObject()
    {
        var uiGo = new GameObject(nameof(InGameUI));
        var ui = uiGo.AddComponent<InGameUI>();
        var button = new GameObject("StartBattleButton").AddComponent<Button>();
        var battleStateText = new GameObject("BattleState").AddComponent<TextMeshProUGUI>();
        var isMyTurnText = new GameObject("IsMyTurn").AddComponent<TextMeshProUGUI>();

        SetField(ui, "StartBattle", button);
        SetField(ui, "battleState", battleStateText);
        SetField(ui, "isMyTurnText", isMyTurnText);

        var executor = new StubExecutor();
        SetField(ui, "_gameCommandExecutor", executor);

        var turnState = new StubTurnState();
        ui.Init(turnState);

        turnState.BattleStateProperty.Value = BattleState.inProgress;
        Assert.That(battleStateText.text, Is.EqualTo(BattleState.inProgress.ToString()));

        turnState.SetIsMyTurn(true);
        turnState.ActiveObjectProperty.Value = new StubCombatObject(Team.Blue, new Vector2Int(1, 2));
        Assert.That(isMyTurnText.text, Does.StartWith("Your Turn"));

        turnState.SetIsMyTurn(false);
        turnState.ActiveObjectProperty.Value = new StubCombatObject(Team.Red, new Vector2Int(0, 0));
        Assert.That(isMyTurnText.text, Does.StartWith("Enemy Turn"));

        button.onClick.Invoke();
        Assert.That(executor.StartBattleInvoked, Is.True);

        UnityEngine.Object.DestroyImmediate(uiGo);
    }

    private static void SetField(object instance, string fieldName, object value)
    {
        var field = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        field.SetValue(instance, value);
    }

    private sealed class StubExecutor : IGameCommandExecutor
    {
        public bool StartBattleInvoked { get; private set; }
        public ActionType? LastType { get; private set; }
        public ActionContext LastContext { get; private set; }

        public bool Execute(ActionType type, ActionContext ctx)
        {
            LastType = type;
            LastContext = ctx;
            return true;
        }

        public void StartBattle()
        {
            StartBattleInvoked = true;
        }
    }

    private sealed class StubTurnState : ITurnStateViewModel
    {
        public ReactiveProperty<BattleState> BattleStateProperty { get; } = new(global::BattleState.replacement);
        public ReactiveProperty<ICombatObject> ActiveObjectProperty { get; } = new();
        private bool _isMyTurn;
        private Team _team = Team.Blue;

        public bool IsMyTurn => _isMyTurn;
        public Team LocalTeam => _team;

        public IReadOnlyReactiveProperty<int> TurnNumber => throw new NotImplementedException();

        public IObservable<UnitTurnInfo> UnitAddedStream => throw new NotImplementedException();

        public IReadOnlyReactiveProperty<ICombatObject> ActiveObject => ActiveObjectProperty;

        IReadOnlyReactiveProperty<BattleState> ITurnStateViewModel.BattleStateProperty => BattleStateProperty;

        public IReadOnlyList<ICombatObject> CombatUnits => throw new NotImplementedException();

        public void SetIsMyTurn(bool value)
        {
            _isMyTurn = value;
        }
    }

    private sealed class StubCombatObject : ICombatObject
    {
        public StubCombatObject(Team team, Vector2Int position)
        {
            Team = team;
            Position = position;
        }

        public void TakeTurn()
        {
        }

        public void EndTurn()
        {
        }

        public UnitType UnitType => UnitType.Archer;
        private readonly UnitStats _stats = ScriptableObject.CreateInstance<UnitStats>();
        public UnitStats Stats => _stats;
        public Action<ICombatObject> Died { get; set; }
        public Team Team { get; }
        public Vector2Int Position { get; set; }
        public GridContentType GridContentType => GridContentType.unit;
    }
}

