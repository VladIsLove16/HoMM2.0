using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class SessionLobbyService : MonoBehaviour
{
    private const string FlowPropertyKey = "flow";
    private const string HostNamePropertyKey = "hostName";
    private const string PlayerNamePropertyKey = "displayName";
    private const string AdventureFlowValue = "adventure";
    private const string GridFightFlowValue = "gridfight";
    private const string LocalPlayerNamePrefsKey = "Lobby.LocalPlayerName";
    private const string LocalAuthProfileSeedPrefsKey = "Lobby.LocalAuthProfileSeed";
    private const string CurrentJoinCodePrefsKey = "Lobby.CurrentJoinCode";
    private const string MultiplayerStartScenePrefsKey = "Game.MultiplayerStartScene";

    [SerializeField] private NetworkManager networkManager;
    [SerializeField, Min(2)] private int maxPlayers = 2;
    [SerializeField, Min(1)] private int queryCount = 20;
    [SerializeField, Min(0f)] private float roomRefreshCooldownSeconds = 2f;
    [SerializeField] private string defaultRoomNamePrefix = "Amanita Room";
    [SerializeField] private string defaultPlayerNamePrefix = "Player";
    private readonly List<LobbyRoomInfo> _rooms = new();

    private bool _initialized;
    private bool _isBusy;
    private double _lastRoomsRefreshAt;
    private ISession _currentSession;
    private string _resolvedAuthProfile;

    public event Action<IReadOnlyList<LobbyRoomInfo>> RoomsChanged;
    public event Action<LobbySessionSnapshot> SessionChanged;
    public event Action<string> StatusChanged;
    public event Action<string> JoinCodeChanged;
    public event Action<bool> BusyStateChanged;

    public IReadOnlyList<LobbyRoomInfo> Rooms => _rooms;
    public ISession CurrentSession => _currentSession;
    public string CurrentJoinCode => !string.IsNullOrWhiteSpace(_currentSession?.Code)
        ? _currentSession.Code
        : PlayerPrefs.GetString(CurrentJoinCodePrefsKey, string.Empty);
    public string LocalPlayerName { get; private set; }
    public bool IsBusy => _isBusy;

    private async void Awake()
    {
        if (networkManager == null)
            networkManager = GetComponent<NetworkManager>() ?? NetworkManager.Singleton;

        SubscribeNetworkCallbacks();

        LocalPlayerName = PlayerPrefs.GetString(LocalPlayerNamePrefsKey, string.Empty);
        await EnsureInitializedAsync();
    }

    private void OnDestroy()
    {
        UnsubscribeNetworkCallbacks();

        if (_currentSession != null)
            UnsubscribeSession(_currentSession);
    }

    public async Task CreateRoomAsync(string playerName, string roomName)
    {
        await RunBusyAsync(async () =>
        {
            var resolvedPlayerName = ResolvePlayerName(playerName);
            await EnsureSignedInAsync(resolvedPlayerName);

            var sessionOptions = new SessionOptions
            {
                Name = ResolveRoomName(roomName),
                MaxPlayers = maxPlayers
            }.WithRelayNetwork();

            PublishStatus("Creating room...");
            var session = await MultiplayerService.Instance.CreateSessionAsync(sessionOptions);
            await BindSessionAsync(session, resolvedPlayerName, isHostSession: true);
            PublishStatus($"Room created. Join code: {session.Code}");
        });
    }

    public async Task JoinByCodeAsync(string playerName, string joinCode)
    {
        if (string.IsNullOrWhiteSpace(joinCode))
        {
            PublishStatus("Join code is empty.");
            return;
        }

        await RunBusyAsync(async () =>
        {
            var resolvedPlayerName = ResolvePlayerName(playerName);
            await EnsureSignedInAsync(resolvedPlayerName);

            PublishStatus($"Joining room by code {joinCode.Trim()}...");
            var session = await MultiplayerService.Instance.JoinSessionByCodeAsync(joinCode.Trim(), new JoinSessionOptions());
            await BindSessionAsync(session, resolvedPlayerName, isHostSession: false);
            PublishStatus($"Joined room {session.Name}.");
        });
    }

    public async Task JoinBySessionIdAsync(string playerName, string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            PublishStatus("Session id is empty.");
            return;
        }

        await RunBusyAsync(async () =>
        {
            var resolvedPlayerName = ResolvePlayerName(playerName);
            await EnsureSignedInAsync(resolvedPlayerName);

            PublishStatus("Joining selected room...");
            var session = await MultiplayerService.Instance.JoinSessionByIdAsync(sessionId.Trim(), new JoinSessionOptions());
            await BindSessionAsync(session, resolvedPlayerName, isHostSession: false);
            PublishStatus($"Joined room {session.Name}.");
        });
    }

    public async Task SetCurrentSessionFlowAsync(SceneLoader.Scene scene)
    {
        var flow = GameLaunchPreferences.ToSessionFlowValue(scene);
        PersistMultiplayerStartFlow(flow);

        if (_currentSession == null)
        {
            return;
        }

        if (!_currentSession.IsHost)
        {
            PublishStatus("Only the host can change the room mode.");
            return;
        }

        await RunBusyAsync(async () =>
        {
            var hostSession = _currentSession.AsHost();
            hostSession.SetProperty(
                FlowPropertyKey,
                new SessionProperty(flow, VisibilityPropertyOptions.Public, PropertyIndex.String1));
            await hostSession.SavePropertiesAsync();
            HandleSessionChanged();
        });
    }

    public async Task RefreshRoomsAsync()
    {
        if (!CanRefreshRoomsNow())
            return;

        await RunBusyAsync(async () =>
        {
            _lastRoomsRefreshAt = Time.realtimeSinceStartupAsDouble;
            var resolvedPlayerName = ResolvePlayerName(LocalPlayerName);
            await EnsureSignedInAsync(resolvedPlayerName);

            var queryOptions = new QuerySessionsOptions
            {
                Count = queryCount,
                FilterOptions = new List<FilterOption>
                {
                    new(FilterField.IsLocked, bool.FalseString.ToLowerInvariant(), FilterOperation.Equal)
                },
                SortOptions = new List<SortOption>
                {
                    new(SortOrder.Descending, SortField.LastUpdated)
                }
            };

            PublishStatus("Refreshing room list...");
            var results = await MultiplayerService.Instance.QuerySessionsAsync(queryOptions);

            _rooms.Clear();
            foreach (var session in results.Sessions)
            {
                var flow = GetSessionProperty(session.Properties, FlowPropertyKey);

                _rooms.Add(new LobbyRoomInfo(
                    session.Id,
                    session.Name,
                    GetSessionProperty(session.Properties, HostNamePropertyKey),
                    flow,
                    Mathf.Max(0, session.MaxPlayers - session.AvailableSlots),
                    session.MaxPlayers,
                    session.AvailableSlots,
                    session.IsLocked,
                    session.HasPassword,
                    session.LastUpdated));
            }

            RoomsChanged?.Invoke(_rooms);
            PublishStatus($"Found {_rooms.Count} room(s).");
        });
    }

    private bool CanRefreshRoomsNow()
    {
        if (roomRefreshCooldownSeconds <= 0f)
            return true;

        var elapsed = Time.realtimeSinceStartupAsDouble - _lastRoomsRefreshAt;
        return elapsed >= roomRefreshCooldownSeconds;
    }

    public async Task LeaveCurrentSessionAsync()
    {
        if (_currentSession == null)
        {
            PlayerPrefs.DeleteKey(CurrentJoinCodePrefsKey);
            return;
        }

        await RunBusyAsync(async () =>
        {
            PublishStatus("Leaving room...");
            UnsubscribeSession(_currentSession);
            await _currentSession.LeaveAsync();
            _currentSession = null;
            SessionChanged?.Invoke(null);
            JoinCodeChanged?.Invoke(null);
            PlayerPrefs.DeleteKey(CurrentJoinCodePrefsKey);
            PublishStatus("Room left.");
        });
    }

    private async Task BindSessionAsync(ISession session, string playerName, bool isHostSession)
    {
        if (_currentSession != null)
            UnsubscribeSession(_currentSession);

        _currentSession = session;
        LocalPlayerName = playerName;
        PlayerPrefs.SetString(LocalPlayerNamePrefsKey, playerName);
        SubscribeSession(_currentSession);

        await SaveLocalPlayerNameAsync(playerName);

        if (isHostSession && _currentSession.IsHost)
        {
            var startFlow = GetPersistedMultiplayerStartFlow();
            var hostSession = _currentSession.AsHost();
            hostSession.SetProperty(
                FlowPropertyKey,
                new SessionProperty(startFlow, VisibilityPropertyOptions.Public, PropertyIndex.String1));
            hostSession.SetProperty(
                HostNamePropertyKey,
                new SessionProperty(playerName, VisibilityPropertyOptions.Public, PropertyIndex.String2));
            await hostSession.SavePropertiesAsync();
        }

        SyncPreferencesFromSession(_currentSession);
        SessionChanged?.Invoke(BuildSnapshot(_currentSession));
        JoinCodeChanged?.Invoke(_currentSession.Code);
        PlayerPrefs.SetString(CurrentJoinCodePrefsKey, _currentSession.Code ?? string.Empty);
    }

    private async Task SaveLocalPlayerNameAsync(string playerName)
    {
        if (_currentSession?.CurrentPlayer == null)
            return;

        _currentSession.CurrentPlayer.SetProperty(
            PlayerNamePropertyKey,
            new PlayerProperty(playerName, VisibilityPropertyOptions.Public));

        await _currentSession.SaveCurrentPlayerDataAsync();
    }

    private async Task EnsureInitializedAsync()
    {
        if (_initialized)
            return;

        await UnityServices.InitializeAsync();
        _initialized = true;
    }

    private async Task EnsureSignedInAsync(string playerName)
    {
        await EnsureInitializedAsync();

        var authProfile = ResolveAuthProfileName();
        if (AuthenticationService.Instance.IsSignedIn)
        {
            LocalPlayerName = playerName;
            _resolvedAuthProfile = authProfile;
            return;
        }

        AuthenticationService.Instance.SwitchProfile(authProfile);
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        _resolvedAuthProfile = authProfile;
        LocalPlayerName = playerName;
        PublishStatus($"Signed in as {AuthenticationService.Instance.PlayerId} ({authProfile}).");
    }

    private void SubscribeSession(ISession session)
    {
        if (session == null)
            return;

        session.Changed += HandleSessionChanged;
        session.RemovedFromSession += HandleRemovedFromSession;
        session.Deleted += HandleDeleted;
    }

    private void UnsubscribeSession(ISession session)
    {
        if (session == null)
            return;

        session.Changed -= HandleSessionChanged;
        session.RemovedFromSession -= HandleRemovedFromSession;
        session.Deleted -= HandleDeleted;
    }

    private void HandleSessionChanged()
    {
        SyncPreferencesFromSession(_currentSession);
        PlayerPrefs.SetString(CurrentJoinCodePrefsKey, CurrentJoinCode ?? string.Empty);
        JoinCodeChanged?.Invoke(CurrentJoinCode);
        SessionChanged?.Invoke(BuildSnapshot(_currentSession));
    }

    private void HandleRemovedFromSession()
    {
        PublishStatus("Removed from session.");
        var session = _currentSession;
        if (session != null)
            UnsubscribeSession(session);
        _currentSession = null;
        SessionChanged?.Invoke(null);
        JoinCodeChanged?.Invoke(null);
        PlayerPrefs.DeleteKey(CurrentJoinCodePrefsKey);
    }

    private void HandleDeleted()
    {
        PublishStatus("Session deleted.");
        var session = _currentSession;
        if (session != null)
            UnsubscribeSession(session);
        _currentSession = null;
        SessionChanged?.Invoke(null);
        JoinCodeChanged?.Invoke(null);
        PlayerPrefs.DeleteKey(CurrentJoinCodePrefsKey);
    }

    private async Task RunBusyAsync(Func<Task> action)
    {
        if (_isBusy)
            return;

        try
        {
            _isBusy = true;
            BusyStateChanged?.Invoke(true);
            await action();
        }
        catch (Exception exception)
        {
            PublishStatus(exception.Message);
            Debug.LogException(exception, this);
        }
        finally
        {
            _isBusy = false;
            BusyStateChanged?.Invoke(false);
        }
    }

    private void PublishStatus(string message)
    {
        StatusChanged?.Invoke(message);
        Debug.Log($"[SessionLobbyService] {message}", this);
    }

    private void SubscribeNetworkCallbacks()
    {
        if (networkManager == null)
            return;

        networkManager.OnClientConnectedCallback -= HandleClientConnectedCallback;
        networkManager.OnClientDisconnectCallback -= HandleClientDisconnectCallback;
        networkManager.OnTransportFailure -= HandleTransportFailure;

        networkManager.OnClientConnectedCallback += HandleClientConnectedCallback;
        networkManager.OnClientDisconnectCallback += HandleClientDisconnectCallback;
        networkManager.OnTransportFailure += HandleTransportFailure;
    }

    private void UnsubscribeNetworkCallbacks()
    {
        if (networkManager == null)
            return;

        networkManager.OnClientConnectedCallback -= HandleClientConnectedCallback;
        networkManager.OnClientDisconnectCallback -= HandleClientDisconnectCallback;
        networkManager.OnTransportFailure -= HandleTransportFailure;
    }

    private void HandleClientConnectedCallback(ulong clientId)
    {
        PublishStatus($"NGO client connected: local={networkManager.LocalClientId}, connected={clientId}, host={networkManager.IsHost}, client={networkManager.IsClient}");
    }

    private void HandleClientDisconnectCallback(ulong clientId)
    {
        var reason = networkManager != null ? networkManager.DisconnectReason : string.Empty;
        PublishStatus($"NGO client disconnected: local={networkManager?.LocalClientId}, disconnected={clientId}, reason={reason}");
    }

    private void HandleTransportFailure()
    {
        PublishStatus($"NGO transport failure. isHost={networkManager?.IsHost}, isClient={networkManager?.IsClient}, isListening={networkManager?.IsListening}");
    }

    private string ResolvePlayerName(string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            return value.Trim();

        return string.IsNullOrWhiteSpace(LocalPlayerName)
            ? $"{defaultPlayerNamePrefix} {UnityEngine.Random.Range(1000, 9999)}"
            : LocalPlayerName;
    }

    private string ResolveRoomName(string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            return value.Trim();

        return $"{defaultRoomNamePrefix} {DateTime.UtcNow:HHmmss}";
    }

    private string ResolveAuthProfileName()
    {
        if (!string.IsNullOrWhiteSpace(_resolvedAuthProfile))
            return _resolvedAuthProfile;

        var seed = PlayerPrefs.GetString(LocalAuthProfileSeedPrefsKey, string.Empty);
        if (string.IsNullOrWhiteSpace(seed))
        {
            seed = Guid.NewGuid().ToString("N")[..12];
            PlayerPrefs.SetString(LocalAuthProfileSeedPrefsKey, seed);
        }

        var runtimeSuffix = ResolveRuntimeInstanceSuffix();
        return SanitizeProfileName(string.IsNullOrWhiteSpace(runtimeSuffix)
            ? $"player_{seed}"
            : $"player_{seed}_{runtimeSuffix}");
    }

    private static string ResolveRuntimeInstanceSuffix()
    {
        try
        {
            return System.Diagnostics.Process.GetCurrentProcess().Id.ToString();
        }
        catch
        {
            return Environment.TickCount.ToString();
        }
    }

    private static string SanitizeProfileName(string rawValue)
    {
        var trimmed = string.IsNullOrWhiteSpace(rawValue) ? "player" : rawValue.Trim();
        var chars = trimmed.Where(char.IsLetterOrDigit).ToArray();
        return chars.Length == 0 ? "player" : new string(chars);
    }

    private static string GetSessionProperty(IReadOnlyDictionary<string, SessionProperty> properties, string key)
    {
        if (properties == null || string.IsNullOrWhiteSpace(key))
            return string.Empty;

        return properties.TryGetValue(key, out var property) ? property?.Value ?? string.Empty : string.Empty;
    }

    private static LobbySessionSnapshot BuildSnapshot(ISession session)
    {
        if (session == null)
            return null;

        var players = session.Players;
        var snapshotPlayers = new LobbySessionPlayerInfo[players?.Count ?? 0];

        if (players != null)
        {
            for (int i = 0; i < players.Count; i++)
            {
                var player = players[i];
                var displayName = TryGetPlayerProperty(player, PlayerNamePropertyKey);
                if (string.IsNullOrWhiteSpace(displayName))
                    displayName = $"Player {i + 1}";

                snapshotPlayers[i] = new LobbySessionPlayerInfo(
                    player.Id,
                    displayName,
                    string.Equals(player.Id, session.Host, StringComparison.Ordinal));
            }
        }

        return new LobbySessionSnapshot(
            session.Id,
            session.Code,
            session.IsHost,
            GetSessionProperty(session.Properties, FlowPropertyKey),
            session.PlayerCount,
            session.MaxPlayers,
            snapshotPlayers);
    }

    private static void SyncPreferencesFromSession(ISession session)
    {
        if (session == null)
            return;

        var startFlow = GetSessionProperty(session.Properties, FlowPropertyKey);
        PersistMultiplayerStartFlow(startFlow);
    }

    private static string GetPersistedMultiplayerStartFlow()
    {
        var value = PlayerPrefs.GetString(MultiplayerStartScenePrefsKey, AdventureFlowValue);
        return string.IsNullOrWhiteSpace(value) ? AdventureFlowValue : value;
    }

    private static void PersistMultiplayerStartFlow(string flow)
    {
        PlayerPrefs.SetString(
            MultiplayerStartScenePrefsKey,
            string.IsNullOrWhiteSpace(flow) ? AdventureFlowValue : flow);
        PlayerPrefs.Save();
    }

    private static string TryGetPlayerProperty(IReadOnlyPlayer player, string key)
    {
        if (player?.Properties == null || string.IsNullOrWhiteSpace(key))
            return string.Empty;

        return player.Properties.TryGetValue(key, out var property)
            ? property?.Value ?? string.Empty
            : string.Empty;
    }
}

public sealed class LobbySessionPlayerInfo
{
    public LobbySessionPlayerInfo(string playerId, string displayName, bool isHost)
    {
        PlayerId = playerId ?? string.Empty;
        DisplayName = displayName ?? string.Empty;
        IsHost = isHost;
    }

    public string PlayerId { get; }
    public string DisplayName { get; }
    public bool IsHost { get; }
}

public sealed class LobbySessionSnapshot
{
    public LobbySessionSnapshot(
        string sessionId,
        string joinCode,
        bool isHost,
        string flow,
        int playerCount,
        int maxPlayers,
        IReadOnlyList<LobbySessionPlayerInfo> players)
    {
        SessionId = sessionId ?? string.Empty;
        JoinCode = joinCode ?? string.Empty;
        IsHost = isHost;
        Flow = flow ?? string.Empty;
        PlayerCount = playerCount;
        MaxPlayers = maxPlayers;
        Players = players ?? Array.Empty<LobbySessionPlayerInfo>();
    }

    public string SessionId { get; }
    public string JoinCode { get; }
    public bool IsHost { get; }
    public string Flow { get; }
    public int PlayerCount { get; }
    public int MaxPlayers { get; }
    public IReadOnlyList<LobbySessionPlayerInfo> Players { get; }
}
