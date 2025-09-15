using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

/// <summary>
/// Менеджер переходов между сценами в мультиплеере
/// </summary>
public class SceneTransitionManager : NetworkBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string _lobbySceneName = "LobbyScene";
    [SerializeField] private string _gameSceneName = "SampleScene";
    
    [Header("Transition Settings")]
    [SerializeField] private float _transitionDelay = 1f;
    
    /// <summary>
    /// Переход в лобби
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void LoadLobbySceneServerRpc()
    {
        if (IsHost)
        {
            LoadLobbySceneClientRpc();
        }
    }
    
    [ClientRpc]
    private void LoadLobbySceneClientRpc()
    {
        LoadScene(_lobbySceneName);
    }
    
    /// <summary>
    /// Переход в игровую сцену
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void LoadGameSceneServerRpc()
    {
        if (IsHost)
        {
            LoadGameSceneClientRpc();
        }
    }
    
    [ClientRpc]
    private void LoadGameSceneClientRpc()
    {
        LoadScene(_gameSceneName);
    }
    
    /// <summary>
    /// Загрузка сцены с задержкой
    /// </summary>
    private void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneWithDelay(sceneName));
    }
    
    private System.Collections.IEnumerator LoadSceneWithDelay(string sceneName)
    {
        yield return new WaitForSeconds(_transitionDelay);
        
        if (IsHost)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
        else
        {
            // Клиенты ждут загрузки сцены от хоста
            Debug.Log($"[SceneTransitionManager] Waiting for host to load scene: {sceneName}");
        }
    }
    
    /// <summary>
    /// Перезапуск текущей сцены
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void RestartCurrentSceneServerRpc()
    {
        if (IsHost)
        {
            var currentScene = SceneManager.GetActiveScene();
            NetworkManager.Singleton.SceneManager.LoadScene(currentScene.name, LoadSceneMode.Single);
        }
    }
}

