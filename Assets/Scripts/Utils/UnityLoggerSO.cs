using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "UnityLoggerSO", menuName = "ScriptableObjects/UnityLoggerSO", order = 1)]
public class UnityLoggerSO : ScriptableObject
{
    private const string AssetPath = "Assets/ScriptableObjects/GridGame/UnityLoggerSO.asset";

    private static UnityLoggerSO _instance;

    public static UnityLoggerSO Instance
    {
        get
        {
            if (_instance != null)
                return _instance;

#if UNITY_EDITOR
            // Editor-only: пробуем загрузить или создать asset через AssetDatabase
            _instance = AssetDatabase.LoadAssetAtPath<UnityLoggerSO>(AssetPath);

            if (_instance == null)
            {
                Debug.LogWarning("UnityLoggerSO asset not found at " + AssetPath + ". Creating a new one (editor only).");
                _instance = CreateInstance<UnityLoggerSO>();
                AssetDatabase.CreateAsset(_instance, AssetPath);
                AssetDatabase.SaveAssets();
            }

            _instance.EnsureEnumSynced();
#else
            // Runtime: пытаемся найти уже загруженный экземпляр (например, привязанный к сцене)
            _instance = FindLoadedInstance();
            if (_instance == null)
            {
                Debug.LogWarning("UnityLoggerSO instance not found at runtime. Logging categories will use defaults.");
                _instance = CreateInstance<UnityLoggerSO>();
            }
#endif
            return _instance;
        }
    }

    public List<LogCategoryEntry> Categories = new List<LogCategoryEntry>();

    /// Синхронизация списка категорий с enum
    public void EnsureEnumSynced()
    {
        bool changed = false;

        Array enumValues = Enum.GetValues(typeof(LogCategory));

        foreach (LogCategory val in enumValues)
        {
            if (!Categories.Exists(x => x.Category == val))
            {
                Categories.Add(new LogCategoryEntry
                {
                    Category = val,
                    Enabled = false
                });
                changed = true;
            }
        }

        if (changed)
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
#endif
        }
    }

    /// Проверка, включена ли категория
    public bool IsEnabled(LogCategory category)
    {
        var entry = Categories.Find(x => x.Category == category);
        return entry != null && entry.Enabled;
    }

    /// Включить/выключить
    public void SetEnabled(LogCategory category, bool enabled)
    {
        var entry = Categories.Find(x => x.Category == category);
        if (entry != null)
            entry.Enabled = enabled;

#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
#endif
    }

    private static UnityLoggerSO FindLoadedInstance()
    {
        var instances = Resources.FindObjectsOfTypeAll<UnityLoggerSO>();
        return instances != null && instances.Length > 0 ? instances[0] : null;
    }
}

[Serializable]
public class LogCategoryEntry
{
    public LogCategory Category;
    public bool Enabled = true;
}

public enum LogCategory
{
    Gameplay,
    UI,
    Network,
    AI,
    Audio,
    Unit,
    ActionResolver
}

