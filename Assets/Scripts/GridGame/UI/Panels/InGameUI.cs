using CustomEventBus;
using GridGame.UI;
using System;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class InGameUI : MonoBehaviour
{
    private ITurnStateViewModel _turnState;
    private IBattleControlModeService _battleControlModes;
    [Inject] private IGameCommandExecutor _gameCommandExecutor;
    [SerializeField] private Button StartBattle;
    [SerializeField] ForceDefeatAndReturnButton LoseBattle;
    [SerializeField] private TextMeshProUGUI turnNumber;
    [SerializeField] private Animator turnNumberAnimator;
    [SerializeField] private Animator battleStateAnimator;
    [SerializeField] private TextMeshProUGUI battleState;
    [SerializeField] private TextMeshProUGUI isMyTurnText;
    private int currentTurn;
    private readonly CompositeDisposable _disposables = new();
    private EventBus eventBus;

    [Inject]
    public void Init(ITurnStateViewModel turnState, EventBus eventBus, [InjectOptional] IBattleControlModeService battleControlModes = null)
    {
        _turnState = turnState;
        _battleControlModes = battleControlModes;
        _turnState.ActiveObject.Subscribe(OnUnitTurnStarted).AddTo(_disposables);
        _turnState.BattleStateProperty.Subscribe(OnTurnStateChanged).AddTo(_disposables);
        _battleControlModes?.TeamModeChanged.Subscribe(_ => OnUnitTurnStarted(_turnState.ActiveObject.Value)).AddTo(_disposables);
        StartBattle.onClick.AddListener(() => _gameCommandExecutor.StartBattle());
        this.eventBus = eventBus;
        LoseBattle.Construct(eventBus);
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
