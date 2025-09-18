using System;
using TMPro;
using UniRx;
using UnityEngine;
using Zenject;

public class InGameUI : MonoBehaviour
{
    [Inject] private GameViewModel _gameViewModel;
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
        _gameViewModel.UnitTurnStarted += OnUnitTurnStarted;
        _subscribed = true;
    }
    private void OnUnitTurnStarted(IViewModel unitViewModel)
    {
        // Update UI based on turn changes
        isMyTurnText.text = "Your Turn"; // This should be determined by GameViewModel
    }

    private void OnDestroy()
    {
        if (_gameViewModel != null)
        {
            _gameViewModel.UnitTurnStarted -= OnUnitTurnStarted;
        }
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