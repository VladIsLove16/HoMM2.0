using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public static partial class Loader
{
    private class LoadingMonoBehaviour : MonoBehaviour { }

    private static AsyncOperation loadingAsyncOperation;
    private static Func<IEnumerator> _customLoadRoutineFactory;
    private static Action onLoaderCallback;

    public static void Load(Scene scene, Func<IEnumerator> loadRoutineOverride = null)
    {
        _customLoadRoutineFactory = loadRoutineOverride;
        onLoaderCallback = () =>
        {
            GameObject loadingGO = new GameObject("loadingGO");
            var loaderBehaviour = loadingGO.AddComponent<LoadingMonoBehaviour>();
            if (_customLoadRoutineFactory != null)
            {
                loaderBehaviour.StartCoroutine(RunCustomLoadRoutine(scene));
            }
            else
            {
                loaderBehaviour.StartCoroutine(LoadSceneAsync(scene));
            }
        };
        SceneManager.LoadScene(Scene.Loading.ToString());
    }

    private static IEnumerator RunCustomLoadRoutine(Scene scene)
    {
        yield return null;

        if (_customLoadRoutineFactory != null)
        {
            var routine = _customLoadRoutineFactory.Invoke();
            if (routine != null)
            {
                yield return routine;
            }
            loadingAsyncOperation = null;
            _customLoadRoutineFactory = null;
        }
        else
        {
            yield return LoadSceneAsync(scene);
        }
    }

    private static IEnumerator LoadSceneAsync(Scene scene)
    {
        yield return null;
        loadingAsyncOperation = SceneManager.LoadSceneAsync(scene.ToString());
        while (!loadingAsyncOperation.isDone)
        {
            yield return null;
        }
        _customLoadRoutineFactory = null;
    }

    public static float GetLoadingProgress()
    {
        if (loadingAsyncOperation == null) return 1f;
        return loadingAsyncOperation.progress;
    }

    public static void LoaderCallback()
    {
        if (onLoaderCallback != null)
        {
            onLoaderCallback();
            onLoaderCallback = null;
        }
    }
}
