using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

[DefaultExecutionOrder(-1000)]
public class mainmenuUI : MonoBehaviour
{
    [Header("Static Buttons")]
    [SerializeField] private Button SingleplayerButton;
    [SerializeField] private Button NetworkPlayButton;
    [SerializeField] private Button SettingsButton;
    [SerializeField] private Button QuitButton;

    [Header("Panels")]
    [SerializeField] private GameObject mainMenuRoot;
    [SerializeField] private RectTransform networkMenuRoot;
    [SerializeField] private RectTransform settingsPanelRoot;
    [SerializeField] private LobbyUI lobbyUI;

    private MainMenuViewModel _viewModel;
    private CompositeDisposable _bindings;
    private bool _staticReferencesValid;

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
        TryResolveEmbeddedLobbyReferences();

        if (!ValidateRequiredReferences(out var errorMessage))
        {
            Debug.LogError(errorMessage, this);
            enabled = false;
            return;
        }

        if (settingsPanelRoot == null)
        {
            SettingsButton.interactable = false;
            Debug.LogWarning("[MainMenuUI] Settings panel is not assigned. Settings button is disabled.", this);
        }

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
        _viewModel?.ToggleSettingsPanel();
    }

    public void OnQuitButtonClicked()
    {
        _viewModel?.QuitGame();
    }

    private bool ValidateRequiredReferences(out string errorMessage)
    {
        if (SingleplayerButton == null)
        {
            errorMessage = "[MainMenuUI] SingleplayerButton is not assigned.";
            return false;
        }

        if (NetworkPlayButton == null)
        {
            errorMessage = "[MainMenuUI] NetworkPlayButton is not assigned.";
            return false;
        }

        if (SettingsButton == null)
        {
            errorMessage = "[MainMenuUI] SettingsButton is not assigned.";
            return false;
        }

        if (QuitButton == null)
        {
            errorMessage = "[MainMenuUI] QuitButton is not assigned.";
            return false;
        }

        if (networkMenuRoot == null)
        {
            errorMessage = "[MainMenuUI] Network menu root is not assigned.";
            return false;
        }

        if (lobbyUI == null)
        {
            errorMessage = "[MainMenuUI] LobbyUI is not assigned.";
            return false;
        }

        errorMessage = null;
        return true;
    }

    private void BindButtons()
    {
        BindButton(SingleplayerButton, OnPlayButtonClicked);
        BindButton(NetworkPlayButton, () => _viewModel?.OpenNetworkLobby());
        BindButton(SettingsButton, OnSettingsButtonClicked);
        BindButton(QuitButton, OnQuitButtonClicked);
    }

    private void TryBindViewModel()
    {
        if (!_staticReferencesValid || !isActiveAndEnabled || _viewModel == null || _bindings != null)
            return;

        BindButtons();

        _bindings = new CompositeDisposable();
        _viewModel.IsSettingsPanelOpen
            .Subscribe(SetSettingsPanelVisible)
            .AddTo(_bindings);
        _viewModel.ActiveScreen
            .Subscribe(ApplyScreen)
            .AddTo(_bindings);
    }

    private void SetSettingsPanelVisible(bool visible)
    {
        if (settingsPanelRoot == null)
        {
            if (visible)
            {
                Debug.LogWarning("[MainMenuUI] Settings panel is not assigned.", this);
            }

            return;
        }

        settingsPanelRoot.gameObject.SetActive(visible);

        if (visible)
            HideNetworkMenu();
    }

    private void HideSettingsPanel()
    {
        if (settingsPanelRoot != null)
            settingsPanelRoot.gameObject.SetActive(false);
    }

    private void ShowLobbyRoot()
    {
        networkMenuRoot.gameObject.SetActive(true);
    }

    private void ShowLobbyConnectionFlow()
    {
        HideSettingsPanel();
        SetMainMenuVisible(false);
        ShowLobbyRoot();
        lobbyUI.OpenConnectionFlow();
    }

    private void HideNetworkMenu()
    {
        networkMenuRoot.gameObject.SetActive(false);
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
            SettingsButton == null ||
            QuitButton == null)
        {
            return null;
        }

        var candidate = SingleplayerButton.transform.parent;
        if (candidate == null)
            return null;

        return NetworkPlayButton.transform.parent == candidate &&
               SettingsButton.transform.parent == candidate &&
               QuitButton.transform.parent == candidate
            ? candidate
            : null;
    }

    private void SetMainMenuVisible(bool visible)
    {
        if (mainMenuRoot != null)
        {
            mainMenuRoot.SetActive(visible);
            return;
        }

        SingleplayerButton.gameObject.SetActive(visible);
        NetworkPlayButton.gameObject.SetActive(visible);
        SettingsButton.gameObject.SetActive(visible);
        QuitButton.gameObject.SetActive(visible);
    }

}
