using System;
using TMPro;
using UniRx;
using UnityEngine;
using Zenject;

public class InGameUI : MonoBehaviour
{
    [Inject] private TurnSystem _turnSystem;
    [SerializeField] private TextMeshProUGUI turnNumber;
    [SerializeField] private Animator turnNumberAnimator;
    [SerializeField] private Animator battleStateAnimator;
    [SerializeField] private TextMeshProUGUI battleState;
    [SerializeField] private TextMeshProUGUI isMyTurnText;
    private int currentTurn;
    private bool _subscribed = false;
    
    [Inject]
    private void Init()
    {
        // Subscribe to GameViewModel events instead of direct domain access
        _turnSystem.ActiveObject.Subscribe(OnUnitTurnStarted);
        _subscribed = true;
    }
    private void OnUnitTurnStarted(ICombatObject combatObject)
    {
        if(_turnSystem.IsMyTurn)
            isMyTurnText.text = "Your Turn " + combatObject.ToString();
        else
            isMyTurnText.text = "Enemy Turn " + combatObject.ToString();
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