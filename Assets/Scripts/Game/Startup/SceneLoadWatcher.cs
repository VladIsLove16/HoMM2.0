using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadWatcher : MonoBehaviour
{
    public event Action OnSceneReady;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Ждём 1 кадр после загрузки
        StartCoroutine(NotifySceneReady(scene));
    }

    private IEnumerator NotifySceneReady(Scene scene)
    {
        yield return null; // дождаться вызова всех Start()
        Debug.Log($"[SceneLoadWatcher] Scene ready: {scene.name}");
        OnSceneReady?.Invoke();
    }
}
