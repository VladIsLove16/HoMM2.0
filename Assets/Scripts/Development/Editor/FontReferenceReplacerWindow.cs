using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class FontReferenceReplacerWindow : EditorWindow
{
    private TMP_FontAsset _oldTmpFont;
    private TMP_FontAsset _newTmpFont;
    private Font _oldLegacyFont;
    private Font _newLegacyFont;
    private bool _processPrefabs = true;
    private bool _processScenes = true;
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
        _processPrefabs = EditorGUILayout.ToggleLeft("Process prefabs in Assets", _processPrefabs);
        _processScenes = EditorGUILayout.ToggleLeft("Process scenes in Assets", _processScenes);
        _updateTmpSettings = EditorGUILayout.ToggleLeft("Update TMP Settings default font", _updateTmpSettings);

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
                "This will modify scenes and prefabs under Assets. Make sure your project is saved or committed first.",
                "Replace",
                "Cancel"))
        {
            return;
        }

        var replacedTmpCount = 0;
        var replacedLegacyCount = 0;
        var changedAssetCount = 0;

        if (_processPrefabs)
            ProcessPrefabs(ref replacedTmpCount, ref replacedLegacyCount, ref changedAssetCount);

        if (_processScenes)
            ProcessScenes(ref replacedTmpCount, ref replacedLegacyCount, ref changedAssetCount);

        if (_updateTmpSettings)
            ProcessTmpSettings(ref replacedTmpCount, ref changedAssetCount);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Replace Font References",
            $"Changed assets: {changedAssetCount}\nTMP references: {replacedTmpCount}\nLegacy UI references: {replacedLegacyCount}",
            "OK");
    }

    private void ProcessPrefabs(ref int replacedTmpCount, ref int replacedLegacyCount, ref int changedAssetCount)
    {
        var prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
        for (var index = 0; index < prefabGuids.Length; index++)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(prefabGuids[index]);
            EditorUtility.DisplayProgressBar("Replacing Fonts", $"Prefab: {assetPath}", (index + 1f) / prefabGuids.Length);

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

    private void ProcessScenes(ref int replacedTmpCount, ref int replacedLegacyCount, ref int changedAssetCount)
    {
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

    private void ProcessTmpSettings(ref int replacedTmpCount, ref int changedAssetCount)
    {
        if (_oldTmpFont == null || _newTmpFont == null)
            return;

        var tmpSettings = TMP_Settings.instance;
        if (tmpSettings == null || tmpSettings.defaultFontAsset != _oldTmpFont)
            return;

        tmpSettings.defaultFontAsset = _newTmpFont;
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
