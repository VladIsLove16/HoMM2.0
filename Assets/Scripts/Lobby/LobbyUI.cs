using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Zenject;

public class LobbyUI : MonoBehaviour
{
    [Header("Panels")]
    [FormerlySerializedAs("_connectionPanel")]
    [SerializeField] private GameObject connectionPanel;
    [FormerlySerializedAs("_lobbyPanel")]
    [SerializeField] private GameObject lobbyPanel;

    [Header("Connection Panel")]
    [FormerlySerializedAs("_hostButton")]
    [SerializeField] private Button hostButton;
    [FormerlySerializedAs("_clientButton")]
    [SerializeField] private Button clientButton;
    [FormerlySerializedAs("_ipInputField")]
    [SerializeField] private InputField legacyJoinCodeInput;
    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private TMP_InputField roomNameInput;
    [SerializeField] private TMP_InputField joinCodeInput;
    [SerializeField] private Button refreshRoomsButton;
    [SerializeField] private RectTransform roomsListParent;
    [SerializeField] private GameObject roomListItemPrefab;
    [SerializeField] private TMP_Text joinCodeLabel;
    [SerializeField] private TMP_Text statusMessageLabel;

    [Header("Lobby Panel")]
    [FormerlySerializedAs("_playersListParent")]
    [SerializeField] private RectTransform playersListParent;
    [FormerlySerializedAs("_playerListItemPrefab")]
    [SerializeField] private GameObject playerListItemPrefab;
    [FormerlySerializedAs("_startGameButton")]
    [SerializeField] private Button startGameButton;
    [FormerlySerializedAs("_singlePlayerButton")]
    [SerializeField] private Button singlePlayerButton;
    [FormerlySerializedAs("_configToggle")]
    [SerializeField] private Toggle configToggle;
    [FormerlySerializedAs("_configLabel")]
    [SerializeField] private Text configLabel;
    [FormerlySerializedAs("_disconnectButton")]
    [SerializeField] private Button disconnectButton;
    [SerializeField] private Toggle sharedSceneLoadingToggle;

    [Header("External")]
    [Inject] private LobbyManager lobbyManager;

    [Header("Pooling")]
    [FormerlySerializedAs("_initialPoolSize")]
    [SerializeField, Min(0)] private int initialPlayerPoolSize = 8;
    [SerializeField, Min(0)] private int initialRoomPoolSize = 8;

    [Header("Localization")]
    [SerializeField] private LocalizedString gameStartedMessage;
    [SerializeField] private LocalizedString sharedAdventureModeLabelText;
    [SerializeField] private LocalizedString armyPresetLabelFormat;
    [SerializeField] private LocalizedString noArmyPresetLabelText;

    private readonly List<LobbyPlayerListItem> _activePlayerItems = new(16);
    private readonly Queue<LobbyPlayerListItem> _playerItemPool = new(32);
    private readonly List<LobbyRoomListItem> _activeRoomItems = new(16);
    private readonly Queue<LobbyRoomListItem> _roomItemPool = new(32);

    private bool _subscribed;
    private bool _localizationSubscribed;
    private bool _showingDirectGridFightArmySelector;
    private string _currentArmyPresetLabel = string.Empty;

    private TMP_Text _configLabelTMP;

    private void Awake()
    {
        LocalizationSettings.InitializationOperation.WaitForCompletion();
        EnsureLocalizationDefaults();
        _configLabelTMP = configLabel != null ? configLabel.GetComponent<TMP_Text>() : null;
    }

    private void Start()
    {
        if (!ValidateReferences())
        {
            Debug.LogError("[LobbyUI] Missing references. UI disabled.");
            enabled = false;
            return;
        }

        SetupPool();
        SetupUiListeners();

        if (lobbyManager == null)
        {
            Debug.LogError("[LobbyUI] LobbyManager was not injected. Add SceneContext to Lobby scene and register LobbyRoot installer.", this);
            enabled = false;
            return;
        }

        TrySubscribeToLobbyManager();
        ShowConnectionPanel();
        ApplyLocalization();
        lobbyManager.PublishCurrentLobbyState();
        lobbyManager.RefreshRooms();
    }

