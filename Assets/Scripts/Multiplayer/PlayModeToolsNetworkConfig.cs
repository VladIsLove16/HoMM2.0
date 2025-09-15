//using Unity.Netcode;
//using UnityEditor;
//using UnityEngine;

//[System.Serializable]
//public class PlayModeToolsNetworkConfig : PlayModeToolsConfig
//{
//    [Header("Network Instance Settings")]
//    public bool instance1IsServer = true;
//    public bool instance2IsClient = true;

//    [Header("Connection Settings")]
//    public string serverIP = "127.0.0.1";
//    public ushort serverPort = 7777;

//    public override void OnPlayModeStateChanged(PlayModeStateChange state)
//    {
//        base.OnPlayModeStateChanged(state);

//        if (state == PlayModeStateChange.EnteredPlayMode)
//        {
//            SetupNetworkInstance();
//        }
//    }

//    private void SetupNetworkInstance()
//    {
//        var instanceId = PlayModeTools.GetCurrentInstanceId();

//        if (instanceId == 1 && instance1IsServer)
//        {
//            StartServerInstance();
//        }
//        else if (instanceId == 2 && instance2IsClient)
//        {
//            StartClientInstance();
//        }
//    }

//    private void StartServerInstance()
//    {
//        Debug.Log($"[PlayModeTools] Instance {PlayModeTools.GetCurrentInstanceId()} starting as SERVER");

//        var networkManager = NetworkManager.Singleton;
//        if (networkManager != null)
//        {
//            networkManager.StartHost();
//        }
//    }

//    private void StartClientInstance()
//    {
//        Debug.Log($"[PlayModeTools] Instance {PlayModeTools.GetCurrentInstanceId()} starting as CLIENT");

//        var networkManager = NetworkManager.Singleton;
//        if (networkManager != null)
//        {
//            networkManager.StartClient();
//        }
//    }
//}
