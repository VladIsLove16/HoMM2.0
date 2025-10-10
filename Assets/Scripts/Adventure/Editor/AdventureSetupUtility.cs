using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Zenject;
using Adventure.Application.Dialog;
using Adventure.Presentation.Dialog;
using Adventure.Presentation.Interaction;
using Adventure.Infrastructure.Dialog;
using Adventure.Domain.Dialog;
using Adventure.Integration.Battle;
using Adventure.Presentation.Mushroom;
using Adventure.Infrastructure.Movement;
using Adventure.Infrastructure.Interaction;
using Assets.Scripts.Adventure.Infrastructure.Input;
public static class AdventureSetupUtility
{
    private const string RootFolder = "Assets/Adventure/Generated";
    private const string MovementSettingsAssetName = "MovementSettings.asset";
    private const string MushroomCatalogAssetName = "MushroomCatalog.asset";
    private const string DialogueDatabaseAssetName = "DialogueDatabase.asset";
    private const string DialogueGraphAssetName = "SampleDialogue.asset";
    private const string DialogueExitNodeAssetName = "SampleDialogueExit.asset";
    private const string DialogueBattleNodeAssetName = "SampleDialogueBattle.asset";
    private const string ArmyLineupAssetName = "ArmyLineup.asset";
    private const string GameConfigurationServiceAssetName = "GameConfigurationService.asset";
    private const string InputActionsAssetPath = "Assets/Scripts/Input/InputSystem_Actions/InputSystem_Adventure.inputactions";

    [MenuItem("Tools/Adventure/Setup Scene")]
    public static void SetupScene()
    {
        try
        {
            EnsureFolderStructure();

            var movementSettings = EnsureMovementSettings();
            var catalog = EnsureMushroomCatalog();
            var (dialogueDatabase, startNode, exitNode) = EnsureDialogueAssets();
            var lineup = EnsureArmyLineup();
            var configurationService = EnsureGameConfigurationService();

            var systemsRoot = EnsureSystemsRoot();
            var promptView = EnsureInteractionPromptView(systemsRoot);
            var playerRoot = EnsurePlayer(movementSettings, promptView);

            var gateway = EnsureGridConfigurationGateway(systemsRoot, configurationService);
            var dialogueView = EnsureDialogueView(systemsRoot);
            var orchestrator = EnsureDialogueOrchestrator(systemsRoot, dialogueView, lineup);

            var interaction = playerRoot.GetComponent<PlayerInteractionController>();
            if (interaction == null)
                interaction = playerRoot.AddComponent<PlayerInteractionController>();
            ConfigureInteractionController(interaction, playerRoot, promptView);

            var installer = systemsRoot.GetComponent<AdventureGameplayInstaller>();
            if (installer == null)
                installer = systemsRoot.AddComponent<AdventureGameplayInstaller>();
            BindInstaller(installer, playerRoot.GetComponent<PlayerMovementController>(), catalog, dialogueDatabase,
                gateway, lineup, interaction , orchestrator);

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();
            Debug.Log("Adventure setup complete.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Adventure setup failed: {ex}");
        }
    }

    private static void EnsureFolderStructure()
    {
        EnsureFolder("Assets", "Adventure");
        EnsureFolder("Assets/Adventure", "Generated");
    }

