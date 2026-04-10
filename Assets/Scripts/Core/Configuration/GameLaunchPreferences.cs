using UnityEngine;

public static class GameLaunchPreferences
{
    private const string MultiplayerStartScenePrefsKey = "Game.MultiplayerStartScene";
    private const string DirectGridFightArmyPresetPrefsKey = "Game.DirectGridFightArmyPresetIndex";
    private const string AdventureFlowValue = "adventure";
    private const string GridFightFlowValue = "gridfight";

    public static SceneLoader.Scene GetMultiplayerStartScene()
    {
        var storedValue = PlayerPrefs.GetString(MultiplayerStartScenePrefsKey, AdventureFlowValue);
        return ParseSessionFlowValue(storedValue);
    }

    public static void SetMultiplayerStartScene(SceneLoader.Scene scene)
    {
        PlayerPrefs.SetString(MultiplayerStartScenePrefsKey, ToSessionFlowValue(scene));
        PlayerPrefs.Save();
    }

    public static int GetDirectGridFightArmyPresetIndex()
    {
        return Mathf.Max(0, PlayerPrefs.GetInt(DirectGridFightArmyPresetPrefsKey, 0));
    }

    public static void SetDirectGridFightArmyPresetIndex(int index)
    {
        PlayerPrefs.SetInt(DirectGridFightArmyPresetPrefsKey, Mathf.Max(0, index));
        PlayerPrefs.Save();
    }

    public static string ToSessionFlowValue(SceneLoader.Scene scene)
    {
        return scene == SceneLoader.Scene.GridFight
            ? GridFightFlowValue
            : AdventureFlowValue;
    }

    public static SceneLoader.Scene ParseSessionFlowValue(string value)
    {
        return string.Equals(value, GridFightFlowValue, System.StringComparison.OrdinalIgnoreCase)
            ? SceneLoader.Scene.GridFight
            : SceneLoader.Scene.Adventure;
    }
}
