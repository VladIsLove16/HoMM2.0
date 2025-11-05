using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
// NetworkPlayModeManager.cs
public class NetworkPlayModeManager : MonoBehaviour
{
    [SerializeField] private PlayModeToolsConfig _config;
    [SerializeField] private bool _isServerInstance = false;
    [SerializeField] private bool _isClientInstance = false;

    private NetworkManager _networkManager;

    void Start()
    {
        _networkManager = NetworkManager.Singleton;

        if (_config == null)
        {
            Debug.LogError("[NetworkPlayModeManager] Config not assigned!");
            return;
        }

        if (!_config.enableNetworkTesting)
        {
            Debug.Log("[NetworkPlayModeManager] Network testing disabled");
            return;
        }

        StartCoroutine(InitializeNetwork());
    }

    private IEnumerator InitializeNetwork()
    {
        // ∆дем инициализации NetworkManager
        yield return new WaitUntil(() => _networkManager != null);

        // ќпредел€ем роль на основе имени сцены или других параметров
        var sceneName = SceneManager.GetActiveScene().name;

        if (_isServerInstance || sceneName.Contains("Server"))
        {
            yield return StartCoroutine(StartAsServer());
        }
        else if (_isClientInstance || sceneName.Contains("Client"))
        {
            yield return StartCoroutine(StartAsClient());
        }
        else
        {
            Debug.LogWarning("[NetworkPlayModeManager] No role specified, running in local mode");
        }
    }

    private IEnumerator StartAsServer()
    {
        Debug.Log("[NetworkPlayModeManager] Starting as SERVER");

        if (!_networkManager.IsServer && !_networkManager.IsHost)
        {
            _networkManager.StartHost();
        }

        yield return new WaitUntil(() => _networkManager.IsServer);
        Debug.Log("[NetworkPlayModeManager] Server started successfully");
    }

    private IEnumerator StartAsClient()
    {
        Debug.Log("[NetworkPlayModeManager] Starting as CLIENT");

        if (!_networkManager.IsClient)
        {
            _networkManager.StartClient();
        }

        yield return new WaitUntil(() => _networkManager.IsClient);
        Debug.Log("[NetworkPlayModeManager] Client started successfully");
    }
}