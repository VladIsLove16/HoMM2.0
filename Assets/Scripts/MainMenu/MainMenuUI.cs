using Adventure.Infrastructure.Persistence;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

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

    private void Awake()
    {
        ApplySavedLocale();
        TryResolveEmbeddedLobbyReferences();

        if (!ValidateRequiredReferences(out var errorMessage))
        {
            Debug.LogError(errorMessage, this);
            enabled = false;
            return;
        }

        HideNetworkMenu();
        HideSettingsPanel();

        if (settingsPanelRoot == null)
        {
            SettingsButton.interactable = false;
            Debug.LogWarning("[MainMenuUI] Settings panel is not assigned. Settings button is disabled.", this);
        }

        ShowPrimaryMenu();
    }

    private static void ApplySavedLocale()
    {
        LocalizationSettings.InitializationOperation.WaitForCompletion();

        var locales = LocalizationSettings.AvailableLocales?.Locales;
        if (locales == null || locales.Count == 0)
            return;

        var storage = new JsonFileStorage();
        var data = storage.Load<GameSettingsSaveData>("game-settings");
        if (data == null)
            return;

        var index = Mathf.Clamp(data.LanguageIndex, 0, locales.Count - 1);
        var targetLocale = locales[index];
        if (targetLocale != null && LocalizationSettings.SelectedLocale != targetLocale)
        {
            LocalizationSettings.SelectedLocale = targetLocale;
        }
    }

    public void OnPlayButtonClicked()
    {
        StartSinglePlayer();
    }

    public void OnSettingsButtonClicked()
    {
        ToggleSettingsPanel();
    }

    public void OnQuitButtonClicked()
    {
        QuitGame();
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

    private void ShowPrimaryMenu()
    {
        SetMainMenuVisible(true);
        HideNetworkMenu();
        HideSettingsPanel();
        BindButton(SingleplayerButton, StartSinglePlayer);
        BindButton(NetworkPlayButton, OpenNetworkLobby);
        BindButton(SettingsButton, ToggleSettingsPanel);
        BindButton(QuitButton, QuitGame);
    }

    private void StartSinglePlayer()
    {
        SceneLoader.Load(SceneLoader.Scene.Adventure);
    }

    private void OpenNetworkLobby()
    {
        GameLaunchPreferences.SetMultiplayerStartScene(SceneLoader.Scene.Adventure);
        ShowLobbyConnectionFlow();
    }

    private void ToggleSettingsPanel()
    {
        if (settingsPanelRoot == null)
        {
            Debug.LogWarning("[MainMenuUI] Settings panel is not assigned.", this);
            return;
        }

        var show = !settingsPanelRoot.gameObject.activeSelf;
        settingsPanelRoot.gameObject.SetActive(show);

        if (show)
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

    private static void QuitGame()
    {
        Application.Quit();
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