    private void OnEnable()
    {
        TrySubscribeToLobbyManager();
        SubscribeLocalization();
    }

    private void OnDisable()
    {
        UnsubscribeLocalization();
        UnsubscribeFromLobbyManager();
    }

    private void OnDestroy()
    {
        UnsubscribeLocalization();
        UnsubscribeFromLobbyManager();
    }

    private bool ValidateReferences()
    {
        return connectionPanel != null &&
               lobbyPanel != null &&
               hostButton != null &&
               clientButton != null &&
               playersListParent != null &&
               playerListItemPrefab != null &&
               startGameButton != null &&
               configToggle != null &&
               configLabel != null &&
               disconnectButton != null;
    }

    private void SetupUiListeners()
    {
        hostButton.onClick.RemoveAllListeners();
        hostButton.onClick.AddListener(OnHostButtonClicked);

        clientButton.onClick.RemoveAllListeners();
        clientButton.onClick.AddListener(OnClientButtonClicked);

        startGameButton.onClick.RemoveAllListeners();
        startGameButton.onClick.AddListener(OnStartGameClicked);

        if (singlePlayerButton != null)
        {
            singlePlayerButton.onClick.RemoveAllListeners();
            singlePlayerButton.onClick.AddListener(OnAuxiliaryButtonClicked);
        }

        if (refreshRoomsButton != null)
        {
            refreshRoomsButton.onClick.RemoveAllListeners();
            refreshRoomsButton.onClick.AddListener(OnRefreshRoomsClicked);
        }

        configToggle.onValueChanged.RemoveAllListeners();
        configToggle.onValueChanged.AddListener(OnFlowToggleChanged);

        disconnectButton.onClick.RemoveAllListeners();
        disconnectButton.onClick.AddListener(OnDisconnectClicked);

        if (sharedSceneLoadingToggle != null)
        {
            sharedSceneLoadingToggle.onValueChanged.RemoveAllListeners();
            sharedSceneLoadingToggle.onValueChanged.AddListener(OnSharedSceneLoadingToggleChanged);
        }
    }

    private void SetupPool()
    {
        for (int i = 0; i < initialPlayerPoolSize; i++)
        {
            var go = Instantiate(playerListItemPrefab, playersListParent);
            go.SetActive(false);
            var item = go.GetComponent<LobbyPlayerListItem>();
            if (item != null)
                _playerItemPool.Enqueue(item);
        }

        if (roomsListParent == null || roomListItemPrefab == null)
            return;

        for (int i = 0; i < initialRoomPoolSize; i++)
        {
            var go = Instantiate(roomListItemPrefab, roomsListParent);
            go.SetActive(false);
            var item = go.GetComponent<LobbyRoomListItem>();
            if (item != null)
                _roomItemPool.Enqueue(item);
        }
    }

    private LobbyPlayerListItem GetPooledPlayerItem()
    {
        while (_playerItemPool.Count > 0)
        {
            var item = _playerItemPool.Dequeue();
            if (item != null)
                return item;
        }

        var newGo = Instantiate(playerListItemPrefab, playersListParent);
        var newItem = newGo.GetComponent<LobbyPlayerListItem>();
        newGo.SetActive(false);
        return newItem;
    }

    private LobbyRoomListItem GetPooledRoomItem()
    {
        while (_roomItemPool.Count > 0)
        {
            var item = _roomItemPool.Dequeue();
            if (item != null)
                return item;
        }

        if (roomListItemPrefab == null || roomsListParent == null)
            return null;

        var newGo = Instantiate(roomListItemPrefab, roomsListParent);
        var newItem = newGo.GetComponent<LobbyRoomListItem>();
        newGo.SetActive(false);
        return newItem;
    }

