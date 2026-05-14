using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class FontReferenceReplacerWindow : EditorWindow
{
    private enum ReplacementScope
    {
        AllPrefabs,
        SceneUsedPrefabs,
        SceneObjectsOnly
    }

    private enum SceneScope
    {
        CurrentScene,
        AllScenes
    }

    private TMP_FontAsset _oldTmpFont;
    private TMP_FontAsset _newTmpFont;
    private Font _oldLegacyFont;
    private Font _newLegacyFont;
    private ReplacementScope _replacementScope = ReplacementScope.AllPrefabs;
    private SceneScope _sceneScope = SceneScope.CurrentScene;
    private bool _updateTmpSettings = true;

    [MenuItem("Tools/UI/Replace Font References")]
    private static void OpenWindow()
    {
        GetWindow<FontReferenceReplacerWindow>("Replace Fonts");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("TMP Fonts", EditorStyles.boldLabel);
        _oldTmpFont = (TMP_FontAsset)EditorGUILayout.ObjectField("Current TMP Font", _oldTmpFont, typeof(TMP_FontAsset), false);
        _newTmpFont = (TMP_FontAsset)EditorGUILayout.ObjectField("New TMP Font", _newTmpFont, typeof(TMP_FontAsset), false);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Legacy UI Fonts", EditorStyles.boldLabel);
        _oldLegacyFont = (Font)EditorGUILayout.ObjectField("Current UI Font", _oldLegacyFont, typeof(Font), false);
        _newLegacyFont = (Font)EditorGUILayout.ObjectField("New UI Font", _newLegacyFont, typeof(Font), false);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Scope", EditorStyles.boldLabel);
        _replacementScope = (ReplacementScope)EditorGUILayout.EnumPopup("Replacement Mode", _replacementScope);
        if (_replacementScope != ReplacementScope.AllPrefabs)
            _sceneScope = (SceneScope)EditorGUILayout.EnumPopup("Scene Scope", _sceneScope);
        _updateTmpSettings = EditorGUILayout.ToggleLeft("Update TMP Settings default font", _updateTmpSettings);

        EditorGUILayout.HelpBox(GetScopeDescription(), MessageType.None);

        EditorGUILayout.Space();

        using (new EditorGUI.DisabledScope(!CanReplace()))
        {
            if (GUILayout.Button("Replace Font References", GUILayout.Height(32f)))
                ReplaceFontReferences();
        }

        if (!CanReplace())
        {
            EditorGUILayout.HelpBox(
                "Assign at least one valid old/new font pair. TMP and legacy UI fonts can be replaced independently.",
                MessageType.Info);
        }
    }

    private bool CanReplace()
    {
        var canReplaceTmp = _oldTmpFont != null && _newTmpFont != null && _oldTmpFont != _newTmpFont;
        var canReplaceLegacy = _oldLegacyFont != null && _newLegacyFont != null && _oldLegacyFont != _newLegacyFont;
        return canReplaceTmp || canReplaceLegacy;
    }

    private void ReplaceFontReferences()
    {
        if (!EditorUtility.DisplayDialog(
                "Replace Font References",
                $"Mode: {GetScopeTitle()}\n{GetSceneScopeSummary()}\nThis will modify project assets. Make sure your project is saved or committed first.",
                "Replace",
                "Cancel"))
        {
            return;
        }

        var replacedTmpCount = 0;
        var replacedLegacyCount = 0;
        var changedAssetCount = 0;
        try
        {
            switch (_replacementScope)
            {
                case ReplacementScope.AllPrefabs:
                    ProcessAllPrefabs(ref replacedTmpCount, ref replacedLegacyCount, ref changedAssetCount);
                    break;
                case ReplacementScope.SceneUsedPrefabs:
                    ProcessSceneUsedPrefabs(ref replacedTmpCount, ref replacedLegacyCount, ref changedAssetCount);
                    break;
                case ReplacementScope.SceneObjectsOnly:
                    ProcessSceneObjects(ref replacedTmpCount, ref replacedLegacyCount, ref changedAssetCount);
                    break;
            }

            if (_updateTmpSettings)
                ProcessTmpSettings(ref replacedTmpCount, ref changedAssetCount);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Replace Font References",
                $"Changed assets: {changedAssetCount}\nTMP references: {replacedTmpCount}\nLegacy UI references: {replacedLegacyCount}",
                "OK");
        }
        catch (System.Exception exception)
        {
            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("Replace Font References", exception.Message, "OK");
        }
    }

    private string GetScopeTitle()
    {
        return _replacementScope switch
        {
            ReplacementScope.AllPrefabs => "All Prefab Assets",
            ReplacementScope.SceneUsedPrefabs => "Prefab Assets Used In Scene Scope",
            ReplacementScope.SceneObjectsOnly => "Scene Objects Only",
            _ => _replacementScope.ToString()
        };
    }

    private string GetSceneScopeSummary()
    {
        if (_replacementScope == ReplacementScope.AllPrefabs)
            return "Scene scope: not used";

        return _sceneScope switch
        {
            SceneScope.CurrentScene => "Scene scope: current open scene",
            SceneScope.AllScenes => "Scene scope: all scenes under Assets",
            _ => string.Empty
        };
    }

    private string GetScopeDescription()
    {
        return _replacementScope switch
        {
            ReplacementScope.AllPrefabs =>
                "Replaces font references in every prefab asset under Assets. Scene-only objects are not touched.",
            ReplacementScope.SceneUsedPrefabs =>
                "Scans the selected scene scope, finds prefab instances placed there, and updates only those prefab assets. Scene-only objects are not touched.",
            ReplacementScope.SceneObjectsOnly =>
                "Replaces font references directly in scene objects in the selected scene scope. Prefab assets remain unchanged; prefab instances on scenes become scene overrides if needed.",
            _ => string.Empty
        };
    }

    private void ProcessAllPrefabs(ref int replacedTmpCount, ref int replacedLegacyCount, ref int changedAssetCount)
    {
        var prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
        var prefabPaths = new List<string>(prefabGuids.Length);
        foreach (var prefabGuid in prefabGuids)
            prefabPaths.Add(AssetDatabase.GUIDToAssetPath(prefabGuid));

        ProcessPrefabAssets(prefabPaths, ref replacedTmpCount, ref replacedLegacyCount, ref changedAssetCount);
    }

    private void ProcessSceneUsedPrefabs(ref int replacedTmpCount, ref int replacedLegacyCount, ref int changedAssetCount)
    {
        var prefabPaths = CollectPrefabPathsUsedInScenes();
        ProcessPrefabAssets(prefabPaths, ref replacedTmpCount, ref replacedLegacyCount, ref changedAssetCount);
    }

    private void ProcessPrefabAssets(
        IReadOnlyList<string> prefabPaths,
        ref int replacedTmpCount,
        ref int replacedLegacyCount,
        ref int changedAssetCount)
    {
        for (var index = 0; index < prefabPaths.Count; index++)
        {
            var assetPath = prefabPaths[index];
            EditorUtility.DisplayProgressBar(
                "Replacing Fonts",
                $"Prefab: {assetPath}",
                prefabPaths.Count == 0 ? 1f : (index + 1f) / prefabPaths.Count);

            var root = PrefabUtility.LoadPrefabContents(assetPath);
            var changed = ReplaceFontsInHierarchy(root, ref replacedTmpCount, ref replacedLegacyCount);

            if (changed)
            {
                PrefabUtility.SaveAsPrefabAsset(root, assetPath);
                changedAssetCount++;
            }

            PrefabUtility.UnloadPrefabContents(root);
        }

        EditorUtility.ClearProgressBar();
    }

    private void ProcessSceneObjects(ref int replacedTmpCount, ref int replacedLegacyCount, ref int changedAssetCount)
    {
        if (_sceneScope == SceneScope.CurrentScene)
        {
            ProcessCurrentSceneObjects(ref replacedTmpCount, ref replacedLegacyCount, ref changedAssetCount);
            return;
        }

        var sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });
        var originalSceneSetup = EditorSceneManager.GetSceneManagerSetup();

        try
        {
            for (var index = 0; index < sceneGuids.Length; index++)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(sceneGuids[index]);
                EditorUtility.DisplayProgressBar("Replacing Fonts", $"Scene: {assetPath}", (index + 1f) / sceneGuids.Length);

                var scene = EditorSceneManager.OpenScene(assetPath, OpenSceneMode.Single);
                var changed = false;

                foreach (var root in scene.GetRootGameObjects())
                    changed |= ReplaceFontsInHierarchy(root, ref replacedTmpCount, ref replacedLegacyCount);

                if (changed)
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                    changedAssetCount++;
                }
            }
        }
        finally
        {
            EditorSceneManager.RestoreSceneManagerSetup(originalSceneSetup);
            EditorUtility.ClearProgressBar();
        }
    }

    private List<string> CollectPrefabPathsUsedInScenes()
    {
        if (_sceneScope == SceneScope.CurrentScene)
        {
            var currentScene = SceneManager.GetActiveScene();
            EnsureSceneCanBeProcessed(currentScene);

            var currentScenePrefabPaths = new HashSet<string>();
            foreach (var root in currentScene.GetRootGameObjects())
                CollectPrefabPaths(root, currentScenePrefabPaths);

            return new List<string>(currentScenePrefabPaths);
        }

        var sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });
        var originalSceneSetup = EditorSceneManager.GetSceneManagerSetup();
        var prefabPaths = new HashSet<string>();

        try
        {
            for (var index = 0; index < sceneGuids.Length; index++)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(sceneGuids[index]);
                EditorUtility.DisplayProgressBar(
                    "Replacing Fonts",
                    $"Scanning scene prefabs: {assetPath}",
                    (index + 1f) / sceneGuids.Length);

                var scene = EditorSceneManager.OpenScene(assetPath, OpenSceneMode.Single);
                foreach (var root in scene.GetRootGameObjects())
                    CollectPrefabPaths(root, prefabPaths);
            }
        }
        finally
        {
            EditorSceneManager.RestoreSceneManagerSetup(originalSceneSetup);
            EditorUtility.ClearProgressBar();
        }

        return new List<string>(prefabPaths);
    }

    private void ProcessCurrentSceneObjects(ref int replacedTmpCount, ref int replacedLegacyCount, ref int changedAssetCount)
    {
        var currentScene = SceneManager.GetActiveScene();
        EnsureSceneCanBeProcessed(currentScene);

        var changed = false;
        foreach (var root in currentScene.GetRootGameObjects())
            changed |= ReplaceFontsInHierarchy(root, ref replacedTmpCount, ref replacedLegacyCount);

        if (!changed)
            return;

        EditorSceneManager.MarkSceneDirty(currentScene);
        EditorSceneManager.SaveScene(currentScene);
        changedAssetCount++;
    }

    private static void EnsureSceneCanBeProcessed(Scene scene)
    {
        if (!scene.IsValid() || !scene.isLoaded)
            throw new System.InvalidOperationException("Current scene is not loaded.");

        if (!string.IsNullOrEmpty(scene.path))
            return;

        throw new System.InvalidOperationException("Current scene must be saved before replacing font references.");
    }

    private static void CollectPrefabPaths(GameObject root, HashSet<string> prefabPaths)
    {
        foreach (var transform in root.GetComponentsInChildren<Transform>(true))
        {
            var gameObject = transform.gameObject;
            if (!PrefabUtility.IsPartOfPrefabInstance(gameObject))
                continue;

            var assetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(gameObject);
            if (!string.IsNullOrEmpty(assetPath))
                prefabPaths.Add(assetPath);
        }
    }

    private void ProcessTmpSettings(ref int replacedTmpCount, ref int changedAssetCount)
    {
        if (_oldTmpFont == null || _newTmpFont == null)
            return;

        var tmpSettings = TMP_Settings.instance;
        if (tmpSettings == null || TMP_Settings.defaultFontAsset != _oldTmpFont)
            return;

        TMP_Settings.defaultFontAsset = _newTmpFont;
        EditorUtility.SetDirty(tmpSettings);
        replacedTmpCount++;
        changedAssetCount++;
    }

    private bool ReplaceFontsInHierarchy(GameObject root, ref int replacedTmpCount, ref int replacedLegacyCount)
    {
        var changed = false;

        if (_oldTmpFont != null && _newTmpFont != null)
        {
            foreach (var tmpText in root.GetComponentsInChildren<TMP_Text>(true))
            {
                if (tmpText.font != _oldTmpFont)
                    continue;

                Undo.RecordObject(tmpText, "Replace TMP Font");
                tmpText.font = _newTmpFont;
                EditorUtility.SetDirty(tmpText);
                replacedTmpCount++;
                changed = true;
            }

            foreach (var tmpInput in root.GetComponentsInChildren<TMP_InputField>(true))
            {
                changed |= ReplaceTextComponentFont(tmpInput.textViewport, ref replacedTmpCount);
                changed |= ReplaceTextComponentFont(tmpInput.textComponent, ref replacedTmpCount);
                changed |= ReplaceTextComponentFont(tmpInput.placeholder as TMP_Text, ref replacedTmpCount);
            }
        }

        if (_oldLegacyFont != null && _newLegacyFont != null)
        {
            foreach (var uiText in root.GetComponentsInChildren<Text>(true))
            {
                if (uiText.font != _oldLegacyFont)
                    continue;

                Undo.RecordObject(uiText, "Replace UI Font");
                uiText.font = _newLegacyFont;
                EditorUtility.SetDirty(uiText);
                replacedLegacyCount++;
                changed = true;
            }
        }

        return changed;
    }

    private bool ReplaceTextComponentFont(Object candidate, ref int replacedTmpCount)
    {
        if (candidate is not TMP_Text tmpText || tmpText.font != _oldTmpFont)
            return false;

        Undo.RecordObject(tmpText, "Replace TMP Font");
        tmpText.font = _newTmpFont;
        EditorUtility.SetDirty(tmpText);
        replacedTmpCount++;
        return true;
    }
}
