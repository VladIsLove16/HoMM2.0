using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Менеджер лобби для управления подключениями игроков и настройками игры.
/// Отвечает только за бизнес-логику и сетевую синхронизацию.
/// </summary>
public class LobbyManager : NetworkBehaviour
{
    private const string GameSceneName = "GridFight";

    [Header("Game Configuration")]
    // Ссылка на ScriptableObject (назначать в инспекторе у префаба/объекта на сцене)
    [SerializeField] private GameConfigurationService GameConfigurationService;

    // Сетевые переменные (хост владеет и реплицирует)
    private NetworkList<LobbyPlayerData> _lobbyPlayers;
    private NetworkVariable<int> _selectedConfigIndex = new NetworkVariable<int>(0);
    private NetworkVariable<bool> _gameStarted = new NetworkVariable<bool>(false);

    // События для UI
    public Action<LobbyPlayerData[]> OnPlayersListChanged;
    public Action<int> OnConfigChanged;
    public Action<bool> OnGameStarted;
    public Action<int,int> OnGridSizeChanged;
    public Action OnHostStarted;
    public Action OnClientStarted;

    private void Awake()
    {
        // Инициализируем NetworkList — по умолчанию пустой.
        _lobbyPlayers = new NetworkList<LobbyPlayerData>();
    }

    public override void OnNetworkSpawn()
    {
        // Подписываемся на изменения (всегда — на хосте и на клиентах)
        _lobbyPlayers.OnListChanged += OnNetworkPlayersListChanged;
        _selectedConfigIndex.OnValueChanged += OnNetworkConfigChanged;
        _gameStarted.OnValueChanged += OnNetworkGameStartedChanged;

        // Добавляем текущего игрока в лобби (клиенты отправляют RPC на сервер)
        if (IsClient || IsHost)
        {
            AddPlayerToLobby();
        }

        // Уведомляем UI о роли
        if (IsHost)
        {
            OnHostStarted?.Invoke();
        }
        else if (IsClient)
        {
            OnClientStarted?.Invoke();
        }

        // Применяем текущие сетевые значения в локальный GameConfigurationService (чтобы UI/локальная логика знали)
        ApplyNetworkStateToLocalService();
    }

    // Публичные методы для UI / кнопок
    public void StartHost()
    {
        // Запускаем хост — сетевые переменные будут контролироваться хостом
        NetworkManager.Singleton.StartHost();
        // выставляем режим и команду (на хосте)
        if (GameConfigurationService != null)
        {
            GameConfigurationService.SetGameMode(GameMode.Multiplayer);
            GameConfigurationService.SetTeam(Team.Blue);
        }
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        // клиент местный — не должен менять глобальную конфигурацию в момент запуска
        if (GameConfigurationService != null)
        {
            GameConfigurationService.SetGameMode(GameMode.Multiplayer);
            GameConfigurationService.SetTeam(Team.Red);
        }
    }

    public void StartSinglePlayer()
    {
        // локальный режим — применяем конфигурацию сразу и загружаем сцену
        if (GameConfigurationService != null)
        {
            GameConfigurationService.SetGameMode(GameMode.SinglePlayer);
            GameConfigurationService.SetTeam(Team.Blue);
            ApplyConfigurationToService(); // локально
        }
        Loader.Load(Loader.Scene.GridFight);
    }

