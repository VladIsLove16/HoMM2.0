using UnityEditor;
using UnityEngine;
using Zenject;

public static class UnityLogger
{
    public static void Log(string message)
    {
        Debug.Log(message);
    }
    public static void Log(string message, LogCategory @class)
    {
#if UNITY_EDITOR
        if (UnityLoggerSO.Instance.IsEnabled(@class))
            Debug.Log(message);
        else
            return;
#endif
    }
}
