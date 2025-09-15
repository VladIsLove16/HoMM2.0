using System;
using TMPro;
using UnityEngine;
using Zenject;

public class InGameUI : MonoBehaviour
{
    [Inject] private  TurnSystem _turnSystem;
    [SerializeField] private TextMeshProUGUI turnNumber;
    [SerializeField] private Animator turnNumberAnimator;
    [SerializeField] private Animator battleStateAnimator;
    [SerializeField] private TextMeshProUGUI battleState;
    private int currentTurn;
    private bool _subscribed = false;
    [Inject]
    private void Init()
    {
        _turnSystem.OnBattleStateChanged += HandleBattleStateChanged;
        _subscribed = true;
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