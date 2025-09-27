using UnityEngine;

public static class PlayModeTestSetup
{
    public static SceneTransitionDataService EnsureSceneTransitionDataService()
    {
        if (SceneTransitionDataService.Instance != null)
            return SceneTransitionDataService.Instance;

        var go = new GameObject("SceneTransitionDataService_Test");
        var svc = go.AddComponent<SceneTransitionDataService>();
        return svc;
    }
}
