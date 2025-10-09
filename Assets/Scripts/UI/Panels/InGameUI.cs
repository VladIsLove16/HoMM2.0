using System;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class InGameUI : MonoBehaviour
{
    private ITurnStateViewModel _turnState;
    [Inject] private IGameCommandExecutor _gameCommandExecutor;
    [SerializeField] private Button StartBattle;
    [SerializeField] private TextMeshProUGUI turnNumber;
    [SerializeField] private Animator turnNumberAnimator;
    [SerializeField] private Animator battleStateAnimator;
    [SerializeField] private TextMeshProUGUI battleState;
    [SerializeField] private TextMeshProUGUI isMyTurnText;
    private int currentTurn;
    private readonly CompositeDisposable _disposables = new();

    [Inject]
    public void Init(ITurnStateViewModel turnState)
    {
        _turnState = turnState;
        _turnState.ActiveObject.Subscribe(OnUnitTurnStarted).AddTo(_disposables);
        _turnState.BattleStateProperty.Subscribe(OnTurnStateChanged).AddTo(_disposables);
        StartBattle.onClick.AddListener(() => _gameCommandExecutor.StartBattle());
    }

    private void OnTurnStateChanged(BattleState state)
    {
        battleState.text = state.ToString();
    }

    private void OnUnitTurnStarted(ICombatObject combatObject)
    {
        var newtext = string.Empty;
        if (combatObject == null)
            newtext = "Game starting";
        else if (_turnState.IsMyTurn)
            newtext = "Your Turn " + combatObject;
        else
            newtext = "Enemy Turn " + combatObject;
        isMyTurnText.text = newtext;
    }

    private void OnDestroy()
    {
        _disposables.Dispose();
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
        _disposables.Dispose();
    }
}
