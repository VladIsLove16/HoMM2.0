using System.IO;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine;
using Unity.Netcode;

public static class PrefabCommandExecutorFixer
{
    private const string UnitsRoot = "Assets/Prefabs/Units";

    [MenuItem("Tools/Units/Ensure Command Executors On Prefabs")] 
    public static void EnsureCommandExecutors()
    {
        var guids = AssetDatabase.FindAssets("t:Prefab", new[] { UnitsRoot });
        int processed = 0, modified = 0;
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            // Open prefab in isolation to edit safely
            var stage = PrefabStageUtility.GetCurrentPrefabStage();
            bool closeAfter = false;
            if (stage == null || stage.assetPath != path)
            {
                PrefabStageUtility.OpenPrefab(path);
                closeAfter = true;
            }

            var openedStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (openedStage == null)
                continue;

            var root = openedStage.prefabContentsRoot;
            bool isNetworkVariant = root.GetComponent<NetworkObject>() != null;
            bool changed = false;

            // Ensure UnitView3D exists (optional, skip if absent)
            var view = root.GetComponent<UnitView3D>();
            if (view == null)
            {
                // Not forcing add, but you can enable next line if required
                // view = root.AddComponent<UnitView3D>(); changed = true;
            }

            if (isNetworkVariant)
            {
                // Ensure UnitNetworkController
                if (root.GetComponent<UnitNetworkController>() == null)
                {
                    root.AddComponent<UnitNetworkController>();
                    changed = true;
                }
                // Ensure NetworkTransform (useful for transform sync)
                if (root.GetComponent<Unity.Netcode.Components.NetworkTransform>() == null)
                {
                    root.AddComponent<Unity.Netcode.Components.NetworkTransform>();
                    changed = true;
                }
            }
            else
            {
                // Ensure LocalUnitCommandExecutor on local variants
                if (root.GetComponent<LocalUnitCommandExecutor>() == null)
                {
                    root.AddComponent<LocalUnitCommandExecutor>();
                    changed = true;
                }
            }

            if (changed)
            {
                EditorSceneManager.MarkSceneDirty(openedStage.scene);
                PrefabUtility.SaveAsPrefabAsset(root, path);
                modified++;
                Debug.Log($"[PrefabCommandExecutorFixer] Updated: {path} (Network: {isNetworkVariant})");
            }

            if (closeAfter)
            {
                // Close stage without saving (we already saved prefab)
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            }

            processed++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[PrefabCommandExecutorFixer] Processed: {processed}, Modified: {modified}");
    }
}


