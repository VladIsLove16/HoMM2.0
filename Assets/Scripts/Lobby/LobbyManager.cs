using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using System;

/// <summary>
/// Менеджер лобби для управления подключениями игроков и настройками игры
/// Отвечает только за бизнес-логику и сетевую синхронизацию
/// </summary>
public class LobbyManager : NetworkBehaviour
{
    [Header("Game Configuration")]
    [SerializeField] private GridContentEntrySO[] _availableConfigs;
    
    private NetworkList<LobbyPlayerData> _lobbyPlayers;
    private NetworkVariable<int> _selectedConfigIndex = new NetworkVariable<int>(0);
    private NetworkVariable<bool> _gameStarted = new NetworkVariable<bool>(false);
    
    // События для UI
    public System.Action<LobbyPlayerData[]> OnPlayersListChanged;
    public System.Action<int> OnConfigChanged;
    public System.Action<bool> OnGameStarted;
    public System.Action OnHostStarted;
    public System.Action OnClientStarted;
    
    private void Awake()
    {
        _lobbyPlayers = new NetworkList<LobbyPlayerData>();
    }
    
    public override void OnNetworkSpawn()
    {
        // Подписываемся на изменения
        _lobbyPlayers.OnListChanged += OnNetworkPlayersListChanged;
        _selectedConfigIndex.OnValueChanged += OnNetworkConfigChanged;
        _gameStarted.OnValueChanged += OnNetworkGameStartedChanged;
        
        // Добавляем текущего игрока в лобби
        if (IsClient || IsHost)
        {
            AddPlayerToLobby();
        }
        
        // Уведомляем UI о роли
        if (IsHost)
        {
            OnHostStarted?.Invoke();
        }
        else if (IsClient)
        {
            OnClientStarted?.Invoke();
        }
    }
    
    // Публичные методы для UI
    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
    }
    
    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }
    
    private void AddPlayerToLobby()
    {
        var playerData = new LobbyPlayerData
        {
            ClientId = NetworkManager.Singleton.LocalClientId,
            PlayerName = $"Player {NetworkManager.Singleton.LocalClientId}",
            IsReady = false
        };
        
        AddPlayerToLobbyServerRpc(playerData);
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void AddPlayerToLobbyServerRpc(LobbyPlayerData playerData)
    {
        _lobbyPlayers.Add(playerData);
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void RemovePlayerFromLobbyServerRpc(ulong clientId)
    {
        for (int i = _lobbyPlayers.Count - 1; i >= 0; i--)
        {
            if (_lobbyPlayers[i].ClientId == clientId)
            {
                _lobbyPlayers.RemoveAt(i);
                break;
            }
        }
    }
    
    private void OnNetworkPlayersListChanged(NetworkListEvent<LobbyPlayerData> changeEvent)
    {
        // Уведомляем UI о изменении списка игроков
        var playersArray = new LobbyPlayerData[_lobbyPlayers.Count];
        for (int i = 0; i < _lobbyPlayers.Count; i++)
        {
            playersArray[i] = _lobbyPlayers[i];
        }
        OnPlayersListChanged?.Invoke(playersArray);
    }
    
    public void OnConfigToggleChanged(bool isOn)
    {
        if (IsHost)
        {
            var newIndex = isOn ? 1 : 0;
            ChangeConfigServerRpc(newIndex);
            
            // Обновляем сервис передачи данных
            if (SceneTransitionDataService.Instance != null)
            {
                SceneTransitionDataService.Instance.SetSelectedConfiguration(newIndex);
            }
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void ChangeConfigServerRpc(int configIndex)
    {
        if (IsHost && configIndex >= 0 && configIndex < _availableConfigs.Length)
        {
            _selectedConfigIndex.Value = configIndex;
        }
    }
    
    private void OnNetworkConfigChanged(int previousValue, int newValue)
    {
        // Уведомляем UI о изменении конфигурации
        OnConfigChanged?.Invoke(newValue);
    }
    
    public void StartGame()
    {
        if (IsHost)
        {
            StartGameServerRpc();
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void StartGameServerRpc()
    {
        if (IsHost)
        {
            _gameStarted.Value = true;
        }
    }
    
    private void OnNetworkGameStartedChanged(bool previousValue, bool newValue)
    {
        if (newValue)
        {
            OnGameStarted?.Invoke(true);
            LoadGameScene();
        }
    }
    
    private void LoadGameScene()
    {
        // Сохраняем выбранную конфигурацию в сервис передачи данных
        if (SceneTransitionDataService.Instance != null)
        {
            SceneTransitionDataService.Instance.SetSelectedConfiguration(_selectedConfigIndex.Value);
        }
        
        // Загружаем игровую сцену
        NetworkManager.Singleton.SceneManager.LoadScene("SampleScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
    
    public override void OnNetworkDespawn()
    {
        if (IsClient || IsHost)
        {
            RemovePlayerFromLobbyServerRpc(NetworkManager.Singleton.LocalClientId);
        }
    }
    
    /// <summary>
    /// Получить выбранную конфигурацию юнитов
    /// </summary>
    public GridContentEntrySO GetSelectedConfig()
    {
        if (_availableConfigs != null && _selectedConfigIndex.Value < _availableConfigs.Length)
        {
            return _availableConfigs[_selectedConfigIndex.Value];
        }
        return null;
    }
    
    /// <summary>
    /// Получить конфигурацию по индексу
    /// </summary>
    public GridContentEntrySO GetConfigByIndex(int index)
    {
        if (_availableConfigs != null && index >= 0 && index < _availableConfigs.Length)
        {
            return _availableConfigs[index];
        }
        return null;
    }
}

/// <summary>
/// Данные игрока в лобби
/// </summary>
[System.Serializable]
public struct LobbyPlayerData : INetworkSerializable, IEquatable<LobbyPlayerData>
{
    public ulong ClientId;
    public FixedString128Bytes PlayerName;
    public bool IsReady;

    public bool Equals(LobbyPlayerData other)
    {
        return ClientId == other.ClientId;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref PlayerName);
        serializer.SerializeValue(ref IsReady);
    }
}

