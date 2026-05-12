using System;
using Game.Events;
using Adventure.Infrastructure.State;
using TMPro;
using UniRx;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using CustomEventBus;
using SharedView;

public class GridFightEndedPanel : CanvasGroupVisibilityPanelBase, IDisposable
{
    [SerializeField] private TextMeshProUGUI BattleResult;
    [SerializeField] private Button ReturnForestSceneButton;

    private ITurnStateViewModel _turnState;
    private EventBus _gameplayEvents;
    private SinglePlayerStartConfigurationSO _startConfiguration;
    private readonly CompositeDisposable _disposables = new CompositeDisposable();
    private bool _resultHandled;

    [Inject]
    public void Construct(
        ITurnStateViewModel turnState,
        EventBus gameplayEvents,
        [InjectOptional] SinglePlayerStartConfigurationSO startConfiguration = null)
    {
        _turnState = turnState;
        _gameplayEvents = gameplayEvents;
        _startConfiguration = startConfiguration;
        if (_turnState != null)
        {
            _turnState.BattleStateProperty
                .Subscribe(HandleBattleState)
                .AddTo(_disposables);

            HandleBattleState(_turnState.BattleStateProperty.Value);
        }
    }

    protected override void Awake()
    {
        base.Awake();

        if (ReturnForestSceneButton != null)
        {
            ReturnForestSceneButton.onClick.AddListener(OnReturnClicked);
        }
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

        _startConfiguration?.EnsureDirectBattlePostBattleContext();
        BattleStateCache.CompleteBattle(playerWon);
        _gameplayEvents?.Invoke(new BattleCompletedCustomEvent(playerWon));

        if (BattleResult != null)
        {
            BattleResult.text = playerWon ? "Victory!" : "Defeat...";
        }

        if (ShouldReturnImmediately(playerWon))
        {
            ReturnToConfiguredScene();
            return;
        }

        ShowPanel();
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
        ReturnToConfiguredScene();
    }

    private bool ShouldReturnImmediately(bool playerWon)
    {
        if (!playerWon || !BattleStateCache.HasPendingPostBattleFlow())
        {
            return false;
        }

        var networkManager = NetworkManager.Singleton;
        return networkManager == null || !networkManager.IsListening;
    }

    private void ReturnToConfiguredScene()
    {
        var targetScene = BattleStateCache.GetReturnSceneOrDefault();
        var networkManager = NetworkManager.Singleton;
        if (networkManager != null && networkManager.IsListening)
        {
            if (networkManager.IsServer || networkManager.IsHost)
            {
                networkManager.SceneManager.LoadScene(targetScene.ToString(), UnityEngine.SceneManagement.LoadSceneMode.Single);
                return;
            }

#if UNITY_2023_1_OR_NEWER
            var gateway = UnityEngine.Object.FindFirstObjectByType<GameNetworkCommandGateway>();
#else
            var gateway = UnityEngine.Object.FindObjectOfType<GameNetworkCommandGateway>();
#endif
            if (gateway != null && gateway.RequestReturnToAdventure())
                return;

            Debug.LogWarning("[GridFightEndedPanel] Waiting for the host to return the party to the adventure scene.", this);
            return;
        }

        SceneLoader.Load(targetScene);
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }

    protected override void OnDestroy()
    {
        Dispose();
        base.OnDestroy();
    }
}



