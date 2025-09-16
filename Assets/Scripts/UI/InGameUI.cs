using System;
using TMPro;
using UniRx;
using UnityEngine;
using Zenject;

public class InGameUI : MonoBehaviour
{
    [Inject] private  TurnSystem _turnSystem;
    [SerializeField] private TextMeshProUGUI turnNumber;
    [SerializeField] private Animator turnNumberAnimator;
    [SerializeField] private Animator battleStateAnimator;
    [SerializeField] private TextMeshProUGUI battleState;
    [SerializeField] private TextMeshProUGUI isMyTurnText;
    [Inject] private GameNetworkCommandGateway gameNetworkCommandGateway;
    private int currentTurn;
    private bool _subscribed = false;
    [Inject]
    private void Init()
    {
        _turnSystem.OnBattleStateChanged += HandleBattleStateChanged;
        _subscribed = true;
        _turnSystem.ActiveObject.Subscribe(OngameNetworkCommandGateway_TurnOwnerChanged);
    }
    private void OngameNetworkCommandGateway_TurnOwnerChanged(ICombatObject combatObject)
    {
        isMyTurnText.text = _turnSystem.IsMyTurn ? "Your Turn" : "Wait for player to move";
        return;
    }

    private void HandleBattleStateChanged(BattleState state)
    {
        switch(state)
        {
            case BattleState.blueTeamWins:
                battleStateAnimator.SetTrigger("blueTeamWins");
                
                break;
            case BattleState.redTeamWins:
                battleStateAnimator.SetTrigger("redTeamWins");
                break;
            case BattleState.inProgress:
                battleStateAnimator.SetTrigger("inProgress");
                break;
        }
        battleState.text = state.ToString();
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
            _turnSystem.OnBattleStateChanged -= HandleBattleStateChanged;
            _subscribed = false;
        }
    }
}