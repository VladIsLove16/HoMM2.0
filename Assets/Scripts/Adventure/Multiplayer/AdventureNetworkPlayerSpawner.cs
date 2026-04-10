using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Adventure.Multiplayer
{
    [DisallowMultipleComponent]
    public sealed class AdventureNetworkPlayerSpawner : MonoBehaviour
    {
        private const float SpawnGroundProbeUpOffset = 5f;
        private const float SpawnGroundProbeDistance = 25f;
        private const float SpawnGroundPadding = 0.05f;

        [SerializeField] private NetworkAdventurePlayer playerPrefab;
        [SerializeField] private Transform[] spawnPoints;

        private readonly HashSet<ulong> _spawnedClients = new();
        private readonly Dictionary<ulong, Coroutine> _pendingSetupRoutines = new();
        private NetworkManager _networkManager;
        private bool _callbacksRegistered;
        private bool _isConfigured;

        public void Configure(NetworkAdventurePlayer configuredPrefab, Transform[] configuredSpawnPoints)
        {
            if (configuredPrefab != null)
                playerPrefab = configuredPrefab;

            if (configuredSpawnPoints != null && configuredSpawnPoints.Length > 0)
                spawnPoints = configuredSpawnPoints;

            RefreshConfigurationState();
            TryHookNetworkCallbacks();
            TrySetupConnectedClients();
        }

        private void OnEnable()
        {
            RefreshConfigurationState();
            TryHookNetworkCallbacks();

            if (_isConfigured)
                TrySetupConnectedClients();
        }

        private void OnDisable()
        {
            UnhookNetworkCallbacks();
        }

        private void HandleClientConnected(ulong clientId)
        {
            SchedulePlayerSetup(clientId);
        }

        private void HandleClientDisconnected(ulong clientId)
        {
            _spawnedClients.Remove(clientId);
            if (_pendingSetupRoutines.TryGetValue(clientId, out var routine) && routine != null)
            {
                StopCoroutine(routine);
                _pendingSetupRoutines.Remove(clientId);
            }
        }

        private void TrySetupPlayer(ulong clientId)
        {
            if (_spawnedClients.Contains(clientId))
                return;

            if (!TryResolveServerNetworkManager(out var networkManager))
                return;

            var existingPlayer = networkManager.SpawnManager.GetPlayerNetworkObject(clientId);
            if (existingPlayer != null)
            {
                MovePlayerToSpawnPoint(existingPlayer, clientId);
                _spawnedClients.Add(clientId);
                return;
            }

            if (networkManager.NetworkConfig.PlayerPrefab != null)
                return;

            if (!_isConfigured)
                return;

            if (playerPrefab == null)
            {
                Debug.LogError("[AdventureNetworkPlayerSpawner] Player prefab is not assigned.", this);
                return;
            }

            var spawnPoint = ResolveSpawnPoint(clientId);
            var player = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
            MovePlayerToSpawnPoint(player.NetworkObject, clientId);
            _spawnedClients.Add(clientId);
        }

        private Transform ResolveSpawnPoint(ulong clientId)
        {
            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                var index = (int)(clientId % (ulong)spawnPoints.Length);
                var point = spawnPoints[index];
                if (point != null)
                    return point;
            }

            return transform;
        }

        private void TrySetupConnectedClients()
        {
            if (!TryResolveServerNetworkManager(out var networkManager))
                return;

            foreach (var clientId in networkManager.ConnectedClientsIds)
                SchedulePlayerSetup(clientId);
        }

        private void TryHookNetworkCallbacks()
        {
            if (_callbacksRegistered || !TryResolveServerNetworkManager(out var networkManager))
                return;

            _networkManager = networkManager;
            _networkManager.OnClientConnectedCallback += HandleClientConnected;
            _networkManager.OnClientDisconnectCallback += HandleClientDisconnected;
            if (_networkManager.SceneManager != null)
                _networkManager.SceneManager.OnSceneEvent += HandleSceneEvent;
            _callbacksRegistered = true;
        }

        private void UnhookNetworkCallbacks()
        {
            if (!_callbacksRegistered || _networkManager == null)
                return;

            _networkManager.OnClientConnectedCallback -= HandleClientConnected;
            _networkManager.OnClientDisconnectCallback -= HandleClientDisconnected;
            if (_networkManager.SceneManager != null)
                _networkManager.SceneManager.OnSceneEvent -= HandleSceneEvent;
            _callbacksRegistered = false;
            _spawnedClients.Clear();
            foreach (var pair in _pendingSetupRoutines)
            {
                if (pair.Value != null)
                    StopCoroutine(pair.Value);
            }
            _pendingSetupRoutines.Clear();
            _networkManager = null;
        }

        private void SchedulePlayerSetup(ulong clientId)
        {
            if (_spawnedClients.Contains(clientId) || _pendingSetupRoutines.ContainsKey(clientId))
                return;

            _pendingSetupRoutines[clientId] = StartCoroutine(WaitForPlayerObject(clientId));
        }

        private void HandleSceneEvent(SceneEvent sceneEvent)
        {
            if (_networkManager == null || !_networkManager.IsServer)
                return;

            if (sceneEvent.SceneEventType != SceneEventType.LoadComplete)
                return;

            if (!string.Equals(sceneEvent.SceneName, gameObject.scene.name, StringComparison.Ordinal))
                return;

            ShowExistingPlayersToClient(sceneEvent.ClientId);
            SchedulePlayerSetup(sceneEvent.ClientId);
        }

        private System.Collections.IEnumerator WaitForPlayerObject(ulong clientId)
        {
            const int maxFrames = 120;
            int waitedFrames = 0;

            while (waitedFrames < maxFrames)
            {
                TrySetupPlayer(clientId);
                if (_spawnedClients.Contains(clientId))
                    break;

                waitedFrames++;
                yield return null;
            }

            if (!_spawnedClients.Contains(clientId))
            {
                Debug.LogWarning($"[AdventureNetworkPlayerSpawner] Timed out while waiting for player object of client {clientId}.", this);
            }

            _pendingSetupRoutines.Remove(clientId);
        }

        private void MovePlayerToSpawnPoint(NetworkObject playerObject, ulong clientId)
        {
            if (playerObject == null)
                return;

            var spawnPoint = ResolveSpawnPoint(clientId);
            if (spawnPoint == null)
                return;

            var targetTransform = playerObject.transform;
            var characterController = playerObject.GetComponent<CharacterController>();
            var wasEnabled = characterController != null && characterController.enabled;
            if (characterController != null)
                characterController.enabled = false;

            var targetPosition = ResolveSpawnPosition(spawnPoint, characterController);
            targetTransform.SetPositionAndRotation(targetPosition, spawnPoint.rotation);

            var networkPlayer = playerObject.GetComponent<NetworkAdventurePlayer>();
            if (networkPlayer != null)
            {
                var rpcParams = new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new[] { clientId }
                    }
                };
                networkPlayer.ApplySpawnTransformClientRpc(targetPosition, spawnPoint.rotation, rpcParams);
            }

            if (characterController != null)
                characterController.enabled = wasEnabled;
        }

        private void ShowExistingPlayersToClient(ulong clientId)
        {
#if UNITY_2023_1_OR_NEWER
            var players = UnityEngine.Object.FindObjectsByType<NetworkAdventurePlayer>(FindObjectsSortMode.None);
#else
            var players = UnityEngine.Object.FindObjectsOfType<NetworkAdventurePlayer>();
#endif
            foreach (var player in players)
            {
                if (player == null || !player.IsSpawned || player.NetworkObject == null)
                    continue;

                if (player.NetworkObject.IsNetworkVisibleTo(clientId))
                    continue;

                player.NetworkObject.NetworkShow(clientId);
            }
        }

        private static Vector3 ResolveSpawnPosition(Transform spawnPoint, CharacterController characterController)
        {
            var basePosition = spawnPoint.position;
            var probeOrigin = basePosition + Vector3.up * SpawnGroundProbeUpOffset;
            var probeDistance = SpawnGroundProbeUpOffset + SpawnGroundProbeDistance;

            if (!Physics.Raycast(probeOrigin, Vector3.down, out var hit, probeDistance, ~0, QueryTriggerInteraction.Ignore))
                return basePosition;

            if (characterController == null)
                return hit.point + Vector3.up * SpawnGroundPadding;

            var verticalOffset = (characterController.height * 0.5f) - characterController.center.y + SpawnGroundPadding;
            return hit.point + Vector3.up * verticalOffset;
        }

        private static bool TryResolveServerNetworkManager(out NetworkManager networkManager)
        {
            networkManager = NetworkManager.Singleton;
            return networkManager != null && networkManager.IsListening && networkManager.IsServer;
        }

        private void RefreshConfigurationState()
        {
            var networkManager = NetworkManager.Singleton;
            _isConfigured = playerPrefab != null ||
                            (networkManager != null && networkManager.NetworkConfig.PlayerPrefab != null);
        }
    }

    public sealed class AdventureMultiplayerRuntimeBootstrap : IInitializable
    {
        private readonly NetworkAdventurePlayer _networkPlayerPrefab;
        private readonly Transform[] _spawnPoints;

        public AdventureMultiplayerRuntimeBootstrap(
            NetworkAdventurePlayer networkPlayerPrefab,
            Transform[] spawnPoints)
        {
            _networkPlayerPrefab = networkPlayerPrefab;
            _spawnPoints = spawnPoints ?? Array.Empty<Transform>();
        }

        public void Initialize()
        {
            var networkManager = ResolveNetworkManager();
            if (networkManager == null || !networkManager.IsListening)
                return;

            if (_networkPlayerPrefab == null)
            {
                Debug.LogWarning("[AdventureMultiplayerRuntimeBootstrap] Network player prefab is not assigned. Adventure multiplayer player spawning is disabled.");
                return;
            }

            if (_networkPlayerPrefab.GetComponent<NetworkObject>() == null)
            {
                Debug.LogError("[AdventureMultiplayerRuntimeBootstrap] Network player prefab must contain a NetworkObject.", _networkPlayerPrefab);
                return;
            }

            if (!IsPlayerPrefabRegistered(networkManager))
            {
                Debug.LogError("[AdventureMultiplayerRuntimeBootstrap] Network player prefab is not registered in NetworkManager.NetworkConfig.Prefabs. Register it in DefaultNetworkPrefabs before starting the session.", _networkPlayerPrefab);
                return;
            }

            EnsureSpawnerConfigured();
        }

        private bool IsPlayerPrefabRegistered(NetworkManager networkManager)
        {
            var prefabObject = _networkPlayerPrefab.gameObject;
            if (networkManager.NetworkConfig.PlayerPrefab == prefabObject)
                return true;

            if (networkManager.NetworkConfig.Prefabs.Contains(prefabObject))
                return true;

            foreach (var list in networkManager.NetworkConfig.Prefabs.NetworkPrefabsLists)
            {
                if (list == null || list.PrefabList == null)
                    continue;

                foreach (var prefab in list.PrefabList)
                {
                    if (prefab != null && prefab.Prefab == prefabObject)
                        return true;
                }
            }

            return false;
        }

        private void EnsureSpawnerConfigured()
        {
#if UNITY_2023_1_OR_NEWER
            var spawner = UnityEngine.Object.FindFirstObjectByType<AdventureNetworkPlayerSpawner>();
#else
            var spawner = UnityEngine.Object.FindObjectOfType<AdventureNetworkPlayerSpawner>();
#endif
            if (spawner == null)
            {
                var spawnerObject = new GameObject(nameof(AdventureNetworkPlayerSpawner));
                spawner = spawnerObject.AddComponent<AdventureNetworkPlayerSpawner>();
            }

            spawner.Configure(_networkPlayerPrefab, _spawnPoints);
        }

        private static NetworkManager ResolveNetworkManager()
        {
            if (NetworkManager.Singleton != null)
                return NetworkManager.Singleton;

#if UNITY_2023_1_OR_NEWER
            return UnityEngine.Object.FindFirstObjectByType<NetworkManager>();
#else
            return UnityEngine.Object.FindObjectOfType<NetworkManager>();
#endif
        }
    }
}

