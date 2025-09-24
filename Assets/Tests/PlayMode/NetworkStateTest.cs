using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using NUnit.Framework;
using Unity.Netcode;

namespace Tests.PlayMode
{
    /// <summary>
    /// Тест для проверки сетевого состояния
    /// </summary>
    public class NetworkStateTest
    {
        [UnityTest]
        public IEnumerator TestNetworkManagerState()
        {
            Debug.Log("=== Network State Test Started ===");
            
            // Проверяем NetworkManager
            if (NetworkManager.Singleton == null)
            {
                Debug.Log("❌ NetworkManager.Singleton is null - running in local mode");
            }
            else
            {
                Debug.Log($"✅ NetworkManager found");
                Debug.Log($"   - IsClient: {NetworkManager.Singleton.IsClient}");
                Debug.Log($"   - IsServer: {NetworkManager.Singleton.IsServer}");
                Debug.Log($"   - IsHost: {NetworkManager.Singleton.IsHost}");
                Debug.Log($"   - IsConnectedClient: {NetworkManager.Singleton.IsConnectedClient}");
            }
            
            // Ищем GameController
            var gameController = Object.FindAnyObjectByType<GameController>();
            if (gameController == null)
            {
                Debug.LogError("❌ GameController not found in scene!");
                yield break;
            }
            
            Debug.Log($"✅ GameController found");

            // Проверяем NetworkObject
            var networkObject = gameController.GetComponent<NetworkObject>();
            if (networkObject == null)
            {
                Debug.LogWarning("⚠️ GameController has no NetworkObject component");
            }
            else
            {
                Debug.Log($"✅ GameController has NetworkObject");
                Debug.Log($"   - IsSpawned: {networkObject.IsSpawned}");
                Debug.Log($"   - NetworkObjectId: {networkObject.NetworkObjectId}");
            }
            
            // Ждем немного
            yield return new WaitForSeconds(1f);
            
            // Проверяем, был ли вызван OnNetworkSpawn
            Debug.Log("=== Network State Test Completed ===");
            
            yield return null;
        }
        
        [UnityTest]
        public IEnumerator TestGameControllerInitialization()
        {
            Debug.Log("=== GameController Initialization Test Started ===");
            
            var gameController = Object.FindAnyObjectByType<GameController>();
            if (gameController == null)
            {
                Debug.LogError("❌ GameController not found in scene!");
                yield break;
            }
            
            // Ждем инициализации
            yield return new WaitForSeconds(0.5f);
            
            // Проверяем состояние
            Debug.Log($"GameController state:");
            Debug.Log($"   - IsHost: {gameController.IsHost}");
            Debug.Log($"   - IsClient: {gameController.IsClient}");
            Debug.Log($"   - IsServer: {gameController.IsServer}");
            
            // Проверяем конфигурацию
            var isConfigReady = gameController.IsConfigurationReady();
            Debug.Log($"   - Configuration ready: {isConfigReady}");
            
            Debug.Log("=== GameController Initialization Test Completed ===");
            
            yield return null;
        }
    }
}