    private void RecycleAllPlayerItems()
    {
        foreach (var item in _activePlayerItems)
        {
            if (item == null)
                continue;

            item.gameObject.SetActive(false);
            _playerItemPool.Enqueue(item);
        }

        _activePlayerItems.Clear();
    }

    private void RecycleAllRoomItems()
    {
        foreach (var item in _activeRoomItems)
        {
            if (item == null)
                continue;

            item.gameObject.SetActive(false);
            _roomItemPool.Enqueue(item);
        }

        _activeRoomItems.Clear();
    }

    private void TrySubscribeToLobbyManager()
    {
        if (lobbyManager == null || _subscribed)
            return;

        lobbyManager.OnPlayersListChanged += UpdatePlayersList;
        lobbyManager.OnGameStarted += OnGameStarted;
        lobbyManager.OnHostStarted += OnHostStarted;
        lobbyManager.OnClientStarted += OnClientStarted;
        lobbyManager.OnRoomsListChanged += UpdateRoomsList;
        lobbyManager.OnJoinCodeChanged += UpdateJoinCodeDisplay;
        lobbyManager.OnStatusMessageChanged += UpdateStatusMessage;
        lobbyManager.OnSharedSceneLoadingChanged += UpdateSharedSceneLoadingToggle;
        lobbyManager.OnDirectGridFightArmyPresetChanged += UpdateDirectGridFightArmyPreset;
        _subscribed = true;
    }

    private void UnsubscribeFromLobbyManager()
    {
        if (lobbyManager == null || !_subscribed)
            return;

        lobbyManager.OnPlayersListChanged -= UpdatePlayersList;
        lobbyManager.OnGameStarted -= OnGameStarted;
        lobbyManager.OnHostStarted -= OnHostStarted;
        lobbyManager.OnClientStarted -= OnClientStarted;
        lobbyManager.OnRoomsListChanged -= UpdateRoomsList;
        lobbyManager.OnJoinCodeChanged -= UpdateJoinCodeDisplay;
        lobbyManager.OnStatusMessageChanged -= UpdateStatusMessage;
        lobbyManager.OnSharedSceneLoadingChanged -= UpdateSharedSceneLoadingToggle;
        lobbyManager.OnDirectGridFightArmyPresetChanged -= UpdateDirectGridFightArmyPreset;
        _subscribed = false;
    }

    private void SubscribeLocalization()
    {
        if (_localizationSubscribed)
            return;

        LocalizationSettings.SelectedLocaleChanged += OnLocalizationLanguageChanged;
        _localizationSubscribed = true;
        ApplyLocalization();
    }

    private void UnsubscribeLocalization()
    {
        if (!_localizationSubscribed)
            return;

        LocalizationSettings.SelectedLocaleChanged -= OnLocalizationLanguageChanged;
        _localizationSubscribed = false;
    }

    private void OnHostButtonClicked()
    {
        lobbyManager?.CreateRoom(GetPlayerNameInput(), GetRoomNameInput());
    }

    private void OnClientButtonClicked()
    {
        lobbyManager?.JoinRoomByCode(GetPlayerNameInput(), GetJoinCodeInput());
    }

    private void OnRefreshRoomsClicked()
    {
        lobbyManager?.RefreshRooms();
    }

    private void OnStartGameClicked()
    {
        lobbyManager?.StartGame();
    }

    private void OnAuxiliaryButtonClicked()
    {
        if (lobbyManager != null && lobbyManager.IsDirectGridFightFlow())
            lobbyManager.SelectNextDirectGridFightArmyPreset();
    }

    private void OnFlowToggleChanged(bool isOn)
    {
        lobbyManager?.SetRoomFlow(isOn ? SceneLoader.Scene.GridFight : SceneLoader.Scene.Adventure);
    }

