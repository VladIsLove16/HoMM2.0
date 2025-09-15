using UnityEngine;
using Unity.Netcode;
using System.Linq;

/// <summary>
/// Автоматический запуск сети для Multiplayer PlayMode
/// </summary>
public class AutoNetworkStarter : MonoBehaviour
{
    [Header("Auto Start Settings")]
    [SerializeField] private bool _autoStart = true;
    [SerializeField] private float _startDelay = 2f;
    [SerializeField] private bool _enableDebugLogs = true;
    
    private void Start()
    {
        if (_autoStart)
        {
            StartCoroutine(AutoStart());
        }
    }
    
    private System.Collections.IEnumerator AutoStart()
    {
        yield return new WaitForSeconds(_startDelay);
        
        var networkManager = NetworkManager.Singleton;
        if (networkManager == null) 
        {
            LogDebug("NetworkManager not found!");
            yield break;
        }
        
        // Автоматически определяем роль
        if (IsFirstInstance())
        {
            networkManager.StartHost();
            LogDebug("Auto-started as HOST");
        }
        else
        {
            networkManager.StartClient();
            LogDebug("Auto-started as CLIENT");
        }
    }
    
    private bool IsFirstInstance()
    {
        try
        {
            // Простой способ определить первое окно
            var processes = System.Diagnostics.Process.GetProcessesByName("Unity");
            if (processes.Length < 2) return true;
            
            var currentProcess = System.Diagnostics.Process.GetCurrentProcess();
            var firstProcess = processes.OrderBy(p => p.StartTime).First();
            
            return currentProcess.Id == firstProcess.Id;
        }
        catch (System.Exception e)
        {
            LogDebug($"Error determining instance order: {e.Message}");
            return true; // Fallback - считаем первым
        }
    }
    
    private void LogDebug(string message)
    {
        if (_enableDebugLogs)
        {
            Debug.Log($"[AutoNetworkStarter] {message}");
        }
    }
}