    private static string EnsureFolder(string parent, string folder)
    {
        var path = Path.Combine(parent, folder).Replace("\\", "/");
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder(parent, folder);
        }
        return path;
    }

    private static MovementSettingsSO EnsureMovementSettings()
    {
        var path = Path.Combine(RootFolder, MovementSettingsAssetName).Replace("\\", "/");
        var settings = AssetDatabase.LoadAssetAtPath<MovementSettingsSO>(path);
        if (settings == null)
        {
            settings = ScriptableObject.CreateInstance<MovementSettingsSO>();
            AssetDatabase.CreateAsset(settings, path);
        }

        var so = new SerializedObject(settings);
        so.FindProperty("moveSpeed").floatValue = 4f;
        so.FindProperty("sprintMultiplier").floatValue = 1.5f;
        so.FindProperty("acceleration").floatValue = 10f;
        so.FindProperty("gravity").floatValue = -9.81f;
        so.FindProperty("stepHeight").floatValue = 0.5f;
        so.FindProperty("groundSnapDistance").floatValue = 0.2f;
        so.FindProperty("lookSensitivity").floatValue = 1f;
        so.FindProperty("maxLookPitch").floatValue = 80f;
        so.FindProperty("collisionMask").intValue = ~0;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(settings);
        return settings;
    }

    private static MushroomCatalogSO EnsureMushroomCatalog()
    {
        var path = Path.Combine(RootFolder, MushroomCatalogAssetName).Replace("\\", "/");
        var catalog = AssetDatabase.LoadAssetAtPath<MushroomCatalogSO>(path);
        if (catalog == null)
        {
            catalog = ScriptableObject.CreateInstance<MushroomCatalogSO>();
            AssetDatabase.CreateAsset(catalog, path);
        }

        var so = new SerializedObject(catalog);
        var entries = so.FindProperty("entries");
        if (entries.arraySize == 0)
            entries.arraySize = 1;
        var entry = entries.GetArrayElementAtIndex(0);
        entry.FindPropertyRelative("Id").stringValue = "mushroom_sample";
        entry.FindPropertyRelative("DisplayName").stringValue = "Sample Mushroom";
        entry.FindPropertyRelative("Description").stringValue = "A mysterious mushroom found near the camp.";
        entry.FindPropertyRelative("Icon").objectReferenceValue = null;
        entry.FindPropertyRelative("IconHovered").objectReferenceValue = null;
        entry.FindPropertyRelative("HumanizedIcon").objectReferenceValue = null;
        entry.FindPropertyRelative("HumanizedIconHovered").objectReferenceValue = null;
        var characteristics = entry.FindPropertyRelative("Characteristics");
        if (characteristics.arraySize == 0)
            characteristics.arraySize = 1;
        characteristics.GetArrayElementAtIndex(0).stringValue = "Weight: Light";
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(catalog);
        return catalog;
    }

    private static (DialogueDatabaseSO database, DialogueNodeSO startNode, DialogueNodeSO exitNode) EnsureDialogueAssets()
    {
        var startPath = Path.Combine(RootFolder, DialogueBattleNodeAssetName).Replace("\\", "/");
        var exitPath = Path.Combine(RootFolder, DialogueExitNodeAssetName).Replace("\\", "/");
        var graphPath = Path.Combine(RootFolder, DialogueGraphAssetName).Replace("\\", "/");
        var dbPath = Path.Combine(RootFolder, DialogueDatabaseAssetName).Replace("\\", "/");

        var startNode = AssetDatabase.LoadAssetAtPath<DialogueNodeSO>(startPath);
        if (startNode == null)
        {
            startNode = ScriptableObject.CreateInstance<DialogueNodeSO>();
            AssetDatabase.CreateAsset(startNode, startPath);
        }

        var exitNode = AssetDatabase.LoadAssetAtPath<DialogueNodeSO>(exitPath);
        if (exitNode == null)
        {
            exitNode = ScriptableObject.CreateInstance<DialogueNodeSO>();
            AssetDatabase.CreateAsset(exitNode, exitPath);
        }

        var startSerialized = new SerializedObject(startNode);
        startSerialized.FindProperty("nodeId").stringValue = "start";
        startSerialized.FindProperty("speaker").stringValue = "Guide";
        startSerialized.FindProperty("text").stringValue = "Welcome, traveler. What will you do?";
        var startChoices = startSerialized.FindProperty("choices");
        startChoices.arraySize = 2;
        var battleChoice = startChoices.GetArrayElementAtIndex(0);
        battleChoice.FindPropertyRelative("Id").stringValue = "battle";
        battleChoice.FindPropertyRelative("Text").stringValue = "Let's test our might.";
        battleChoice.FindPropertyRelative("Action").enumValueIndex = (int)DialogueChoiceAction.StartBattle;
        battleChoice.FindPropertyRelative("NextNode").objectReferenceValue = null;

        var exitChoice = startChoices.GetArrayElementAtIndex(1);
        exitChoice.FindPropertyRelative("Id").stringValue = "leave";
        exitChoice.FindPropertyRelative("Text").stringValue = "I'll be back later.";
        exitChoice.FindPropertyRelative("Action").enumValueIndex = (int)DialogueChoiceAction.EndDialogue;
        exitChoice.FindPropertyRelative("NextNode").objectReferenceValue = exitNode;
        startSerialized.ApplyModifiedPropertiesWithoutUndo();

        var exitSerialized = new SerializedObject(exitNode);
        exitSerialized.FindProperty("nodeId").stringValue = "exit";
        exitSerialized.FindProperty("speaker").stringValue = "Guide";
        exitSerialized.FindProperty("text").stringValue = "Safe travels.";
        exitSerialized.FindProperty("choices").arraySize = 0;
        exitSerialized.ApplyModifiedPropertiesWithoutUndo();

        var graph = AssetDatabase.LoadAssetAtPath<DialogueGraphSO>(graphPath);
        if (graph == null)
        {
            graph = ScriptableObject.CreateInstance<DialogueGraphSO>();
            AssetDatabase.CreateAsset(graph, graphPath);
        }
        var graphSerialized = new SerializedObject(graph);
        graphSerialized.FindProperty("startNode").objectReferenceValue = startNode;
        var additionalNodes = graphSerialized.FindProperty("additionalNodes");
        additionalNodes.arraySize = 1;
        additionalNodes.GetArrayElementAtIndex(0).objectReferenceValue = exitNode;
        graphSerialized.ApplyModifiedPropertiesWithoutUndo();

        var database = AssetDatabase.LoadAssetAtPath<DialogueDatabaseSO>(dbPath);
        if (database == null)
        {
            database = ScriptableObject.CreateInstance<DialogueDatabaseSO>();
            AssetDatabase.CreateAsset(database, dbPath);
        }
        var dbSerialized = new SerializedObject(database);
        var graphsProp = dbSerialized.FindProperty("graphs");
        graphsProp.arraySize = 1;
        graphsProp.GetArrayElementAtIndex(0).objectReferenceValue = graph;
        dbSerialized.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(startNode);
        EditorUtility.SetDirty(exitNode);
        EditorUtility.SetDirty(graph);
        EditorUtility.SetDirty(database);

        return (database, startNode, exitNode);
    }

    private static ArmyLineupSO EnsureArmyLineup()
    {
        var path = Path.Combine(RootFolder, ArmyLineupAssetName).Replace("\\", "/");
        var lineup = AssetDatabase.LoadAssetAtPath<ArmyLineupSO>(path);
        if (lineup == null)
        {
            lineup = ScriptableObject.CreateInstance<ArmyLineupSO>();
            AssetDatabase.CreateAsset(lineup, path);
        }

        var so = new SerializedObject(lineup);
        var playerUnits = so.FindProperty("playerUnits");
        if (playerUnits.arraySize == 0)
            playerUnits.arraySize = 1;
        playerUnits.GetArrayElementAtIndex(0).FindPropertyRelative("UnitType").enumValueIndex = (int)UnitType.Archer;
        playerUnits.GetArrayElementAtIndex(0).FindPropertyRelative("Amount").intValue = 10;

        var enemyUnits = so.FindProperty("enemyUnits");
        if (enemyUnits.arraySize == 0)
            enemyUnits.arraySize = 1;
        enemyUnits.GetArrayElementAtIndex(0).FindPropertyRelative("UnitType").enumValueIndex = (int)UnitType.Witch;
        enemyUnits.GetArrayElementAtIndex(0).FindPropertyRelative("Amount").intValue = 10;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(lineup);
        return lineup;
    }

    private static GameConfigurationService EnsureGameConfigurationService()
    {
        var path = Path.Combine(RootFolder, GameConfigurationServiceAssetName).Replace("\\", "/");
        var configuration = AssetDatabase.LoadAssetAtPath<GameConfigurationService>(path);
        if (configuration == null)
        {
            configuration = ScriptableObject.CreateInstance<GameConfigurationService>();
            AssetDatabase.CreateAsset(configuration, path);
        }

        var so = new SerializedObject(configuration);
        so.FindProperty("_selectedIndex").intValue = 0;
        so.FindProperty("_team").enumValueIndex = (int)Team.Blue;
        so.FindProperty("_mode").enumValueIndex = (int)GameMode.SinglePlayer;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(configuration);
        return configuration;
    }

    private static GameObject EnsurePlayer(MovementSettingsSO settings, InteractionPromptView promptView)
    {
        const string playerName = "AdventurePlayer";
        var player = GameObject.Find(playerName);
        var wasCreated = false;
        if (player == null)
        {
            player = new GameObject(playerName);
            Undo.RegisterCreatedObjectUndo(player, "Create Adventure Player");
            player.transform.position = Vector3.zero;
            wasCreated = true;
        }

        var characterController = player.GetComponent<CharacterController>();
        if (characterController == null)
        {
            characterController = player.AddComponent<CharacterController>();
            if (!wasCreated)
                Undo.RegisterCreatedObjectUndo(characterController, "Add CharacterController");
        }
        characterController.height = 1.8f;
        characterController.radius = 0.4f;

        var playerInput = player.GetComponent<AdventurePlayerInput>();
        if (playerInput == null)
        {
            playerInput = player.AddComponent<AdventurePlayerInput>();
            Undo.RegisterCreatedObjectUndo(playerInput, "Add AdventurePlayerInput");
        }
        ConfigurePlayerInput(playerInput);

        var movement = player.GetComponent<PlayerMovementController>();
        if (movement == null)
        {
            movement = player.AddComponent<PlayerMovementController>();
            Undo.RegisterCreatedObjectUndo(movement, "Add PlayerMovementController");
        }

        var movementSO = new SerializedObject(movement);
        movementSO.FindProperty("settings").objectReferenceValue = settings;

        var pivotTransform = player.transform.Find("CameraPivot");
        if (pivotTransform == null)
        {
            var cameraPivot = new GameObject("CameraPivot");
            Undo.RegisterCreatedObjectUndo(cameraPivot, "Create Camera Pivot");
            pivotTransform = cameraPivot.transform;
            pivotTransform.SetParent(player.transform);
        }
        pivotTransform.localPosition = new Vector3(0f, 1.6f, 0f);

        var cameraTransform = pivotTransform.Find("PlayerCamera");
        Camera playerCamera;
        if (cameraTransform == null)
        {
            var cameraGO = new GameObject("PlayerCamera");
            Undo.RegisterCreatedObjectUndo(cameraGO, "Create Player Camera");
            cameraTransform = cameraGO.transform;
            cameraTransform.SetParent(pivotTransform);
            cameraTransform.localPosition = Vector3.zero;
            cameraTransform.localRotation = Quaternion.identity;
            playerCamera = cameraGO.AddComponent<Camera>();
        }
        else
        {
            playerCamera = cameraTransform.GetComponent<Camera>();
            if (playerCamera == null)
                playerCamera = cameraTransform.gameObject.AddComponent<Camera>();
        }
        playerCamera.tag = "MainCamera";

        movementSO.FindProperty("cameraPivot").objectReferenceValue = pivotTransform;
        movementSO.FindProperty("legacyInputProvider").objectReferenceValue = playerInput;
        movementSO.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(movement);

        var interaction = player.GetComponent<PlayerInteractionController>();
        if (interaction == null)
        {
            interaction = player.AddComponent<PlayerInteractionController>();
            Undo.RegisterCreatedObjectUndo(interaction, "Add PlayerInteractionController");
        }
        ConfigureInteractionController(interaction, player, promptView, playerCamera, playerInput);

        var router = player.GetComponent<AdventureInputRouter>();
        if (router == null)
        {
            router = player.AddComponent<AdventureInputRouter>();
            Undo.RegisterCreatedObjectUndo(router, "Add AdventureInputRouter");
        }
        ConfigureInputRouter(router, movement, interaction, playerInput);

        return player;
    }

    private static void ConfigurePlayerInput(AdventurePlayerInput playerInput)
    {
        if (playerInput == null)
            return;

        var asset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsAssetPath);
        if (asset == null)
        {
            Debug.LogWarning($"Input actions asset not found at '{InputActionsAssetPath}'. AdventurePlayerInput will remain unconfigured.", playerInput);
            return;
        }

        var so = new SerializedObject(playerInput);
        so.FindProperty("inputActions").objectReferenceValue = asset;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(playerInput);
    }

    private static void ConfigureInteractionController(
        PlayerInteractionController controller,
        GameObject player,
        InteractionPromptView promptView,
        Camera cameraOverride = null,
        AdventurePlayerInput playerInputOverride = null)
    {
        if (controller == null)
            return;

        var playerInput = playerInputOverride ?? player.GetComponent<AdventurePlayerInput>();
        var camera = cameraOverride ?? player.GetComponentInChildren<Camera>();

        var so = new SerializedObject(controller);
        so.FindProperty("playerInput").objectReferenceValue = playerInput;
        so.FindProperty("playerCamera").objectReferenceValue = camera;
        so.FindProperty("promptView").objectReferenceValue = promptView;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(controller);
    }

    private static void ConfigureInputRouter(
        AdventureInputRouter router,
        PlayerMovementController movementController,
        PlayerInteractionController interactionController,
        AdventurePlayerInput playerInput)
    {
        if (router == null)
            return;

        var so = new SerializedObject(router);
        so.FindProperty("playerInput").objectReferenceValue = playerInput;
        so.FindProperty("movementController").objectReferenceValue = movementController;
        so.FindProperty("interactionController").objectReferenceValue = interactionController;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(router);
    }

    private static GameObject EnsureSystemsRoot()
    {
        const string systemsName = "AdventureSystems";
        var existing = GameObject.Find(systemsName);
        if (existing != null)
            return existing;

        var systems = new GameObject(systemsName);
        Undo.RegisterCreatedObjectUndo(systems, "Create Adventure Systems");
        return systems;
    }

    private static InteractionPromptView EnsureInteractionPromptView(GameObject systemsRoot)
    {
        const string canvasName = "InteractionPromptCanvas";
        var canvasTransform = systemsRoot.transform.Find(canvasName);
        GameObject canvasGO;
        if (canvasTransform == null)
        {
            canvasGO = new GameObject(canvasName, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Undo.RegisterCreatedObjectUndo(canvasGO, "Create Interaction Prompt Canvas");
            canvasGO.transform.SetParent(systemsRoot.transform);
            canvasGO.transform.localPosition = Vector3.zero;
            canvasGO.transform.localRotation = Quaternion.identity;
            canvasGO.transform.localScale = Vector3.one;
        }
        else
        {
            canvasGO = canvasTransform.gameObject;
        }

        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        EnsureEventSystem();

        Transform viewTransform = canvasGO.transform.Find("Prompt");
        GameObject viewGO;
        if (viewTransform == null)
        {
            viewGO = new GameObject("Prompt", typeof(RectTransform), typeof(CanvasGroup), typeof(Image), typeof(InteractionPromptView));
            Undo.RegisterCreatedObjectUndo(viewGO, "Create Interaction Prompt View");
            viewGO.transform.SetParent(canvasGO.transform);
        }
        else
        {
            viewGO = viewTransform.gameObject;
            if (viewGO.GetComponent<CanvasGroup>() == null)
                viewGO.AddComponent<CanvasGroup>();
            if (viewGO.GetComponent<Image>() == null)
                viewGO.AddComponent<Image>();
            if (viewGO.GetComponent<InteractionPromptView>() == null)
                viewGO.AddComponent<InteractionPromptView>();
        }

        var rect = viewGO.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.2f);
        rect.anchorMax = new Vector2(0.5f, 0.2f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(320f, 90f);

        var background = viewGO.GetComponent<Image>();
        background.color = new Color(0f, 0f, 0f, 0.65f);

        var canvasGroup = viewGO.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        var bindingText = EnsureText(viewGO.transform, "BindingLabel", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -12f), new Vector2(280f, 32f), 24f, FontStyles.Bold);
        bindingText.alignment = TextAlignmentOptions.Center;

        var actionText = EnsureText(viewGO.transform, "ActionLabel", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 12f), new Vector2(280f, 32f), 20f);
        actionText.alignment = TextAlignmentOptions.Center;

        var view = viewGO.GetComponent<InteractionPromptView>();
        var viewSO = new SerializedObject(view);
        viewSO.FindProperty("root").objectReferenceValue = canvasGroup;
        viewSO.FindProperty("bindingLabel").objectReferenceValue = bindingText;
        viewSO.FindProperty("actionLabel").objectReferenceValue = actionText;
        viewSO.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(view);

        return view;
    }

    private static GridConfigurationGateway EnsureGridConfigurationGateway(GameObject parent, GameConfigurationService configurationService)
    {
        const string gatewayName = "GridConfigurationGateway";
        var child = parent.transform.Find(gatewayName);
        GameObject gatewayGO;
        if (child == null)
        {
            gatewayGO = new GameObject(gatewayName);
            Undo.RegisterCreatedObjectUndo(gatewayGO, "Create Grid Configuration Gateway");
            gatewayGO.transform.SetParent(parent.transform);
        }
        else
        {
            gatewayGO = child.gameObject;
        }

        var gateway = gatewayGO.GetComponent<GridConfigurationGateway>();
        if (gateway == null)
            gateway = gatewayGO.AddComponent<GridConfigurationGateway>();

        var so = new SerializedObject(gateway);
        so.FindProperty("configurationService").objectReferenceValue = configurationService;
        so.FindProperty("battleSceneName").stringValue = "SampleScene";
        so.FindProperty("gridWidth").intValue = 12;
        so.FindProperty("gridHeight").intValue = 8;
        so.FindProperty("playerTeam").enumValueIndex = (int)Team.Blue;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(gateway);
        return gateway;
    }

    private static DialogueUIView EnsureDialogueView(GameObject parent)
    {
        const string canvasName = "AdventureDialogueCanvas";
        var canvasTransform = parent.transform.Find(canvasName);
        GameObject canvasGO;
        if (canvasTransform == null)
        {
            canvasGO = new GameObject(canvasName, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup));
            Undo.RegisterCreatedObjectUndo(canvasGO, "Create Dialogue Canvas");
            canvasGO.transform.SetParent(parent.transform);
        }
        else
        {
            canvasGO = canvasTransform.gameObject;
        }

        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        EnsureEventSystem();

        DialogueUIView view = canvasGO.GetComponent<DialogueUIView>();
        if (view == null)
            view = canvasGO.AddComponent<DialogueUIView>();

        var canvasGroup = canvasGO.GetComponent<CanvasGroup>();

        Transform panelTransform = canvasGO.transform.Find("Panel");
        if (panelTransform == null)
        {
            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            Undo.RegisterCreatedObjectUndo(panel, "Create Dialogue Panel");
            panelTransform = panel.transform;
            panelTransform.SetParent(canvasGO.transform);
            var panelRect = panelTransform.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0f);
            panelRect.anchorMax = new Vector2(0.5f, 0f);
            panelRect.pivot = new Vector2(0.5f, 0f);
            panelRect.sizeDelta = new Vector2(600f, 250f);
            panelRect.anchoredPosition = new Vector2(0f, 200f);
            panelTransform.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.75f);
        }

        var speakerText = EnsureText(panelTransform, "SpeakerText", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -20f), new Vector2(560f, 40f), 24f, FontStyles.Bold);
        var bodyText = EnsureText(panelTransform, "BodyText", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -80f), new Vector2(560f, 100f), 20f);

        Transform choicesRoot = panelTransform.Find("Choices");
        if (choicesRoot == null)
        {
            var choices = new GameObject("Choices", typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(choices, "Create Dialogue Choices Root");
            choicesRoot = choices.transform;
            choicesRoot.SetParent(panelTransform);
            var rect = choicesRoot.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 20f);
            rect.sizeDelta = new Vector2(560f, 80f);
        }

        var buttonPrefab = panelTransform.Find("ChoiceButton");
        Button buttonComponent;
        if (buttonPrefab == null)
        {
            var buttonGO = new GameObject("ChoiceButton", typeof(RectTransform), typeof(Image), typeof(Button));
            Undo.RegisterCreatedObjectUndo(buttonGO, "Create Dialogue Choice Button");
            buttonPrefab = buttonGO.transform;
            buttonPrefab.SetParent(panelTransform);
            var rect = buttonPrefab.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(540f, 40f);
            rect.anchoredPosition = new Vector2(0f, -20f);
            buttonComponent = buttonPrefab.GetComponent<Button>();
            buttonComponent.targetGraphic = buttonPrefab.GetComponent<Image>();
            buttonPrefab.gameObject.SetActive(false);

            var label = EnsureText(buttonPrefab, "Label", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(520f, 30f), 20f);
            label.alignment = TextAlignmentOptions.Center;
        }
        else
        {
            buttonComponent = buttonPrefab.GetComponent<Button>();
        }

        var viewSO = new SerializedObject(view);
        viewSO.FindProperty("canvasGroup").objectReferenceValue = canvasGroup;
        viewSO.FindProperty("speakerText").objectReferenceValue = speakerText;
        viewSO.FindProperty("bodyText").objectReferenceValue = bodyText;
        viewSO.FindProperty("choicesRoot").objectReferenceValue = choicesRoot;
        viewSO.FindProperty("choiceButtonPrefab").objectReferenceValue = buttonComponent;
        viewSO.ApplyModifiedPropertiesWithoutUndo();

        return view;
    }

    private static TextMeshProUGUI EnsureText(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Vector2 size, float fontSize, FontStyles style = FontStyles.Normal)
    {
        var child = parent.Find(name);
        TextMeshProUGUI text;
        if (child == null)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            Undo.RegisterCreatedObjectUndo(go, $"Create {name}");
            child = go.transform;
            child.SetParent(parent);
        }
        text = child.GetComponent<TextMeshProUGUI>();
        var rect = child.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = size;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.text = string.Empty;
        text.enableWordWrapping = true;
        return text;
    }

    private static void EnsureEventSystem()
    {
        if (UnityEngine.Object.FindObjectOfType<EventSystem>() != null)
            return;
        var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        Undo.RegisterCreatedObjectUndo(eventSystem, "Create EventSystem");
    }

    private static AdventureDialogueOrchestrator EnsureDialogueOrchestrator(GameObject systemsRoot, DialogueUIView view, ArmyLineupSO defaultLineup)
    {
        const string orchestratorName = "AdventureDialogueOrchestrator";
        var existing = systemsRoot.transform.Find(orchestratorName);
        GameObject go;
        if (existing == null)
        {
            go = new GameObject(orchestratorName);
            Undo.RegisterCreatedObjectUndo(go, "Create Adventure Dialogue Orchestrator");
            go.transform.SetParent(systemsRoot.transform);
        }
        else
        {
            go = existing.gameObject;
        }

        var orchestrator = go.GetComponent<AdventureDialogueOrchestrator>();
        if (orchestrator == null)
            orchestrator = go.AddComponent<AdventureDialogueOrchestrator>();

        var so = new SerializedObject(orchestrator);
        so.FindProperty("view").objectReferenceValue = view;
        so.FindProperty("defaultLineup").objectReferenceValue = defaultLineup;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(orchestrator);
        return orchestrator;
    }
    private static void BindInstaller(
        AdventureGameplayInstaller installer,
        PlayerMovementController movementController,
        MushroomCatalogSO catalog,
        DialogueDatabaseSO dialogueDatabase,
        GridConfigurationGateway gateway,
        ArmyLineupSO lineup,
        PlayerInteractionController interactionController,
        AdventureDialogueOrchestrator orchestrator)
    {
        var so = new SerializedObject(installer);
        so.FindProperty("playerMovementController").objectReferenceValue = movementController;
        so.FindProperty("mushroomCatalog").objectReferenceValue = catalog;
        so.FindProperty("dialogueDatabase").objectReferenceValue = dialogueDatabase;
        so.FindProperty("gridConfigurationGateway").objectReferenceValue = gateway;
        so.FindProperty("defaultLineup").objectReferenceValue = lineup;
        so.FindProperty("interactionController").objectReferenceValue = interactionController;
        so.FindProperty("dialogueOrchestrator").objectReferenceValue = orchestrator;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(installer);
    }
}