    private void AddPlayerToLobby()
    {
        var playerData = new LobbyPlayerData
        {
            ClientId = NetworkManager.Singleton.LocalClientId,
            PlayerName = $"Player {NetworkManager.Singleton.LocalClientId}",
            IsReady = false
        };

        AddPlayerToLobbyServerRpc(playerData);
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddPlayerToLobbyServerRpc(LobbyPlayerData playerData)
    {
        // На сервере добавляем игрока, если ещё нет
        if (!_lobbyPlayers.Contains(playerData))
        {
            _lobbyPlayers.Add(playerData);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RemovePlayerFromLobbyServerRpc(ulong clientId)
    {
        for (int i = _lobbyPlayers.Count - 1; i >= 0; i--)
        {
            if (_lobbyPlayers[i].ClientId == clientId)
            {
                _lobbyPlayers.RemoveAt(i);
                break;
            }
        }
    }

    private void OnNetworkPlayersListChanged(NetworkListEvent<LobbyPlayerData> changeEvent)
    {
        // Уведомляем UI о изменении списка игроков
        var playersArray = new LobbyPlayerData[_lobbyPlayers.Count];
        for (int i = 0; i < _lobbyPlayers.Count; i++)
        {
            playersArray[i] = _lobbyPlayers[i];
        }
        OnPlayersListChanged?.Invoke(playersArray);
    }

    // UI: переключатель конфигурации (на хосте)
    public void OnConfigToggleChanged(bool isOn)
    {
        if (IsHost)
        {
            var newIndex = isOn ? 1 : 0;
            ChangeConfigServerRpc(newIndex);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeConfigServerRpc(int configIndex)
    {
        var max = Mathf.Max((GameConfigurationService.AvailableConfigs?.Count ?? 0) - 1, 0) ;
        var clamped = Mathf.Clamp(configIndex, 0,max);
        GameConfigurationService.SetSelectedConfiguration(clamped);

        if (configIndex >= 0 && configIndex < max)
        {
            _selectedConfigIndex.Value = configIndex;
            // хост обновит локальную GameConfigurationService через OnNetworkConfigChanged, а клиенты получат OnValueChanged и применят локально
        }
    }

    private void OnNetworkConfigChanged(int previousValue, int newValue)
    {
        // Уведомляем UI о изменении конфигурации
        OnConfigChanged?.Invoke(newValue);

        // Применяем значение в локальный GameConfigurationService (хост и клиенты)
        if (GameConfigurationService != null)
        {
            // Если у нас есть список конфигов, экономим от выхода за границы
            var count = GameConfigurationService.AvailableConfigs?.Count ?? 0;
            var clamped = (count > 0) ? Mathf.Clamp(newValue, 0, count - 1) : -1;
            GameConfigurationService.SetSelectedConfiguration(clamped);
        }
    }

    public void StartGame()
    {
        if (IsHost)
        {
            StartGameServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void StartGameServerRpc()
    {
        if (!IsServer) return;
        _gameStarted.Value = true; // все клиенты получат OnValueChanged
    }

    private void OnNetworkGameStartedChanged(bool previousValue, bool newValue)
    {
        if (newValue)
        {
            OnGameStarted?.Invoke(true);
            // Клиенты и хост должны загрузить сцену. Хост использует NetworkManager.SceneManager для синхронной загрузки.
            LoadGameScene();
        }
    }

    private void LoadGameScene()
    {
        // Применяем текущую конфигурацию в локальный сервис (ещё раз)
        ApplyConfigurationToService();

        // Загружаем игровую сцену через Netcode (чтобы синхронизировать сцену между игроками)
        if (IsHost)
        {
            Loader.Load(Loader.Scene.GridFight, HostLoadRoutine);
        }
        else
        {
            // Клиенты просто дождутся команды с сервера/Netcode, но на всякий случай можно использовать обычную загрузку если не сетевой режим
            Loader.Load(Loader.Scene.GridFight, ClientWaitRoutine);
        }
    }

    private void ApplyConfigurationToService()
    {

        GameConfigurationService.SetSelectedConfiguration(_selectedConfigIndex.Value);
    }


    private void ApplyNetworkStateToLocalService()
    {
        // При старте сцены применяем существующие значения сетевых переменных к локальному SO (если есть)
        if (GameConfigurationService == null) return;

        var count = GameConfigurationService.AvailableConfigs?.Count ?? 0;
        var clamped = (count > 0) ? Mathf.Clamp(_selectedConfigIndex.Value, 0, count - 1) : -1;
        GameConfigurationService.SetSelectedConfiguration(clamped);

    }

    public override void OnNetworkDespawn()
    {
        if (IsClient || IsHost)
        {
            RemovePlayerFromLobbyServerRpc(NetworkManager.Singleton.LocalClientId);
        }
    }

    /// <summary>
    /// Получить выбранную конфигурацию юнитов (локально)
    /// </summary>
    public GridContentEntrySO GetSelectedConfig()
    {
        var configs = GameConfigurationService.AvailableConfigs;
        if (configs != null && configs.Count > 0 && _selectedConfigIndex.Value >= 0 && _selectedConfigIndex.Value < configs.Count)
        {
            return configs[_selectedConfigIndex.Value];
        }
        return null;
    }

    public (int width, int height) GetSelectedGridSize()
    {
        var selectedCOnfig = GameConfigurationService.GetSelectedConfiguration();
        return (selectedCOnfig.Width, selectedCOnfig.Height);
    }

    /// <summary>
    /// Получить конфигурацию по индексу
    /// </summary>
    public GridContentEntrySO GetConfigByIndex(int index)
    {
        if (GameConfigurationService.AvailableConfigs != null && index >= 0 && index < GameConfigurationService.AvailableConfigs.Count)
        {
            return GameConfigurationService.AvailableConfigs[index];
        }
        return null;
    }

    private IEnumerator HostLoadRoutine()
    {
        yield return null;

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(GameSceneName, LoadSceneMode.Single);
        }

        while (SceneManager.GetActiveScene().name != GameSceneName)
        {
            yield return null;
        }
    }

    private IEnumerator ClientWaitRoutine()
    {
        yield return null;

        while (SceneManager.GetActiveScene().name != GameSceneName)
        {
            yield return null;
        }
    }

    internal void SetGameModeMode(GameMode gameMode)
    {
        if (GameConfigurationService != null)
        {
            GameConfigurationService.SetGameMode(gameMode);
        }
    }
}
