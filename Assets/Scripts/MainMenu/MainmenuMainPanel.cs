using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

[DefaultExecutionOrder(-1000)]
public sealed class MainmenuMainPanel : MonoBehaviour
{
    [Header("Static Buttons")]
    [SerializeField] private Button SingleplayerButton;
    [SerializeField] private Button NetworkPlayButton;
    [SerializeField] private Button AchievementsButton;
    [SerializeField] private Button SettingsButton;
    [SerializeField] private Button QuitButton;

    [Header("Panels")]
    [SerializeField] private GameObject mainMenuRoot;
    [SerializeField] private RectTransform networkMenuRoot;
    [SerializeField] private LobbyUI lobbyUI;

    private MainMenuViewModel _viewModel;
    private CompositeDisposable _bindings;
    private bool _staticReferencesValid;
    private CanvasGroup _mainMenuCanvasGroup;
    private CanvasGroup _networkMenuCanvasGroup;

    private void Reset()
    {
        TryResolveEmbeddedLobbyReferences();
        TryResolveMainMenuRoot();
    }

    private void OnValidate()
    {
        TryResolveEmbeddedLobbyReferences();
        TryResolveMainMenuRoot();
    }

    [Inject]
    public void Construct(MainMenuViewModel viewModel)
    {
        _viewModel = viewModel;
        TryBindViewModel();
    }

    private void Awake()
    {
        if (!ValidateRequiredReferences(out var errorMessage))
        {
            Debug.LogError(errorMessage, this);
            enabled = false;
            return;
        }

        PreparePanelCanvasGroups();
        _staticReferencesValid = true;
        TryBindViewModel();
    }

    private void OnEnable()
    {
        TryBindViewModel();
    }

    private void OnDisable()
    {
        _bindings?.Dispose();
        _bindings = null;
    }

    public void OnPlayButtonClicked()
    {
        _viewModel?.StartSinglePlayer();
    }

    public void OnSettingsButtonClicked()
    {
        _viewModel?.ToggleSettings();
    }

    public void OnAchievementsButtonClicked()
    {
        _viewModel?.ToggleAChievements();
    }

    public void OnQuitButtonClicked()
    {
        _viewModel?.QuitGame();
    }

    private bool ValidateRequiredReferences(out string errorMessage)
    {
        if (SingleplayerButton == null)
        {
            errorMessage = "[MainmenuMainPanel] SingleplayerButton is not assigned.";
            return false;
        }

        if (NetworkPlayButton == null)
        {
            errorMessage = "[MainmenuMainPanel] NetworkPlayButton is not assigned.";
            return false;
        }

        if (SettingsButton == null)
        {
            errorMessage = "[MainmenuMainPanel] SettingsButton is not assigned.";
            return false;
        }

        if (AchievementsButton == null)
        {
            errorMessage = "[MainmenuMainPanel] AchievementsButton is not assigned.";
            return false;
        }

        if (QuitButton == null)
        {
            errorMessage = "[MainmenuMainPanel] QuitButton is not assigned.";
            return false;
        }

        if (mainMenuRoot == null)
        {
            errorMessage = "[MainmenuMainPanel] Main menu root is not assigned.";
            return false;
        }

        if (networkMenuRoot == null)
        {
            errorMessage = "[MainmenuMainPanel] Network menu root is not assigned.";
            return false;
        }

        if (lobbyUI == null)
        {
            errorMessage = "[MainmenuMainPanel] LobbyUI is not assigned.";
            return false;
        }

        errorMessage = null;
        return true;
    }

    private void BindButtons()
    {
        BindButton(SingleplayerButton, OnPlayButtonClicked);
        BindButton(NetworkPlayButton, OnNetworkPlayButtonClicked);
        BindButton(AchievementsButton, OnAchievementsButtonClicked);
        BindButton(SettingsButton, OnSettingsButtonClicked);
        BindButton(QuitButton, OnQuitButtonClicked);
    }

    private void TryBindViewModel()
    {
        if (!_staticReferencesValid || !isActiveAndEnabled || _viewModel == null || _bindings != null)
            return;

        BindButtons();
        UpdateButtonsState();

        _bindings = new CompositeDisposable();
        _viewModel.ActiveScreen
            .DistinctUntilChanged()
            .Subscribe(ApplyScreen)
            .AddTo(_bindings);
    }