    private void OnDisconnectClicked()
    {
        lobbyManager?.Disconnect();
        ShowConnectionPanel();
    }

    private void OnSharedSceneLoadingToggleChanged(bool isOn)
    {
        lobbyManager?.SetUseSharedSceneLoading(isOn);
    }

    private void UpdatePlayersList(LobbyPlayerData[] players)
    {
        RecycleAllPlayerItems();

        if (players == null)
            players = Array.Empty<LobbyPlayerData>();

        foreach (var player in players)
        {
            var item = GetPooledPlayerItem();
            if (item == null)
                break;

            item.transform.SetParent(playersListParent, false);
            item.Setup(player);
            item.gameObject.SetActive(true);
            _activePlayerItems.Add(item);
        }
    }

    private void UpdateRoomsList(IReadOnlyList<LobbyRoomInfo> rooms)
    {
        RecycleAllRoomItems();

        if (roomsListParent == null || roomListItemPrefab == null || rooms == null)
            return;

        foreach (var room in rooms)
        {
            var item = GetPooledRoomItem();
            if (item == null)
                break;

            item.transform.SetParent(roomsListParent, false);
            item.Setup(room, HandleJoinRoomClicked);
            item.gameObject.SetActive(true);
            _activeRoomItems.Add(item);
        }
    }

    private void OnGameStarted(bool started)
    {
        if (!started)
            return;

        var message = ResolveLocalized(gameStartedMessage, "Game started");
        Debug.Log($"[LobbyUI] {message}");
    }

    private void OnHostStarted()
    {
        ShowLobbyPanel();
        SetHostUI();
        UpdateJoinCodeDisplay(lobbyManager?.GetCurrentJoinCode());
        UpdateSharedSceneLoadingToggle(lobbyManager != null && lobbyManager.UseSharedSceneLoading);
        RefreshFlowSpecificControls();
        lobbyManager?.EnsureLocalDirectGridFightArmyPresetPublished();
    }

    private void OnClientStarted()
    {
        ShowLobbyPanel();
        SetClientUI();
        UpdateJoinCodeDisplay(lobbyManager?.GetCurrentJoinCode());
        UpdateSharedSceneLoadingToggle(lobbyManager != null && lobbyManager.UseSharedSceneLoading);
        RefreshFlowSpecificControls();
        lobbyManager?.EnsureLocalDirectGridFightArmyPresetPublished();
    }

    private void SetHostUI()
    {
        if (startGameButton != null)
            startGameButton.gameObject.SetActive(true);

        if (configToggle != null)
            configToggle.interactable = true;

        if (sharedSceneLoadingToggle != null)
            sharedSceneLoadingToggle.interactable = true;
    }

    private void SetClientUI()
    {
        if (startGameButton != null)
            startGameButton.gameObject.SetActive(false);

        if (configToggle != null)
            configToggle.interactable = false;

        if (sharedSceneLoadingToggle != null)
            sharedSceneLoadingToggle.interactable = false;
    }

    private void ShowConnectionPanel()
    {
        connectionPanel?.SetActive(true);
        lobbyPanel?.SetActive(false);
        UpdateJoinCodeDisplay(null);
        UpdateDirectGridFightArmyPreset(0, string.Empty, false);
        lobbyManager?.RefreshRooms();
    }

    public void OpenConnectionFlow()
    {
        ShowConnectionPanel();
        UpdateSharedSceneLoadingToggle(lobbyManager != null && lobbyManager.UseSharedSceneLoading);
        lobbyManager?.PublishCurrentLobbyState();
    }

    private void ShowLobbyPanel()
    {
        connectionPanel?.SetActive(false);
        lobbyPanel?.SetActive(true);
    }

    private void RefreshFlowSpecificControls()
    {
        if (lobbyManager == null)
        {
            UpdateDirectGridFightArmyPreset(0, string.Empty, false);
            return;
        }

        UpdateDirectGridFightArmyPreset(
            0,
            lobbyManager.GetLocalDirectGridFightArmyPresetLabel(),
            lobbyManager.IsDirectGridFightFlow());
    }

