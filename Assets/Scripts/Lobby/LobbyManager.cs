using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

public class LobbyManager : NetworkBehaviour
{
    private const string PlayerNamePropertyKey = "displayName";
    private static readonly Team[] SelectableTeams = { Team.Blue, Team.Red, Team.Green, Team.Yellow };

    [Header("Game Configuration")]
    [FormerlySerializedAs("GameConfigurationService")]
    [SerializeField] private GameConfigurationService gameConfigurationService;

    [Header("Team Selection")]
    [SerializeField] private Team defaultHostTeam = Team.Blue;
    [SerializeField] private Team defaultClientTeam = Team.Red;
    [SerializeField] private Team requestedTeam = Team.None;

    [Header("Session Lobby")]
    [SerializeField] private SessionLobbyService sessionLobbyService;

    [Header("Scene Loading")]
    [SerializeField] private bool useSharedSceneLoading = true;

    [Header("Direct GridFight")]
    [SerializeField] private List<ArmyLineupSO> directGridFightArmyPresets = new();
    [SerializeField, Min(1)] private int directGridFightWidth = 10;
    [SerializeField, Min(2)] private int directGridFightHeight = 10;
    [SerializeField] private int directGridFightBottomFrontlineY = 1;
    [SerializeField] private int directGridFightTopFrontlineY = 8;
    [SerializeField, Min(1)] private int directGridFightColumnSpacing = 1;

    private NetworkList<LobbyPlayerData> _lobbyPlayers;
    private readonly NetworkVariable<int> _selectedConfigIndex = new(0);
    private readonly NetworkVariable<bool> _gameStarted = new(false);
    private readonly NetworkVariable<bool> _sharedSceneLoading = new(true);
    private bool _sessionServiceSubscribed;
    private LobbySessionSnapshot _currentSessionSnapshot;
    private LobbyPlayerData? _cachedLocalPlayerData;
    private readonly Dictionary<ulong, int> _directBattleArmyPresetByClientId = new();
    private GridContentEntrySO _runtimeDirectGridFightConfig;
    private int _localDirectBattleArmyPresetIndex;

    public Action<LobbyPlayerData[]> OnPlayersListChanged;
    public Action<int> OnConfigChanged;
    public Action<bool> OnGameStarted;
    public Action<int, int> OnGridSizeChanged;
    public Action OnHostStarted;
    public Action OnClientStarted;
    public Action<LobbyPlayerData> OnLocalPlayerTeamChanged;
    public Action<IReadOnlyList<LobbyRoomInfo>> OnRoomsListChanged;
    public Action<string> OnJoinCodeChanged;
    public Action<string> OnStatusMessageChanged;
    public Action<bool> OnSharedSceneLoadingChanged;
    public Action<int, string, bool> OnDirectGridFightArmyPresetChanged;

    public bool UseSharedSceneLoading => _sharedSceneLoading.Value;

    private void Awake()
    {
        _lobbyPlayers = new NetworkList<LobbyPlayerData>();
        _localDirectBattleArmyPresetIndex = GameLaunchPreferences.GetDirectGridFightArmyPresetIndex();
        EnsureLobbyDependencies();
        SubscribeSessionService();
    }

    public override void OnDestroy()
    {
        UnsubscribeSessionService();
        base.OnDestroy();
    }

    public override void OnNetworkSpawn()
    {
        _lobbyPlayers.OnListChanged += OnNetworkPlayersListChanged;
        _selectedConfigIndex.OnValueChanged += OnNetworkConfigChanged;
        _gameStarted.OnValueChanged += OnNetworkGameStartedChanged;
        _sharedSceneLoading.OnValueChanged += OnSharedSceneLoadingValueChanged;

        if (IsServer)
            _sharedSceneLoading.Value = useSharedSceneLoading;

        if (IsClient || IsHost)
            AddPlayerToLobby();

        EnsureLocalDirectGridFightArmyPresetPublished();

        if (IsHost)
            OnHostStarted?.Invoke();
        else if (IsClient)
            OnClientStarted?.Invoke();

        ApplyNetworkStateToLocalService();
    }

