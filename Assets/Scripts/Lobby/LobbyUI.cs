using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class LobbyUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject _connectionPanel;
    [SerializeField] private GameObject _lobbyPanel;

    [Header("Connection Panel")]
    [SerializeField] private Button _hostButton;
    [SerializeField] private Button _clientButton;
    [SerializeField] private InputField _ipInputField;

    [Header("Lobby Panel")]
    [SerializeField] private RectTransform _playersListParent;
    [SerializeField] private GameObject _playerListItemPrefab;
    [SerializeField] private Button _startGameButton;
    [SerializeField] private Button _singlePlayerButton;
    [SerializeField] private Toggle _configToggle;
    [SerializeField] private Text _configLabel;
    [SerializeField] private Button _disconnectButton;

    [Header("External")]
    [SerializeField] private LobbyManager _lobbyManager;

    [Header("Pooling")]
    [SerializeField, Min(0)] private int _initialPoolSize = 8;
    [Header("Localization")]
    [SerializeField] private LocalizedString _hostLabelText;
    [SerializeField] private LocalizedString _clientLabelText;
    [SerializeField] private LocalizedString _startGameLabelText;
    [SerializeField] private LocalizedString _singlePlayerLabelText;
    [SerializeField] private LocalizedString _disconnectLabelText;
    [SerializeField] private LocalizedString _configToggleLabelText;
    [SerializeField] private LocalizedString _ipPlaceholderText;
    [SerializeField] private LocalizedString _configLabelFormat;
    [SerializeField] private LocalizedString _noConfigLabelText;
    [SerializeField] private LocalizedString _gameStartedMessage;

    private readonly List<LobbyPlayerListItem> _activeItems = new List<LobbyPlayerListItem>(16);
    private readonly Queue<LobbyPlayerListItem> _itemPool = new Queue<LobbyPlayerListItem>(32);
    private bool _subscribed;

    private Component _hostButtonLabel;
    private Component _clientButtonLabel;
    private Component _startGameButtonLabel;
    private Component _singlePlayerButtonLabel;
    private Component _disconnectButtonLabel;
    private Component _configToggleLabelComponent;
    private Component _ipPlaceholderComponent;
    private TMP_Text _configLabelTMP;
    private bool _localizationSubscribed;
    private int _lastConfigIndex;

    #region Unity lifecycle
    private void Awake()
    {
        LocalizationSettings.InitializationOperation.WaitForCompletion();

        EnsureLocalizationDefaults();
        if (_lobbyManager == null)
        {
#if UNITY_2023_1_OR_NEWER
            _lobbyManager = UnityEngine.Object.FindFirstObjectByType<LobbyManager>();
#else
            _lobbyManager = FindObjectOfType<LobbyManager>();
#endif
        }

        CacheLocalizationTargets();
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
        TrySubscribeToLobbyManager();
        ShowConnectionPanel();
        ApplyLocalization();
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
    #endregion

    #region Setup & validation
    private bool ValidateReferences()
    {
        return _connectionPanel != null && _lobbyPanel != null &&
               _hostButton != null && _clientButton != null && _ipInputField != null &&
               _playersListParent != null && _playerListItemPrefab != null &&
               _startGameButton != null && _configToggle != null && _configLabel != null &&
               _disconnectButton != null;
    }

    private void SetupUiListeners()
    {
        _hostButton.onClick.RemoveAllListeners();
        _hostButton.onClick.AddListener(OnHostButtonClicked);

        _clientButton.onClick.RemoveAllListeners();
        _clientButton.onClick.AddListener(OnClientButtonClicked);

        _startGameButton.onClick.RemoveAllListeners();
        _startGameButton.onClick.AddListener(OnStartGameClicked);

        if (_singlePlayerButton != null)
        {
            _singlePlayerButton.onClick.RemoveAllListeners();
            _singlePlayerButton.onClick.AddListener(OnStartSinglePlayerClicked);
        }

        _configToggle.onValueChanged.RemoveAllListeners();
        _configToggle.onValueChanged.AddListener(OnConfigToggleChanged);

        _disconnectButton.onClick.RemoveAllListeners();
        _disconnectButton.onClick.AddListener(OnDisconnectClicked);
    }

    private void CacheLocalizationTargets()
    {
        _hostButtonLabel = ResolveLabel(_hostButton);
        _clientButtonLabel = ResolveLabel(_clientButton);
        _startGameButtonLabel = ResolveLabel(_startGameButton);
        _singlePlayerButtonLabel = ResolveLabel(_singlePlayerButton);
        _disconnectButtonLabel = ResolveLabel(_disconnectButton);
        _configToggleLabelComponent = ResolveLabel(_configToggle);
        _configLabelTMP = _configLabel != null ? _configLabel.GetComponent<TMP_Text>() : null;

        if (_ipInputField != null && _ipInputField.placeholder != null)
        {
            if (_ipInputField.placeholder is TMP_Text tmp)
                _ipPlaceholderComponent = tmp;
            else
                _ipPlaceholderComponent = _ipInputField.placeholder.GetComponent<Text>();
        }
    }

    private Component ResolveLabel(Component owner)
    {
        if (owner == null)
            return null;

        var tmp = owner.GetComponentInChildren<TMP_Text>(true);
        if (tmp != null)
            return tmp;

        return owner.GetComponentInChildren<Text>(true);
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

    private void TrySubscribeToLobbyManager()
    {
        if (_lobbyManager == null || _subscribed)
            return;

        _lobbyManager.OnPlayersListChanged += UpdatePlayersList;
        _lobbyManager.OnConfigChanged += UpdateConfigDisplay;
        _lobbyManager.OnGameStarted += OnGameStarted;
        _lobbyManager.OnHostStarted += OnHostStarted;
        _lobbyManager.OnClientStarted += OnClientStarted;

        _subscribed = true;
    }

    private void UnsubscribeFromLobbyManager()
    {
        if (_lobbyManager == null || !_subscribed)
            return;

        _lobbyManager.OnPlayersListChanged -= UpdatePlayersList;
        _lobbyManager.OnConfigChanged -= UpdateConfigDisplay;
        _lobbyManager.OnGameStarted -= OnGameStarted;
        _lobbyManager.OnHostStarted -= OnHostStarted;
        _lobbyManager.OnClientStarted -= OnClientStarted;

        _subscribed = false;
    }
    #endregion

    #region Pooling
    private void SetupPool()
    {
        for (int i = 0; i < _initialPoolSize; i++)
        {
            var go = Instantiate(_playerListItemPrefab, _playersListParent);
            go.SetActive(false);
            var item = go.GetComponent<LobbyPlayerListItem>();
            _itemPool.Enqueue(item);
        }
    }

    private LobbyPlayerListItem GetPooledItem()
    {
        while (_itemPool.Count > 0)
        {
            var item = _itemPool.Dequeue();
            if (item != null)
                return item;
        }

        var newGo = Instantiate(_playerListItemPrefab, _playersListParent);
        var newItem = newGo.GetComponent<LobbyPlayerListItem>();
        newGo.SetActive(false);
        return newItem;
    }

    private void RecycleAllActiveItems()
    {
        foreach (var item in _activeItems)
        {
            if (item == null)
                continue;

            item.gameObject.SetActive(false);
            _itemPool.Enqueue(item);
        }

        _activeItems.Clear();
    }
    #endregion

    #region UI handlers
    private void OnHostButtonClicked() => _lobbyManager?.StartHost();

    private void OnClientButtonClicked()
    {
        var ip = _ipInputField?.text;
        if (!string.IsNullOrWhiteSpace(ip))
        {
            _ipInputField.text = "set ip";
        }

        _lobbyManager?.StartClient();
    }

    private void OnStartGameClicked() => _lobbyManager?.StartGame();

    private void OnStartSinglePlayerClicked() => _lobbyManager?.StartSinglePlayer();

    private void OnConfigToggleChanged(bool isOn) => _lobbyManager?.OnConfigToggleChanged(isOn);

    private void OnDisconnectClicked()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }

        ShowConnectionPanel();
    }
    #endregion

    #region Events from LobbyManager
    private void UpdatePlayersList(LobbyPlayerData[] players)
    {
        RecycleAllActiveItems();

        if (players == null)
            players = Array.Empty<LobbyPlayerData>();

        foreach (var player in players)
        {
            var item = GetPooledItem();
            item.transform.SetParent(_playersListParent, false);
            item.Setup(player);
            item.gameObject.SetActive(true);
            _activeItems.Add(item);
        }
    }

    private void UpdateConfigDisplay(int configIndex)
    {
        _lastConfigIndex = configIndex;

        if (_configToggle != null)
            _configToggle.isOn = configIndex == 1;

        if (_lobbyManager == null)
            return;

        var config = _lobbyManager.GetConfigByIndex(configIndex);
        string labelText;

        if (config != null)
        {
            var format = ResolveLocalized(_configLabelFormat, "Selected config: {0}");
            labelText = string.Format(format, config.name);
        }
        else
        {
            labelText = ResolveLocalized(_noConfigLabelText, "No configuration selected");
        }

        if (_configLabel != null)
            _configLabel.text = labelText;

        if (_configLabelTMP != null)
            _configLabelTMP.text = labelText;
    }

    private void OnGameStarted(bool started)
    {
        if (started)
        {
            var message = ResolveLocalized(_gameStartedMessage, "Game started");
            Debug.Log($"[LobbyUI] {message}");
        }
    }

    private void OnHostStarted()
    {
        ShowLobbyPanel();
        SetHostUI();
    }

    private void OnClientStarted()
    {
        ShowLobbyPanel();
        SetClientUI();
    }
    #endregion

    #region Helpers
    private void SetHostUI()
    {
        if (_startGameButton != null)
            _startGameButton.gameObject.SetActive(true);

        if (_configToggle != null)
            _configToggle.interactable = true;
    }

    private void SetClientUI()
    {
        if (_startGameButton != null)
            _startGameButton.gameObject.SetActive(false);

        if (_configToggle != null)
            _configToggle.interactable = false;
    }

    private void ShowConnectionPanel()
    {
        _connectionPanel?.SetActive(true);
        _lobbyPanel?.SetActive(false);
    }

    private void ShowLobbyPanel()
    {
        _connectionPanel?.SetActive(false);
        _lobbyPanel?.SetActive(true);
    }

    private void ApplyLocalization()
    {
        SetComponentText(_hostButtonLabel, ResolveLocalized(_hostLabelText, "Host"));
        SetComponentText(_clientButtonLabel, ResolveLocalized(_clientLabelText, "Join"));
        SetComponentText(_startGameButtonLabel, ResolveLocalized(_startGameLabelText, "Start Game"));

        if (_singlePlayerButton != null)
            SetComponentText(_singlePlayerButtonLabel, ResolveLocalized(_singlePlayerLabelText, "Single Player"));

        SetComponentText(_disconnectButtonLabel, ResolveLocalized(_disconnectLabelText, "Disconnect"));
        SetComponentText(_configToggleLabelComponent, ResolveLocalized(_configToggleLabelText, "Use alt config"));
        SetComponentText(_ipPlaceholderComponent, ResolveLocalized(_ipPlaceholderText, "Enter IP..."));

        UpdateConfigDisplay(_lastConfigIndex);
    }

    private static void SetComponentText(Component target, string value)
    {
        if (target == null || string.IsNullOrEmpty(value))
            return;

        if (target is TMP_Text tmp)
            tmp.text = value;
        else if (target is Text legacy)
            legacy.text = value;
    }

    private void OnLocalizationLanguageChanged(Locale _)
    {
        ApplyLocalization();
    }

    private void EnsureLocalizationDefaults()
    {
        EnsureEntry(ref _hostLabelText, "MainMenu.Host");
        EnsureEntry(ref _clientLabelText, "MainMenu.Client");
        EnsureEntry(ref _startGameLabelText, "MainMenu.StartGame");
        EnsureEntry(ref _singlePlayerLabelText, "MainMenu.SinglePlayer");
        EnsureEntry(ref _disconnectLabelText, "MainMenu.Disconnect");
        EnsureEntry(ref _configToggleLabelText, "MainMenu.ConfigToggle");
        EnsureEntry(ref _ipPlaceholderText, "MainMenu.IpPlaceholder");
        EnsureEntry(ref _configLabelFormat, "MainMenu.ConfigLabel");
        EnsureEntry(ref _noConfigLabelText, "MainMenu.NoConfig");
        EnsureEntry(ref _gameStartedMessage, "System.GameStarted");
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
    #endregion
}
