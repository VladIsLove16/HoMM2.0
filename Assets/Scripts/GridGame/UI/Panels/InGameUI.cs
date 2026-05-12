using CustomEventBus;
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
    [SerializeField] private TextMeshProUGUI turnNumber;
    [SerializeField] private Animator turnNumberAnimator;
    [SerializeField] private Animator battleStateAnimator;
    [SerializeField] private TextMeshProUGUI battleState;
    [SerializeField] private TextMeshProUGUI isMyTurnText;
    private int currentTurn;
    private readonly CompositeDisposable _disposables = new();

    [Inject]
    public void Init(ITurnStateViewModel turnState, EventBus eventBus, [InjectOptional] IBattleControlModeService battleControlModes = null)
    {
        _turnState = turnState;
        _battleControlModes = battleControlModes;
        if (_turnState != null)
        {
            _turnState.ActiveObject.Subscribe(OnUnitTurnStarted).AddTo(_disposables);
            _turnState.BattleStateProperty.Subscribe(OnTurnStateChanged).AddTo(_disposables);
        }
        else
        {
            Debug.LogWarning("[InGameUI] TurnStateViewModel is not injected.", this);
        }

        _battleControlModes?.TeamModeChanged.Subscribe(_ => OnUnitTurnStarted(_turnState?.ActiveObject.Value)).AddTo(_disposables);

        if (StartBattle != null)
        {
            StartBattle.onClick.AddListener(() => _gameCommandExecutor?.StartBattle());
        }
        else
        {
            Debug.LogWarning("[InGameUI] StartBattle button is not assigned.", this);
        }
    }

    private void OnTurnStateChanged(BattleState state)
    {
        if (battleState == null)
            return;

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
        if (isMyTurnText == null)
        {
            Debug.LogWarning("[InGameUI] IsMyTurnText is not assigned.", this);
            return;
        }

        isMyTurnText.text = newtext;
    }

    private void OnDestroy()
    {
        _disposables.Dispose();
    }

    public void OnTurnNumberChanged(int turn)
    {
        turnNumberAnimator?.SetTrigger("turnChanged");
        currentTurn = turn;
    }
    public void UpdateTurnText()
    {
        if (turnNumber == null)
            return;

        turnNumber.text = "Turn " + currentTurn.ToString();
    }
    public void Dispose()
    {
        _disposables.Dispose();
    }
}