    public override void OnNetworkDespawn()
    {
        if ((IsClient || IsHost) && NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
            RemovePlayerFromLobbyServerRpc(NetworkManager.Singleton.LocalClientId);

        _lobbyPlayers.OnListChanged -= OnNetworkPlayersListChanged;
        _selectedConfigIndex.OnValueChanged -= OnNetworkConfigChanged;
        _gameStarted.OnValueChanged -= OnNetworkGameStartedChanged;
        _sharedSceneLoading.OnValueChanged -= OnSharedSceneLoadingValueChanged;
    }

    public void StartHost()
    {
        CreateRoom(null, null);
    }

    public void StartClient()
    {
        JoinRoomByCode(null, null);
    }

    public void CreateRoom(string playerName, string roomName)
    {
        _ = CreateRoomAsync(playerName, roomName);
    }

    public void JoinRoomByCode(string playerName, string joinCode)
    {
        _ = JoinRoomByCodeAsync(playerName, joinCode);
    }

    public void JoinRoomById(string playerName, string sessionId)
    {
        _ = JoinRoomByIdAsync(playerName, sessionId);
    }

    public void RefreshRooms()
    {
        EnsureLobbyDependencies();
        if (sessionLobbyService == null)
            return;

        _ = sessionLobbyService.RefreshRoomsAsync();
    }

    public string GetCurrentJoinCode()
    {
        EnsureLobbyDependencies();
        return sessionLobbyService != null ? sessionLobbyService.CurrentJoinCode : string.Empty;
    }

    public void StartSinglePlayer()
    {
        EnsureGameConfigurationService();

        if (gameConfigurationService != null)
        {
            gameConfigurationService.SetGameMode(GameMode.SinglePlayer);
            gameConfigurationService.SetTeam(Team.Blue);
            ApplyConfigurationToService();
        }

        SceneLoader.Load(SceneLoader.Scene.Adventure);
    }

    public void OnConfigToggleChanged(bool isOn)
    {
        if (!IsHost)
            return;

        var newIndex = isOn ? 1 : 0;
        ChangeConfigServerRpc(newIndex);
    }

    public void StartGame()
    {
        if (!CanStartMultiplayerGame())
        {
            Debug.LogWarning("[LobbyManager] StartGame ignored because local player is not the lobby host.", this);
            return;
        }

        var networkManager = ResolveNetworkManager();
        if (networkManager != null && networkManager.IsListening && (networkManager.IsServer || networkManager.IsHost))
        {
            if (_gameStarted.Value)
                return;

            StartCoroutine(StartMultiplayerGameRoutine());
            return;
        }

        OnGameStarted?.Invoke(true);
        LoadGameScene();
    }

    public void Disconnect()
    {
        _ = DisconnectAsync();
    }

    public void RequestTeam(Team team)
    {
        requestedTeam = team;
        if (IsClient || IsHost)
            RequestTeamServerRpc(NetworkManager.Singleton.LocalClientId, team);
    }

    public void SetUseSharedSceneLoading(bool value)
    {
        useSharedSceneLoading = value;

        var networkManager = ResolveNetworkManager();
        if (networkManager != null && networkManager.IsListening)
        {
            if (networkManager.IsServer || networkManager.IsHost)
                _sharedSceneLoading.Value = value;

            return;
        }

        OnSharedSceneLoadingChanged?.Invoke(value);
    }

    public GridContentEntrySO GetSelectedConfig()
    {
        EnsureGameConfigurationService();
        var configs = gameConfigurationService != null ? gameConfigurationService.AvailableConfigs : null;
        if (configs != null && _selectedConfigIndex.Value >= 0 && _selectedConfigIndex.Value < configs.Count)
            return configs[_selectedConfigIndex.Value];

        return null;
    }

    public (int width, int height) GetSelectedGridSize()
    {
        EnsureGameConfigurationService();
        var selectedConfig = gameConfigurationService != null ? gameConfigurationService.GetSelectedConfiguration() : null;
        return selectedConfig != null ? (selectedConfig.Width, selectedConfig.Height) : (0, 0);
    }

    public GridContentEntrySO GetConfigByIndex(int index)
    {
        EnsureGameConfigurationService();
        if (gameConfigurationService?.AvailableConfigs != null &&
            index >= 0 &&
            index < gameConfigurationService.AvailableConfigs.Count)
        {
            return gameConfigurationService.AvailableConfigs[index];
        }

        return null;
    }

    public bool IsDirectGridFightFlow()
    {
        return ResolveStartScene() == SceneLoader.Scene.GridFight;
    }

    public void SetRoomFlow(SceneLoader.Scene scene)
    {
        _ = SetRoomFlowAsync(scene);
    }

    public void SelectNextDirectGridFightArmyPreset()
    {
        var count = GetDirectGridFightArmyPresetCount();
        if (count <= 0)
            return;

        var nextIndex = (_localDirectBattleArmyPresetIndex + 1) % count;
        SetLocalDirectGridFightArmyPreset(nextIndex, publishToServer: true);
    }

    public void EnsureLocalDirectGridFightArmyPresetPublished()
    {
        if (!IsDirectGridFightFlow())
        {
            PublishDirectGridFightArmyPresetState();
            return;
        }

        SetLocalDirectGridFightArmyPreset(_localDirectBattleArmyPresetIndex, publishToServer: true);
    }

    public string GetLocalDirectGridFightArmyPresetLabel()
    {
        return ResolveDirectGridFightArmyPresetLabel(_localDirectBattleArmyPresetIndex);
    }

    internal void SetGameModeMode(GameMode gameMode)
    {
        EnsureGameConfigurationService();
        if (gameConfigurationService != null)
            gameConfigurationService.SetGameMode(gameMode);
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddPlayerToLobbyServerRpc(LobbyPlayerData playerData)
    {
        for (int i = 0; i < _lobbyPlayers.Count; i++)
        {
            if (_lobbyPlayers[i].ClientId == playerData.ClientId)
                return;
        }

        playerData.Team = ResolveTeamForPlayer(playerData);
        _lobbyPlayers.Add(playerData);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RemovePlayerFromLobbyServerRpc(ulong clientId)
    {
        for (int i = _lobbyPlayers.Count - 1; i >= 0; i--)
        {
            if (_lobbyPlayers[i].ClientId != clientId)
                continue;

            _lobbyPlayers.RemoveAt(i);
            break;
        }

        _directBattleArmyPresetByClientId.Remove(clientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeConfigServerRpc(int configIndex)
    {
        var max = Mathf.Max((gameConfigurationService?.AvailableConfigs?.Count ?? 0) - 1, 0);
        var clamped = Mathf.Clamp(configIndex, 0, max);

        gameConfigurationService?.SetSelectedConfiguration(clamped);
        _selectedConfigIndex.Value = clamped;
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestTeamServerRpc(ulong clientId, Team team)
    {
        for (int i = 0; i < _lobbyPlayers.Count; i++)
        {
            if (_lobbyPlayers[i].ClientId != clientId)
                continue;

            var updated = _lobbyPlayers[i];
            updated.Team = ResolveTeamForPlayer(updated, team);
            _lobbyPlayers[i] = updated;
            return;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateDirectGridFightArmyPresetServerRpc(ulong clientId, int presetIndex)
    {
        _directBattleArmyPresetByClientId[clientId] = SanitizeDirectGridFightArmyPresetIndex(presetIndex);
    }

    [ClientRpc]
    private void PrepareDirectGridFightClientRpc(
        LobbyBattleArmyStackData[] bottomArmy,
        LobbyBattleArmyStackData[] topArmy,
        Team localTeam,
        Team battlefieldBottomTeam,
        ClientRpcParams rpcParams = default)
    {
        ApplyDirectGridFightConfiguration(
            ToUnitStackData(bottomArmy),
            ToUnitStackData(topArmy),
            localTeam,
            battlefieldBottomTeam);
    }

    private void AddPlayerToLobby()
    {
        var teamPreference = requestedTeam != Team.None
            ? requestedTeam
            : (IsHost ? defaultHostTeam : defaultClientTeam);

        var playerData = new LobbyPlayerData
        {
            ClientId = NetworkManager.Singleton.LocalClientId,
            PlayerName = ResolveLocalPlayerName(),
            IsReady = false,
            Team = teamPreference
        };

        AddPlayerToLobbyServerRpc(playerData);
    }

    private IEnumerator StartMultiplayerGameRoutine()
    {
        if (IsDirectGridFightFlow() && !PrepareDirectGridFightForParticipants())
        {
            Debug.LogWarning("[LobbyManager] Unable to start Direct GridFight because battle configuration is incomplete.", this);
            yield break;
        }

        yield return null;

        if (!_gameStarted.Value)
            _gameStarted.Value = true;
    }

    private bool PrepareDirectGridFightForParticipants()
    {
        var participants = ResolveDirectGridFightParticipants();
        if (participants.Count < 2)
        {
            Debug.LogWarning("[LobbyManager] Direct GridFight requires at least two connected lobby players.", this);
            return false;
        }

        var bottomPlayer = participants[0];
        var topPlayer = participants[1];

        var bottomTeam = bottomPlayer.Team != Team.None ? bottomPlayer.Team : defaultHostTeam;
        var topTeam = ResolveOpponentTeam(bottomTeam);
        var bottomArmy = ResolveDirectGridFightArmy(bottomPlayer.ClientId);
        var topArmy = ResolveDirectGridFightArmy(topPlayer.ClientId);
        if (bottomArmy.Count == 0 || topArmy.Count == 0)
        {
            Debug.LogWarning("[LobbyManager] Direct GridFight army preset is empty for one of the players.", this);
            return false;
        }

        ApplyDirectGridFightConfiguration(bottomArmy, topArmy, bottomTeam, bottomTeam);

        PrepareDirectGridFightClientRpc(
            ToNetworkArmy(bottomArmy),
            ToNetworkArmy(topArmy),
            topTeam,
            bottomTeam,
            BuildClientRpcParams(topPlayer.ClientId));

        return true;
    }

    private void ApplyDirectGridFightConfiguration(
        IReadOnlyList<UnitStackData> bottomArmy,
        IReadOnlyList<UnitStackData> topArmy,
        Team localTeam,
        Team battlefieldBottomTeam)
    {
        EnsureGameConfigurationService();
        if (gameConfigurationService == null)
        {
            Debug.LogError("[LobbyManager] GameConfigurationService is not assigned.", this);
            return;
        }

        if (_runtimeDirectGridFightConfig == null)
        {
            _runtimeDirectGridFightConfig = ScriptableObject.CreateInstance<GridContentEntrySO>();
            _runtimeDirectGridFightConfig.name = "RuntimeDirectGridFightConfig";
            _runtimeDirectGridFightConfig.hideFlags = HideFlags.HideAndDontSave;
        }

        var width = Mathf.Max(1, directGridFightWidth);
        var height = Mathf.Max(2, directGridFightHeight);
        var bottomY = Mathf.Clamp(directGridFightBottomFrontlineY, 0, height - 1);
        var topY = Mathf.Clamp(directGridFightTopFrontlineY, 0, height - 1);

        _runtimeDirectGridFightConfig.Width = width;
        _runtimeDirectGridFightConfig.Height = height;
        _runtimeDirectGridFightConfig.contents = BuildDirectGridFightContents(
            bottomArmy,
            topArmy,
            width,
            height,
            bottomY,
            topY,
            battlefieldBottomTeam,
            ResolveOpponentTeam(battlefieldBottomTeam));

        gameConfigurationService.SetAvailableConfigurations(new List<GridContentEntrySO> { _runtimeDirectGridFightConfig });
        gameConfigurationService.SetSelectedConfiguration(0);
        gameConfigurationService.SetGameMode(GameMode.Multiplayer);
        gameConfigurationService.SetTeam(localTeam);
        gameConfigurationService.SetBattlefieldBottomTeam(battlefieldBottomTeam);
    }

    private List<GridContentEntrySO.UnitContent> BuildDirectGridFightContents(
        IReadOnlyList<UnitStackData> bottomArmy,
        IReadOnlyList<UnitStackData> topArmy,
        int width,
        int height,
        int bottomY,
        int topY,
        Team bottomTeam,
        Team topTeam)
    {
        var contents = new List<GridContentEntrySO.UnitContent>();
        contents.AddRange(CreateDirectGridFightFormation(bottomArmy, width, height, bottomY, bottomTeam, forwardDirection: 1));
        contents.AddRange(CreateDirectGridFightFormation(topArmy, width, height, topY, topTeam, forwardDirection: -1));
        return contents;
    }

    private IEnumerable<GridContentEntrySO.UnitContent> CreateDirectGridFightFormation(
        IReadOnlyList<UnitStackData> army,
        int width,
        int height,
        int frontlineY,
        Team team,
        int forwardDirection)
    {
        if (army == null || army.Count == 0)
            yield break;

        var columnSpacing = Mathf.Max(1, directGridFightColumnSpacing);
        var x = 0;
        var y = frontlineY;

        for (int i = 0; i < army.Count; i++)
        {
            var stack = army[i];
            if (stack.Amount <= 0)
                continue;

            yield return new GridContentEntrySO.UnitContent
            {
                Team = team,
                unitType = stack.UnitType,
                Amount = Mathf.Max(1, stack.Amount),
                X = Mathf.Clamp(x, 0, width - 1),
                Y = Mathf.Clamp(y, 0, height - 1)
            };

            x += columnSpacing;
            if (x < width)
                continue;

            x %= width;
            y = Mathf.Clamp(y + forwardDirection, 0, height - 1);
        }
    }

    private List<LobbyPlayerData> ResolveDirectGridFightParticipants()
    {
        var participants = new List<LobbyPlayerData>();
        if (_lobbyPlayers == null)
            return participants;

        for (int i = 0; i < _lobbyPlayers.Count; i++)
        {
            participants.Add(_lobbyPlayers[i]);
        }

        return participants;
    }

    private IReadOnlyList<UnitStackData> ResolveDirectGridFightArmy(ulong clientId)
    {
        var presets = GetDirectGridFightArmyPresets();
        if (presets.Count == 0)
            return Array.Empty<UnitStackData>();

        if (!_directBattleArmyPresetByClientId.TryGetValue(clientId, out var presetIndex))
        {
            presetIndex = clientId == NetworkManager.ServerClientId
                ? _localDirectBattleArmyPresetIndex
                : 0;
        }

        var preset = presets[SanitizeDirectGridFightArmyPresetIndex(presetIndex)];
        return preset != null ? preset.Convert() : Array.Empty<UnitStackData>();
    }

    private void OnNetworkPlayersListChanged(NetworkListEvent<LobbyPlayerData> _)
    {
        var playersArray = new LobbyPlayerData[_lobbyPlayers.Count];
        for (int i = 0; i < _lobbyPlayers.Count; i++)
            playersArray[i] = _lobbyPlayers[i];

        CacheLocalPlayerData(playersArray);
        OnPlayersListChanged?.Invoke(playersArray);
        ApplyNetworkStateToLocalService();
        PublishDirectGridFightArmyPresetState();
    }

    private void OnNetworkConfigChanged(int _, int newValue)
    {
        OnConfigChanged?.Invoke(newValue);

        if (gameConfigurationService == null)
            return;

        var count = gameConfigurationService.AvailableConfigs?.Count ?? 0;
        var clamped = count > 0 ? Mathf.Clamp(newValue, 0, count - 1) : -1;
        gameConfigurationService.SetSelectedConfiguration(clamped);
    }

    private void OnNetworkGameStartedChanged(bool _, bool newValue)
    {
        if (!newValue)
            return;

        OnGameStarted?.Invoke(true);
        LoadGameScene();
    }

    private void OnSharedSceneLoadingValueChanged(bool _, bool newValue)
    {
        useSharedSceneLoading = newValue;
        OnSharedSceneLoadingChanged?.Invoke(newValue);
    }

    private void LoadGameScene()
    {
        ApplyConfigurationToService();
        var targetScene = ResolveStartScene();
        var forceSharedSceneLoading = targetScene == SceneLoader.Scene.GridFight;

        var networkManager = ResolveNetworkManager();
        if (networkManager != null && networkManager.IsListening)
        {
            if (_sharedSceneLoading.Value || forceSharedSceneLoading)
            {
                if (CanControlNetworkSceneLoad(networkManager))
                    networkManager.SceneManager.LoadScene(targetScene.ToString(), UnityEngine.SceneManagement.LoadSceneMode.Single);
                return;
            }

            SceneLoader.Load(targetScene);
            return;
        }

        SceneLoader.Load(targetScene);
    }

    private void ApplyConfigurationToService()
    {
        EnsureGameConfigurationService();
        gameConfigurationService?.SetSelectedConfiguration(_selectedConfigIndex.Value);

        var local = FindLocalPlayerData();
        if (local.HasValue)
            gameConfigurationService?.SetTeam(local.Value.Team);
    }

    private void ApplyNetworkStateToLocalService()
    {
        EnsureGameConfigurationService();
        if (gameConfigurationService == null)
            return;

        var count = gameConfigurationService.AvailableConfigs?.Count ?? 0;
        var clamped = count > 0 ? Mathf.Clamp(_selectedConfigIndex.Value, 0, count - 1) : -1;
        gameConfigurationService.SetSelectedConfiguration(clamped);

        var local = FindLocalPlayerData();
        if (!local.HasValue)
            return;

        gameConfigurationService.SetTeam(local.Value.Team);
        OnLocalPlayerTeamChanged?.Invoke(local.Value);
    }

    private LobbyPlayerData? FindLocalPlayerData()
    {
        if (_cachedLocalPlayerData.HasValue)
            return _cachedLocalPlayerData;

        if (_currentSessionSnapshot != null)
        {
            var snapshotPlayer = ResolveLocalPlayerDataFromSession(_currentSessionSnapshot);
            if (snapshotPlayer.HasValue)
            {
                _cachedLocalPlayerData = snapshotPlayer;
                return snapshotPlayer;
            }
        }

        if (!IsSpawned || _lobbyPlayers == null)
            return null;

        var localId = NetworkManager.Singleton != null ? NetworkManager.Singleton.LocalClientId : 0;
        try
        {
            for (int i = 0; i < _lobbyPlayers.Count; i++)
            {
                if (_lobbyPlayers[i].ClientId != localId)
                    continue;

                _cachedLocalPlayerData = _lobbyPlayers[i];
                return _cachedLocalPlayerData;
            }
        }
        catch (ObjectDisposedException)
        {
            return _cachedLocalPlayerData;
        }

        return null;
    }

    private Team ResolveTeamForPlayer(LobbyPlayerData playerData, Team? requested = null)
    {
        var candidate = requested ?? playerData.Team;
        if (candidate != Team.None && IsTeamAvailable(candidate, playerData.ClientId))
            return candidate;

        foreach (var team in SelectableTeams)
        {
            if (IsTeamAvailable(team, playerData.ClientId))
                return team;
        }

        return Team.Blue;
    }

    private bool IsTeamAvailable(Team team, ulong requestingClientId)
    {
        for (int i = 0; i < _lobbyPlayers.Count; i++)
        {
            var player = _lobbyPlayers[i];
            if (player.ClientId == requestingClientId)
                continue;
            if (player.Team == team)
                return false;
        }

        return true;
    }

    private void SubscribeSessionService()
    {
        if (sessionLobbyService == null || _sessionServiceSubscribed)
            return;

        sessionLobbyService.SessionChanged += HandleSessionChanged;
        sessionLobbyService.RoomsChanged += HandleRoomsChanged;
        sessionLobbyService.JoinCodeChanged += HandleJoinCodeChanged;
        sessionLobbyService.StatusChanged += HandleStatusChanged;
        _sessionServiceSubscribed = true;
    }

    private void UnsubscribeSessionService()
    {
        if (sessionLobbyService == null || !_sessionServiceSubscribed)
            return;

        sessionLobbyService.SessionChanged -= HandleSessionChanged;
        sessionLobbyService.RoomsChanged -= HandleRoomsChanged;
        sessionLobbyService.JoinCodeChanged -= HandleJoinCodeChanged;
        sessionLobbyService.StatusChanged -= HandleStatusChanged;
        _sessionServiceSubscribed = false;
    }

    private void HandleSessionChanged(LobbySessionSnapshot session)
    {
        _currentSessionSnapshot = session;
        _cachedLocalPlayerData = ResolveLocalPlayerDataFromSession(session);

        if (session == null)
        {
            OnPlayersListChanged?.Invoke(Array.Empty<LobbyPlayerData>());
            OnSharedSceneLoadingChanged?.Invoke(useSharedSceneLoading);
            PublishDirectGridFightArmyPresetState();
            return;
        }

        PublishSessionPlayers(session);

        if (session.IsHost)
            OnHostStarted?.Invoke();
        else
            OnClientStarted?.Invoke();

        HandleJoinCodeChanged(session.JoinCode);
        PublishDirectGridFightArmyPresetState();
        EnsureLocalDirectGridFightArmyPresetPublished();

        if (sessionLobbyService != null)
            _ = sessionLobbyService.RefreshRoomsAsync();
    }

    public void PublishCurrentLobbyState()
    {
        if (_currentSessionSnapshot != null)
        {
            PublishSessionPlayers(_currentSessionSnapshot);

            if (_currentSessionSnapshot.IsHost)
                OnHostStarted?.Invoke();
            else
                OnClientStarted?.Invoke();

            HandleJoinCodeChanged(_currentSessionSnapshot.JoinCode);
        }
        else
        {
            OnPlayersListChanged?.Invoke(Array.Empty<LobbyPlayerData>());
            HandleJoinCodeChanged(sessionLobbyService != null ? sessionLobbyService.CurrentJoinCode : string.Empty);
        }

        if (sessionLobbyService != null)
            HandleRoomsChanged(sessionLobbyService.Rooms);

        OnSharedSceneLoadingChanged?.Invoke(useSharedSceneLoading);
        PublishDirectGridFightArmyPresetState();
        EnsureLocalDirectGridFightArmyPresetPublished();
    }

    private void HandleRoomsChanged(IReadOnlyList<LobbyRoomInfo> rooms)
    {
        OnRoomsListChanged?.Invoke(rooms);
    }

    private void HandleJoinCodeChanged(string joinCode)
    {
        OnJoinCodeChanged?.Invoke(joinCode);
    }

    private void HandleStatusChanged(string message)
    {
        OnStatusMessageChanged?.Invoke(message);
    }

    private void PublishSessionPlayers(LobbySessionSnapshot session)
    {
        if (session == null)
        {
            OnPlayersListChanged?.Invoke(Array.Empty<LobbyPlayerData>());
            return;
        }

        var players = session.Players;
        if (players == null || players.Count == 0)
        {
            OnPlayersListChanged?.Invoke(Array.Empty<LobbyPlayerData>());
            return;
        }

        var playerData = new LobbyPlayerData[players.Count];
        for (int i = 0; i < players.Count; i++)
        {
            var player = players[i];
            var displayName = string.IsNullOrWhiteSpace(player.DisplayName)
                ? new FixedString128Bytes($"Player {i + 1}")
                : new FixedString128Bytes(player.DisplayName);

            playerData[i] = new LobbyPlayerData
            {
                ClientId = ComputeStableClientId(player.PlayerId, i),
                PlayerName = displayName,
                IsReady = true,
                Team = player.IsHost ? defaultHostTeam : defaultClientTeam
            };
        }

        CacheLocalPlayerData(playerData);
        OnPlayersListChanged?.Invoke(playerData);
    }

    private static ulong ComputeStableClientId(string playerId, int fallbackIndex)
    {
        if (string.IsNullOrWhiteSpace(playerId))
            return (ulong)fallbackIndex;

        unchecked
        {
            const ulong offset = 14695981039346656037UL;
            const ulong prime = 1099511628211UL;
            ulong hash = offset;

            for (int i = 0; i < playerId.Length; i++)
            {
                hash ^= playerId[i];
                hash *= prime;
            }

            return hash;
        }
    }

    private FixedString128Bytes ResolveLocalPlayerName()
    {
        if (sessionLobbyService != null && !string.IsNullOrWhiteSpace(sessionLobbyService.LocalPlayerName))
            return sessionLobbyService.LocalPlayerName;

        return $"Player {NetworkManager.Singleton.LocalClientId}";
    }

    private async Task CreateRoomAsync(string playerName, string roomName)
    {
        EnsureLobbyDependencies();
        if (sessionLobbyService == null)
        {
            Debug.LogError("[LobbyManager] SessionLobbyService is not assigned.", this);
            return;
        }

        if (gameConfigurationService != null)
        {
            gameConfigurationService.SetGameMode(GameMode.Multiplayer);
            gameConfigurationService.SetTeam(defaultHostTeam);
        }

        GameLaunchPreferences.SetMultiplayerStartScene(SceneLoader.Scene.Adventure);
        await sessionLobbyService.CreateRoomAsync(playerName, roomName);
    }

    private async Task JoinRoomByCodeAsync(string playerName, string joinCode)
    {
        EnsureLobbyDependencies();
        if (sessionLobbyService == null)
        {
            Debug.LogError("[LobbyManager] SessionLobbyService is not assigned.", this);
            return;
        }

        if (gameConfigurationService != null)
        {
            gameConfigurationService.SetGameMode(GameMode.Multiplayer);
            gameConfigurationService.SetTeam(defaultClientTeam);
        }

        await sessionLobbyService.JoinByCodeAsync(playerName, joinCode);
    }

    private async Task JoinRoomByIdAsync(string playerName, string sessionId)
    {
        EnsureLobbyDependencies();
        if (sessionLobbyService == null)
        {
            Debug.LogError("[LobbyManager] SessionLobbyService is not assigned.", this);
            return;
        }

        if (gameConfigurationService != null)
        {
            gameConfigurationService.SetGameMode(GameMode.Multiplayer);
            gameConfigurationService.SetTeam(defaultClientTeam);
        }

        await sessionLobbyService.JoinBySessionIdAsync(playerName, sessionId);
    }

    private async Task SetRoomFlowAsync(SceneLoader.Scene scene)
    {
        EnsureLobbyDependencies();
        GameLaunchPreferences.SetMultiplayerStartScene(scene);

        if (sessionLobbyService == null)
        {
            Debug.LogError("[LobbyManager] SessionLobbyService is not assigned.", this);
            return;
        }

        if (!CanStartMultiplayerGame())
        {
            HandleStatusChanged("Only the host can change the room mode.");
            return;
        }

        await sessionLobbyService.SetCurrentSessionFlowAsync(scene);
    }

    private async Task DisconnectAsync()
    {
        EnsureLobbyDependencies();

        if (sessionLobbyService != null)
            await sessionLobbyService.LeaveCurrentSessionAsync();

        var networkManager = ResolveNetworkManager();
        if (networkManager != null && networkManager.IsListening)
            networkManager.Shutdown();

        _directBattleArmyPresetByClientId.Clear();
        HandleJoinCodeChanged(string.Empty);
        HandleStatusChanged("Disconnected.");
        PublishDirectGridFightArmyPresetState();
    }

    private bool CanStartMultiplayerGame()
    {
        if (_currentSessionSnapshot?.IsHost == true)
            return true;

        var networkManager = ResolveNetworkManager();
        if (networkManager?.LocalClient != null && networkManager.LocalClient.IsSessionOwner)
            return true;

        return IsHost;
    }

    private SceneLoader.Scene ResolveStartScene()
    {
        if (_currentSessionSnapshot != null && !string.IsNullOrWhiteSpace(_currentSessionSnapshot.Flow))
            return GameLaunchPreferences.ParseSessionFlowValue(_currentSessionSnapshot.Flow);

        return GameLaunchPreferences.GetMultiplayerStartScene();
    }

    private IReadOnlyList<ArmyLineupSO> GetDirectGridFightArmyPresets()
    {
        if (directGridFightArmyPresets != null)
        {
            directGridFightArmyPresets.RemoveAll(preset => preset == null);
            if (directGridFightArmyPresets.Count > 0)
                return directGridFightArmyPresets;
        }

        if (gameConfigurationService != null && gameConfigurationService.GetPlayerArmy() != null)
        {
            directGridFightArmyPresets = new List<ArmyLineupSO> { gameConfigurationService.GetPlayerArmy() };
            return directGridFightArmyPresets;
        }

        return Array.Empty<ArmyLineupSO>();
    }

    private int GetDirectGridFightArmyPresetCount()
    {
        return GetDirectGridFightArmyPresets().Count;
    }

    private int SanitizeDirectGridFightArmyPresetIndex(int index)
    {
        var count = GetDirectGridFightArmyPresetCount();
        if (count <= 0)
            return 0;

        return Mathf.Clamp(index, 0, count - 1);
    }

    private void SetLocalDirectGridFightArmyPreset(int presetIndex, bool publishToServer)
    {
        _localDirectBattleArmyPresetIndex = SanitizeDirectGridFightArmyPresetIndex(presetIndex);
        GameLaunchPreferences.SetDirectGridFightArmyPresetIndex(_localDirectBattleArmyPresetIndex);

        if (publishToServer)
        {
            var networkManager = ResolveNetworkManager();
            if (networkManager != null && networkManager.IsListening)
            {
                var localClientId = networkManager.LocalClientId;
                _directBattleArmyPresetByClientId[localClientId] = _localDirectBattleArmyPresetIndex;
                UpdateDirectGridFightArmyPresetServerRpc(localClientId, _localDirectBattleArmyPresetIndex);
            }
        }

        PublishDirectGridFightArmyPresetState();
    }

    private void PublishDirectGridFightArmyPresetState()
    {
        var showSelector = IsDirectGridFightFlow() && GetDirectGridFightArmyPresetCount() > 0;
        OnDirectGridFightArmyPresetChanged?.Invoke(
            _localDirectBattleArmyPresetIndex,
            ResolveDirectGridFightArmyPresetLabel(_localDirectBattleArmyPresetIndex),
            showSelector);
    }

    private string ResolveDirectGridFightArmyPresetLabel(int presetIndex)
    {
        var presets = GetDirectGridFightArmyPresets();
        if (presets.Count == 0)
            return "No army presets";

        var preset = presets[SanitizeDirectGridFightArmyPresetIndex(presetIndex)];
        return preset != null ? preset.name : "Unnamed army preset";
    }

    private bool CanControlNetworkSceneLoad(NetworkManager networkManager)
    {
        if (networkManager == null)
            return false;

        if (_currentSessionSnapshot?.IsHost == true)
            return true;

        return networkManager.IsHost ||
               networkManager.IsServer ||
               (networkManager.LocalClient != null && networkManager.LocalClient.IsSessionOwner);
    }

    private void CacheLocalPlayerData(IReadOnlyList<LobbyPlayerData> players)
    {
        if (players == null)
        {
            _cachedLocalPlayerData = null;
            return;
        }

        var local = ResolveLocalPlayerDataFromPublishedPlayers(players);
        if (local.HasValue)
            _cachedLocalPlayerData = local;
    }

    private LobbyPlayerData? ResolveLocalPlayerDataFromPublishedPlayers(IReadOnlyList<LobbyPlayerData> players)
    {
        if (players == null || players.Count == 0)
            return null;

        if (_currentSessionSnapshot != null)
        {
            for (int i = 0; i < players.Count; i++)
            {
                var expectedTeam = _currentSessionSnapshot.IsHost ? defaultHostTeam : defaultClientTeam;
                if (players[i].Team == expectedTeam)
                    return players[i];
            }
        }

        var localId = NetworkManager.Singleton != null ? NetworkManager.Singleton.LocalClientId : 0;
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientId == localId)
                return players[i];
        }

        return players.Count > 0 ? players[0] : null;
    }

    private LobbyPlayerData? ResolveLocalPlayerDataFromSession(LobbySessionSnapshot session)
    {
        if (session?.Players == null || session.Players.Count == 0)
            return null;

        for (int i = 0; i < session.Players.Count; i++)
        {
            var player = session.Players[i];
            if (player == null || player.IsHost != session.IsHost)
                continue;

            var displayName = string.IsNullOrWhiteSpace(player.DisplayName)
                ? new FixedString128Bytes($"Player {i + 1}")
                : new FixedString128Bytes(player.DisplayName);

            return new LobbyPlayerData
            {
                ClientId = ComputeStableClientId(player.PlayerId, i),
                PlayerName = displayName,
                IsReady = true,
                Team = player.IsHost ? defaultHostTeam : defaultClientTeam
            };
        }

        return null;
    }

    private void EnsureLobbyDependencies()
    {
        EnsureGameConfigurationService();

        if (sessionLobbyService != null)
            return;

        var networkManager = ResolveNetworkManager();
        if (networkManager == null)
        {
            Debug.LogWarning("[LobbyManager] NetworkManager not found. Session lobby features are unavailable.", this);
            return;
        }

        sessionLobbyService = networkManager.GetComponent<SessionLobbyService>();
        if (sessionLobbyService == null)
            sessionLobbyService = networkManager.gameObject.AddComponent<SessionLobbyService>();

        SubscribeSessionService();
        HandleJoinCodeChanged(sessionLobbyService.CurrentJoinCode);
    }

    private static Team ResolveOpponentTeam(Team team)
    {
        return team switch
        {
            Team.Red => Team.Blue,
            Team.Green => Team.Yellow,
            Team.Yellow => Team.Green,
            _ => Team.Red
        };
    }

    private static LobbyBattleArmyStackData[] ToNetworkArmy(IReadOnlyList<UnitStackData> stacks)
    {
        if (stacks == null || stacks.Count == 0)
            return Array.Empty<LobbyBattleArmyStackData>();

        var result = new LobbyBattleArmyStackData[stacks.Count];
        for (int i = 0; i < stacks.Count; i++)
        {
            result[i] = new LobbyBattleArmyStackData((int)stacks[i].UnitType, stacks[i].Amount);
        }

        return result;
    }

    private static List<UnitStackData> ToUnitStackData(IReadOnlyList<LobbyBattleArmyStackData> stacks)
    {
        if (stacks == null || stacks.Count == 0)
            return new List<UnitStackData>();

        var result = new List<UnitStackData>(stacks.Count);
        for (int i = 0; i < stacks.Count; i++)
        {
            if (stacks[i].Amount <= 0)
                continue;

            result.Add(new UnitStackData((UnitType)stacks[i].UnitType, stacks[i].Amount));
        }

        return result;
    }

    private static ClientRpcParams BuildClientRpcParams(ulong clientId)
    {
        return new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new[] { clientId }
            }
        };
    }

    private void EnsureGameConfigurationService()
    {
        if (gameConfigurationService != null)
            return;

        gameConfigurationService = ScriptableObject.CreateInstance<GameConfigurationService>();
        gameConfigurationService.name = "RuntimeGameConfigurationService";
        Debug.LogWarning("[LobbyManager] GameConfigurationService is not assigned. Using runtime instance.", this);
    }

    private NetworkManager ResolveNetworkManager()
    {
        if (NetworkManager.Singleton != null)
            return NetworkManager.Singleton;

#if UNITY_2023_1_OR_NEWER
        return UnityEngine.Object.FindFirstObjectByType<NetworkManager>();
#else
        return UnityEngine.Object.FindObjectOfType<NetworkManager>();
#endif
    }
}