    private void OnNetworkPlayButtonClicked()
    {
        _viewModel?.OpenNetworkLobby();
    }

    private void ShowLobbyRoot()
    {
        SetPanelVisible(_networkMenuCanvasGroup, true);
    }

    private void ShowLobbyConnectionFlow()
    {
        SetMainMenuVisible(false);
        ShowLobbyRoot();
        lobbyUI.OpenConnectionFlow();
    }

    private void HideNetworkMenu()
    {
        SetPanelVisible(_networkMenuCanvasGroup, false);
    }

    private void UpdateButtonsState()
    {
        if (AchievementsButton != null)
            AchievementsButton.interactable = _viewModel != null && _viewModel.CanOpenAchievements;

        if (SettingsButton != null)
            SettingsButton.interactable = _viewModel != null && _viewModel.CanOpenSettings;
    }

    private static void BindButton(Button button, UnityEngine.Events.UnityAction callback)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(callback);
    }

    private void ApplyScreen(MainMenuScreen screen)
    {
        switch (screen)
        {
            case MainMenuScreen.NetworkConnection:
                ShowLobbyConnectionFlow();
                break;
            default:
                SetMainMenuVisible(true);
                HideNetworkMenu();
                break;
        }
    }

    private void TryResolveEmbeddedLobbyReferences()
    {
        if (networkMenuRoot == null && lobbyUI != null)
            networkMenuRoot = lobbyUI.transform as RectTransform;

        if (lobbyUI == null && networkMenuRoot != null)
            lobbyUI = networkMenuRoot.GetComponent<LobbyUI>();

        if (networkMenuRoot == null)
        {
            var embeddedLobby = GetComponentInChildren<LobbyUI>(true);
            if (embeddedLobby != null)
            {
                lobbyUI = embeddedLobby;
                networkMenuRoot = embeddedLobby.transform as RectTransform;
            }
        }
    }

    private void TryResolveMainMenuRoot()
    {
        if (mainMenuRoot != null)
            return;

        var commonParent = ResolveCommonButtonsParent();
        if (commonParent != null)
            mainMenuRoot = commonParent.gameObject;
    }

    private Transform ResolveCommonButtonsParent()
    {
        if (SingleplayerButton == null ||
            NetworkPlayButton == null ||
            AchievementsButton == null ||
            SettingsButton == null ||
            QuitButton == null)
        {
            return null;
        }

        var candidate = SingleplayerButton.transform.parent;
        if (candidate == null)
            return null;

        return NetworkPlayButton.transform.parent == candidate &&
               AchievementsButton.transform.parent == candidate &&
               SettingsButton.transform.parent == candidate &&
               QuitButton.transform.parent == candidate
            ? candidate
            : null;
    }

    private void SetMainMenuVisible(bool visible)
    {
        if (_mainMenuCanvasGroup != null)
        {
            SetPanelVisible(_mainMenuCanvasGroup, visible);
            return;
        }

        SetControlVisible(SingleplayerButton, visible);
        SetControlVisible(NetworkPlayButton, visible);
        SetControlVisible(AchievementsButton, visible);
        SetControlVisible(SettingsButton, visible);
        SetControlVisible(QuitButton, visible);
    }

    private void PreparePanelCanvasGroups()
    {
        _mainMenuCanvasGroup = EnsureCanvasGroup(mainMenuRoot);
        _networkMenuCanvasGroup = EnsureCanvasGroup(networkMenuRoot != null ? networkMenuRoot.gameObject : null);
    }

    private static CanvasGroup EnsureCanvasGroup(GameObject target)
    {
        if (target == null)
            return null;

        target.SetActive(true);

        var group = target.GetComponent<CanvasGroup>();
        if (group != null)
            return group;

        return target.AddComponent<CanvasGroup>();
    }

    private static void SetPanelVisible(CanvasGroup group, bool visible)
    {
        if (group == null)
            return;

        group.alpha = visible ? 1f : 0f;
        group.interactable = visible;
        group.blocksRaycasts = visible;
    }

    private static void SetControlVisible(Selectable selectable, bool visible)
    {
        if (selectable == null)
            return;

        selectable.gameObject.SetActive(visible);
    }
}
