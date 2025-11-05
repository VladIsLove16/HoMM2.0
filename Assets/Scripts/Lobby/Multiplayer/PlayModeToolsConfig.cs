using UnityEngine;
// PlayModeToolsConfig.cs
[CreateAssetMenu(fileName = "PlayModeToolsConfig", menuName = "Game/PlayMode Tools Config")]
public class PlayModeToolsConfig : ScriptableObject
{
    [Header("Network Settings")]
    public bool enableNetworkTesting = true;
    public string serverSceneName = "GameScene";
    public string clientSceneName = "GameScene";

    [Header("Auto Start Settings")]
    public bool autoStartServer = false;
    public bool autoStartClient = false;
    public float startDelay = 1f;
}
