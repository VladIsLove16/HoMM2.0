// Assets/Editor/LobbyUICreator.cs
// Создаёт структуру UI для лобби с корректными позициями панелей, аккуратным Toggle и отступами.

using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public static class LobbyUICreator
{
    [MenuItem("Tools/Create Lobby UI")]
    public static void CreateLobbyUI()
    {
        // Цветовая палитра
        Color32 panelBg = new Color32(14, 16, 19, 255);
        Color32 cardBg = new Color32(23, 26, 31, 255);
        Color32 buttonNormal = new Color32(31, 41, 55, 255);
        Color32 buttonHover = new Color32(60, 97, 150, 255);
        Color32 buttonPressed = new Color32(18, 24, 32, 255);
        Color32 accent = new Color32(96, 165, 250, 255);
        Color32 textColor = new Color32(255, 255, 255, 255);
        Color32 placeholderColor = new Color32(179, 179, 179, 255);
        Color32 viewportColor = new Color32(0, 0, 0, 51);

        // Canvas
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("Canvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGO.AddComponent<GraphicRaycaster>();
            if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject es = new GameObject("EventSystem");
                es.AddComponent<UnityEngine.EventSystems.EventSystem>();
                es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
        }

        GameObject root = canvas.gameObject;

        // LobbyUI
        LobbyUI lobbyUI = root.GetComponent<LobbyUI>();
        if (lobbyUI == null)
            lobbyUI = root.AddComponent<LobbyUI>();

        // ConnectionPanel
        GameObject connectionPanel = new GameObject("ConnectionPanel", typeof(RectTransform));
        connectionPanel.transform.SetParent(root.transform, false);
        var connImg = connectionPanel.AddComponent<Image>();
        connImg.color = panelBg;
        VerticalLayoutGroup connLayout = connectionPanel.AddComponent<VerticalLayoutGroup>();
        connLayout.childForceExpandHeight = false;
        connLayout.childForceExpandWidth = false;
        connLayout.spacing = 8;
        connLayout.padding = new RectOffset(10, 10, 10, 10);

        RectTransform connRT = connectionPanel.GetComponent<RectTransform>();
        connRT.anchorMin = connRT.anchorMax = connRT.pivot = new Vector2(0.5f, 0.5f);
        connRT.anchoredPosition = new Vector2(-250, 0);
        connRT.sizeDelta = new Vector2(300, 200);

        InputField ipInput = CreateInputField("IP Input", connectionPanel.transform, "127.0.0.1", cardBg, textColor, placeholderColor, accent);
        Button hostButton = CreateButton("Host Button", connectionPanel.transform, "Host", buttonNormal, buttonHover, buttonPressed, textColor);
        Button clientButton = CreateButton("Client Button", connectionPanel.transform, "Client", buttonNormal, buttonHover, buttonPressed, textColor);

        // LobbyPanel
        GameObject lobbyPanel = new GameObject("LobbyPanel", typeof(RectTransform));
        lobbyPanel.transform.SetParent(root.transform, false);
        var lobbyImg = lobbyPanel.AddComponent<Image>();
        lobbyImg.color = panelBg;
        VerticalLayoutGroup lobbyLayout = lobbyPanel.AddComponent<VerticalLayoutGroup>();
        lobbyLayout.childForceExpandHeight = false;
        lobbyLayout.childForceExpandWidth = false;
        lobbyLayout.spacing = 8;
        lobbyLayout.padding = new RectOffset(10, 10, 10, 10);

        RectTransform lobbyRT = lobbyPanel.GetComponent<RectTransform>();
        lobbyRT.anchorMin = lobbyRT.anchorMax = lobbyRT.pivot = new Vector2(0.5f, 0.5f);
        lobbyRT.anchoredPosition = new Vector2(250, 0);
        lobbyRT.sizeDelta = new Vector2(420, 400);

        // ScrollRect
        GameObject scrollGO = new GameObject("PlayersScroll", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
        scrollGO.transform.SetParent(lobbyPanel.transform, false);
        ScrollRect scroll = scrollGO.GetComponent<ScrollRect>();
        RectTransform scrollRT = scrollGO.GetComponent<RectTransform>();
        scrollRT.sizeDelta = new Vector2(400, 300);
        scrollGO.GetComponent<Image>().color = cardBg;

        GameObject viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Mask), typeof(Image));
        viewport.transform.SetParent(scrollGO.transform, false);
        Image viewportImg = viewport.GetComponent<Image>();
        viewportImg.color = viewportColor;
        viewport.GetComponent<Mask>().showMaskGraphic = false;
        scroll.viewport = viewport.GetComponent<RectTransform>();

        GameObject content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        content.transform.SetParent(viewport.transform, false);
        var contentLayout = content.GetComponent<VerticalLayoutGroup>();
        contentLayout.childForceExpandHeight = false;
        contentLayout.childForceExpandWidth = true;
        contentLayout.spacing = 6;
        contentLayout.padding = new RectOffset(4, 4, 4, 4);
        var csf = content.GetComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scroll.content = content.GetComponent<RectTransform>();

        // Lobby controls
        Toggle configToggle = CreateToggle("Config Toggle", lobbyPanel.transform, "Use Config", cardBg, accent, textColor);
        Text configLabel = CreateText("Config Label", lobbyPanel.transform, "Config: none", textColor);
        Button startGameButton = CreateButton("Start Game Button", lobbyPanel.transform, "Start Game", buttonNormal, buttonHover, buttonPressed, textColor);
        Button disconnectButton = CreateButton("Disconnect Button", lobbyPanel.transform, "Disconnect", buttonNormal, buttonHover, buttonPressed, textColor);

        // Player item prefab
        GameObject playerItemPrefab = new GameObject("PlayerListItem", typeof(RectTransform));
        var itemScript = playerItemPrefab.AddComponent<LobbyPlayerListItem>();
        var itemImg = playerItemPrefab.AddComponent<Image>();
        itemImg.color = cardBg;
        VerticalLayoutGroup itemLayout = playerItemPrefab.AddComponent<VerticalLayoutGroup>();
        itemLayout.childForceExpandHeight = false;
        itemLayout.childForceExpandWidth = true;
        itemLayout.spacing = 2;
        itemLayout.padding = new RectOffset(8, 8, 4, 4);

        Text nameText = CreateText("Name", playerItemPrefab.transform, "PlayerName", textColor);
        Text statusText = CreateText("Status", playerItemPrefab.transform, "Not Ready", placeholderColor);

        typeof(LobbyPlayerListItem).GetField("_nameText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(itemScript, nameText);
        typeof(LobbyPlayerListItem).GetField("_statusText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(itemScript, statusText);

        string prefabPath = "Assets/PlayerListItem.prefab";
        PrefabUtility.SaveAsPrefabAsset(playerItemPrefab, prefabPath);
        GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        Object.DestroyImmediate(playerItemPrefab);

        // Bind references
        var so = new SerializedObject(lobbyUI);
        so.FindProperty("_connectionPanel").objectReferenceValue = connectionPanel;
        so.FindProperty("_lobbyPanel").objectReferenceValue = lobbyPanel;
        so.FindProperty("_hostButton").objectReferenceValue = hostButton;
        so.FindProperty("_clientButton").objectReferenceValue = clientButton;
        so.FindProperty("_ipInputField").objectReferenceValue = ipInput;
        so.FindProperty("_playersListParent").objectReferenceValue = content.GetComponent<RectTransform>();
        so.FindProperty("_playerListItemPrefab").objectReferenceValue = prefabAsset;
        so.FindProperty("_startGameButton").objectReferenceValue = startGameButton;
        so.FindProperty("_configToggle").objectReferenceValue = configToggle;
        so.FindProperty("_configLabel").objectReferenceValue = configLabel;
        so.FindProperty("_disconnectButton").objectReferenceValue = disconnectButton;
        so.ApplyModifiedProperties();

        Debug.Log("Lobby UI создан и настроен с позициями, Toggle и отступами.");
    }

    private static Button CreateButton(string name, Transform parent, string text, Color normal, Color highlighted, Color pressed, Color textColor)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Button), typeof(Image));
        go.transform.SetParent(parent, false);
        Image img = go.GetComponent<Image>();
        img.color = normal;

        Button button = go.GetComponent<Button>();
        var cb = ColorBlock.defaultColorBlock;
        cb.normalColor = normal;
        cb.highlightedColor = highlighted;
        cb.pressedColor = pressed;
        cb.disabledColor = new Color(0.4f, 0.4f, 0.4f, 1f);
        button.colors = cb;

        Text label = CreateText("Label", go.transform, text, textColor);
        label.alignment = TextAnchor.MiddleCenter;

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(180, 34);
        return button;
    }

    private static InputField CreateInputField(string name, Transform parent, string placeholder, Color bgColor, Color textColor, Color placeholderCol, Color selectionColor)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(InputField));
        go.transform.SetParent(parent, false);
        Image bg = go.GetComponent<Image>();
        bg.color = bgColor;

        InputField input = go.GetComponent<InputField>();
        Text textComp = CreateText("Text", go.transform, "", textColor);
        input.textComponent = textComp;
        Text placeholderText = CreateText("Placeholder", go.transform, placeholder, placeholderCol);
        input.placeholder = placeholderText;
        input.selectionColor = selectionColor;

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(240, 28);
        return input;
    }

    private static Toggle CreateToggle(string name, Transform parent, string labelText, Color bgColor, Color checkColor, Color textColor)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Toggle));
        go.transform.SetParent(parent, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(200, 24);

        Image bg = go.AddComponent<Image>();
        bg.color = bgColor;

        Toggle toggle = go.GetComponent<Toggle>();
        toggle.targetGraphic = bg;

        GameObject check = new GameObject("Checkmark", typeof(RectTransform), typeof(Image));
        check.transform.SetParent(go.transform, false);
        RectTransform checkRT = check.GetComponent<RectTransform>();
        checkRT.anchorMin = new Vector2(0, 0.5f);
        checkRT.anchorMax = new Vector2(0, 0.5f);
        checkRT.pivot = new Vector2(0.5f, 0.5f);
        checkRT.anchoredPosition = new Vector2(12, 0);
        checkRT.sizeDelta = new Vector2(20, 20);
        Image checkImg = check.GetComponent<Image>();
        checkImg.color = checkColor;
        toggle.graphic = checkImg;

        Text label = CreateText("Label", go.transform, labelText, textColor);
        RectTransform labelRT = label.GetComponent<RectTransform>();
        labelRT.anchorMin = new Vector2(0, 0.5f);
        labelRT.anchorMax = new Vector2(0, 0.5f);
        labelRT.pivot = new Vector2(0, 0.5f);
        labelRT.anchoredPosition = new Vector2(40, 0);
        label.alignment = TextAnchor.MiddleLeft;

        return toggle;
    }

    private static Text CreateText(string name, Transform parent, string text, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        Text txt = go.GetComponent<Text>();
        txt.text = text;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize = 14;
        txt.color = color;
        txt.raycastTarget = false;
        return txt;
    }
}
