using System;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class InGameUI : MonoBehaviour
{
    [Inject] private TurnSystem _turnSystem;
    [Inject] private IGameCommandExecutor _gameCommandExecutor;
    [SerializeField] private Button StartBattle;
    [SerializeField] private TextMeshProUGUI turnNumber;
    [SerializeField] private Animator turnNumberAnimator;
    [SerializeField] private Animator battleStateAnimator;
    [SerializeField] private TextMeshProUGUI battleState;
    [SerializeField] private TextMeshProUGUI isMyTurnText;
    private int currentTurn;
    private bool _subscribed = false;
    
    [Inject]
    public void Init()
    {
        // Subscribe to GameViewModel events instead of direct domain access
        _turnSystem.ActiveObject.Subscribe(OnUnitTurnStarted);
        _turnSystem.CurrentBattleState.Subscribe(OnTurnSystem_BattleStateChanged);
        StartBattle.onClick.AddListener(() => _gameCommandExecutor.StartBattle());
        _subscribed = true;
    }

    private void OnTurnSystem_BattleStateChanged(BattleState state)
    {
        battleState.text = state.ToString();
    }

    private void OnUnitTurnStarted(ICombatObject combatObject)
    {
        var newtext = string.Empty;
        if (combatObject == null)
            newtext = "Game starting";
        else if (_turnSystem.IsMyTurn)
            newtext = "Your Turn " + combatObject.ToString();
        else
            newtext = "Enemy Turn " + combatObject.ToString();
        isMyTurnText.text = newtext;
    }

    private void OnDestroy()
    {
        
    }

    public void OnTurnNumberChanged(int turn)
    {
        turnNumberAnimator.SetTrigger("turnChanged");
        currentTurn = turn;
    }
    public void UpdateTurnText()
    {
        turnNumber.text = "Turn " + currentTurn.ToString();
    }
    public void Dispose()
    {
        if (_subscribed)
        {
            _subscribed = false;
        }
    }
}