    private void UpdateDirectGridFightArmyPreset(int _, string label, bool showSelector)
    {
        _showingDirectGridFightArmySelector = showSelector;
        _currentArmyPresetLabel = label ?? string.Empty;

        if (configToggle != null)
            configToggle.SetIsOnWithoutNotify(showSelector);

        if (singlePlayerButton != null)
        {
            singlePlayerButton.gameObject.SetActive(showSelector);
        }

        if (showSelector)
        {
            var presetFormat = ResolveLocalized(armyPresetLabelFormat, "Army preset: {0}");
            var displayLabel = string.IsNullOrWhiteSpace(_currentArmyPresetLabel)
                ? ResolveLocalized(noArmyPresetLabelText, "Army preset is not selected")
                : string.Format(presetFormat, _currentArmyPresetLabel);
            ApplyConfigLabel(displayLabel);
        }
        else
        {
            ApplyConfigLabel(ResolveLocalized(sharedAdventureModeLabelText, "Shared Adventure"));
        }
    }

    private void ApplyConfigLabel(string value)
    {
        if (configLabel != null)
            configLabel.text = value;

        if (_configLabelTMP != null)
            _configLabelTMP.text = value;
    }

    private void UpdateJoinCodeDisplay(string joinCode)
    {
        if (joinCodeLabel == null)
            return;

        joinCodeLabel.text = string.IsNullOrWhiteSpace(joinCode) ? string.Empty : $"Code: {joinCode}";
    }

    private void UpdateStatusMessage(string message)
    {
        if (statusMessageLabel != null)
            statusMessageLabel.text = message ?? string.Empty;
    }

    private void UpdateSharedSceneLoadingToggle(bool value)
    {
        if (sharedSceneLoadingToggle == null)
            return;

        sharedSceneLoadingToggle.SetIsOnWithoutNotify(value);
    }

    private void ApplyLocalization()
    {
        UpdateDirectGridFightArmyPreset(0, _currentArmyPresetLabel, _showingDirectGridFightArmySelector);
    }

    private void OnLocalizationLanguageChanged(Locale _)
    {
        ApplyLocalization();
    }

    private void EnsureLocalizationDefaults()
    {
        EnsureEntry(ref gameStartedMessage, "System.GameStarted");
        EnsureEntry(ref sharedAdventureModeLabelText, "Lobby.SharedAdventureMode");
        EnsureEntry(ref armyPresetLabelFormat, "Lobby.ArmyPresetFormat");
        EnsureEntry(ref noArmyPresetLabelText, "Lobby.NoArmyPreset");
    }

    private static void EnsureEntry(ref LocalizedString entry, string key)
    {
        if (!entry.IsEmpty)
            return;

        entry.TableReference = "MainLocalization";
        entry.TableEntryReference = key;
    }

    private static string ResolveLocalized(LocalizedString entry, string fallback)
    {
        if (entry.IsEmpty)
            return fallback;

        var value = entry.GetLocalizedString();
        return string.IsNullOrEmpty(value) ? fallback : value;
    }

    private string GetPlayerNameInput()
    {
        return playerNameInput != null ? playerNameInput.text : string.Empty;
    }

    private string GetRoomNameInput()
    {
        return roomNameInput != null ? roomNameInput.text : string.Empty;
    }

    private string GetJoinCodeInput()
    {
        if (joinCodeInput != null && !string.IsNullOrWhiteSpace(joinCodeInput.text))
            return joinCodeInput.text;

        return legacyJoinCodeInput != null ? legacyJoinCodeInput.text : string.Empty;
    }

    private void HandleJoinRoomClicked(LobbyRoomInfo room)
    {
        lobbyManager?.JoinRoomById(GetPlayerNameInput(), room.SessionId);
    }
}
