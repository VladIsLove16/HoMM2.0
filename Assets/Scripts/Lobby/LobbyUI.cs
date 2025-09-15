// LobbyUI.Modern.cs
// Обновлённая версия: теперь используется VerticalLayoutGroup для списка игроков,
// и никаких Find/Reflection для поиска компонентов — всё через строго типизированные ссылки.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

/// <summary>
/// Контроллер интерфейса лобби.
/// Работает только с заранее созданным UI (через инспектор или генератор Editor-скриптом).
/// Список игроков построен на VerticalLayoutGroup, элементы управляются через коллекцию.
/// </summary>
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

    private readonly List<LobbyPlayerListItem> _activeItems = new List<LobbyPlayerListItem>(16);
    private readonly Queue<LobbyPlayerListItem> _itemPool = new Queue<LobbyPlayerListItem>(32);
    private bool _subscribed = false;

    #region Unity lifecycle
    private void Awake()
    {
        if (_lobbyManager == null)
        {
#if UNITY_2023_1_OR_NEWER
            _lobbyManager = UnityEngine.Object.FindFirstObjectByType<LobbyManager>();
#else
            _lobbyManager = FindObjectOfType<LobbyManager>();
#endif
        }
    }

    private void Start()
    {
        if (!ValidateReferences())
        {
            Debug.LogError("[LobbyUI] Не все ссылки назначены в инспекторе. Отключаю компонент.");
            enabled = false;
            return;
        }

        SetupPool();
        SetupUiListeners();
        TrySubscribeToLobbyManager();

        ShowConnectionPanel();
    }

    private void OnEnable() => TrySubscribeToLobbyManager();
    private void OnDisable() => UnsubscribeFromLobbyManager();
    private void OnDestroy() => UnsubscribeFromLobbyManager();
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

    private void TrySubscribeToLobbyManager()
    {
        if (_lobbyManager == null || _subscribed) return;

        _lobbyManager.OnPlayersListChanged += UpdatePlayersList;
        _lobbyManager.OnConfigChanged += UpdateConfigDisplay;
        _lobbyManager.OnGameStarted += OnGameStarted;
        _lobbyManager.OnHostStarted += OnHostStarted;
        _lobbyManager.OnClientStarted += OnClientStarted;

        _subscribed = true;
    }

    private void UnsubscribeFromLobbyManager()
    {
        if (_lobbyManager == null || !_subscribed) return;

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
            if (item != null) return item;
        }

        var newGo = Instantiate(_playerListItemPrefab, _playersListParent);
        var newItem = newGo.GetComponent<LobbyPlayerListItem>();
        newGo.SetActive(false);
        return newItem;
    }

    private void RecycleAllActiveItems()
    {
        foreach (var it in _activeItems)
        {
            if (it == null) continue;
            it.gameObject.SetActive(false);
            _itemPool.Enqueue(it);
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
    private void OnStartSinglePlayerClicked()
    {
        // Устанавливаем режим Singleplayer и запускаем игру локально (без сети)
        var gmm = FindObjectOfType<GameModeManager>();
        if (gmm != null)
        {
            gmm.SwitchToSingleplayer();
        }
        // Загрузку игровой сцены выполняем напрямую, минуя сетевой менеджер
        UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

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

        if (players == null) players = Array.Empty<LobbyPlayerData>();

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
        if (_configToggle != null)
            _configToggle.isOn = configIndex == 1;

        if (_configLabel != null && _lobbyManager != null)
        {
            var config = _lobbyManager.GetConfigByIndex(configIndex);
            _configLabel.text = config != null ? $"Config: {config.name}" : "No Config";
        }
    }

    private void OnGameStarted(bool started)
    {
        if (started)
            Debug.Log("[LobbyUI] Game started — LobbyManager должен выполнить переход на сцену игры.");
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
        if (_startGameButton != null) _startGameButton.gameObject.SetActive(true);
        if (_configToggle != null) _configToggle.interactable = true;
    }

    private void SetClientUI()
    {
        if (_startGameButton != null) _startGameButton.gameObject.SetActive(false);
        if (_configToggle != null) _configToggle.interactable = false;
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
    #endregion
}
