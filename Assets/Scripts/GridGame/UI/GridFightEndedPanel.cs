using System;
using Game.Events;
using Adventure.Infrastructure.State;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using CustomEventBus;

public class GridFightEndedPanel : MonoBehaviour, IDisposable
{
    [SerializeField] private TextMeshProUGUI BattleResult;
    [SerializeField] private Button ReturnForestSceneButton;

    private ITurnStateViewModel _turnState;
    private EventBus _gameplayEvents;
    private readonly CompositeDisposable _disposables = new CompositeDisposable();
    private bool _resultHandled;

    [Inject]
    public void Construct(ITurnStateViewModel turnState, EventBus gameplayEvents)
    {
        _turnState = turnState;
        _gameplayEvents = gameplayEvents;
        if (_turnState != null)
        {
            _turnState.BattleStateProperty
                .Subscribe(HandleBattleState)
                .AddTo(_disposables);

            HandleBattleState(_turnState.BattleStateProperty.Value);
        }
    }

    private void Awake()
    {
        if (ReturnForestSceneButton != null)
        {
            ReturnForestSceneButton.onClick.AddListener(OnReturnClicked);
        }

        gameObject.SetActive(false);
    }

    private void HandleBattleState(BattleState state)
    {
        if (_resultHandled)
            return;

        bool finished = state == BattleState.blueTeamWins || state == BattleState.redTeamWins;
        if (!finished)
            return;

        bool playerWon = DeterminePlayerVictory(state);
        _resultHandled = true;

        BattleStateCache.CompleteBattle(playerWon);
        _gameplayEvents?.Invoke(new BattleCompletedCustomEvent(playerWon));

        if (BattleResult != null)
        {
            BattleResult.text = playerWon ? "Victory!" : "Defeat...";
        }

        gameObject.SetActive(true);
    }

    private bool DeterminePlayerVictory(BattleState state)
    {
        if (_turnState == null)
            return state == BattleState.blueTeamWins;

        return state switch
        {
            BattleState.blueTeamWins => _turnState.LocalTeam == Team.Blue,
            BattleState.redTeamWins => _turnState.LocalTeam == Team.Red,
            _ => false
        };
    }

    private void OnReturnClicked()
    {
        var targetScene = BattleStateCache.GetReturnSceneOrDefault();
        Loader.Load(targetScene);
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }

    private void OnDestroy()
    {
        Dispose();
    }
}



