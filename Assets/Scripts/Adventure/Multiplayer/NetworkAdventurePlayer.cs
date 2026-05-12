using System;
using System.Collections;
using System.Collections.Generic;
using Adventure.Domain.Inventory;
using Adventure.Infrastructure.Interaction;
using Adventure.Infrastructure.Movement;
using Adventure.Infrastructure.Players;
using Adventure.Infrastructure.State;
using Adventure.Integration.Battle;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Adventure.Multiplayer
{
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(NetworkTransform))]
    public sealed class NetworkAdventurePlayer : NetworkBehaviour
    {
        private const string PlayerNamePrefsKey = "Lobby.LocalPlayerName";

        [Header("Gameplay")]
        [SerializeField] private PlayerMovementController movementController;
        [SerializeField] private PlayerInteractionController interactionController;
        [SerializeField] private CharacterController characterController;

        [Header("Owner Only")]
        [SerializeField] private Camera ownerCamera;
        [SerializeField] private AudioListener ownerAudioListener;

        [Header("Presentation")]
        [SerializeField] private AdventurePlayerNameplateView nameplateView;
        [SerializeField] private Animator animator;
        [SerializeField] private string moveSpeedParam = "MoveSpeed";
        [SerializeField] private string movingParam = "IsMoving";
        [SerializeField, Range(0.01f, 0.5f)] private float movementSyncThreshold = 0.05f;

        [Inject(Optional = true)] private MushroomInventoryModel _inventoryModel;
        [Inject(Optional = true)] private IGridConfigurationGateway _gridConfigurationGateway;
        [Inject(Optional = true)] private ArmyFormationResolver _formationResolver;
        [Inject(Optional = true)] private SinglePlayerStartConfigurationSO _startConfiguration;

        private readonly NetworkVariable<FixedString128Bytes> _displayName =
            new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private readonly NetworkVariable<float> _movementSpeed =
            new(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private readonly NetworkVariable<bool> _isMoving =
            new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        private static PendingOnlineBattleState s_pendingOnlineBattle;

        private ILocalAdventurePlayerProvider _localPlayerProvider;
        private Vector3 _lastSyncedPosition;
        private bool _registeredAsLocalPlayer;

        public bool RequestStartOnlineBattle(BattleLaunchContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (!IsOwner || !IsSpawned)
            {
                Debug.LogWarning("[NetworkAdventurePlayer] Online battle request ignored because the local player object is not owned/spawned.", this);
                return false;
            }

            RequestStartOnlineBattleServerRpc(
                ToFixedString(context.DialogId),
                ToFixedString(context.VictoryNodeId),
                ToFixedString(context.DefeatNodeId));

            return true;
        }

        public override void OnNetworkSpawn()
        {
            TryResolveLocalPlayerProvider();

            _displayName.OnValueChanged += OnDisplayNameChanged;
            _movementSpeed.OnValueChanged += OnMovementSpeedChanged;
            _isMoving.OnValueChanged += OnMovingChanged;

            ConfigureOwnership();
            ApplyRemoteVisualState();
            OnDisplayNameChanged(default, _displayName.Value);
        }

        public override void OnNetworkDespawn()
        {
            _displayName.OnValueChanged -= OnDisplayNameChanged;
            _movementSpeed.OnValueChanged -= OnMovementSpeedChanged;
            _isMoving.OnValueChanged -= OnMovingChanged;

            UnregisterLocalPlayer();
        }

        private void Update()
        {
            if (!IsOwner)
                return;

            EnsureOwnerDependenciesReady();

            var localPlayerName = ResolveLocalPlayerName();
            if (!_displayName.Value.Equals(localPlayerName))
            {
                _displayName.Value = localPlayerName;
            }

            SyncMovementState();
        }

        [ClientRpc]
        public void ApplySpawnTransformClientRpc(Vector3 position, Quaternion rotation, ClientRpcParams rpcParams = default)
        {
            if (!IsOwner)
                return;

            var wasEnabled = characterController != null && characterController.enabled;
            if (characterController != null)
                characterController.enabled = false;

            transform.SetPositionAndRotation(position, rotation);
            _lastSyncedPosition = position;

            if (movementController != null)
                movementController.ResetExternalInput();

            if (characterController != null)
                characterController.enabled = wasEnabled;
        }

        [ServerRpc]
        private void RequestStartOnlineBattleServerRpc(
            FixedString128Bytes dialogId,
            FixedString128Bytes victoryNodeId,
            FixedString128Bytes defeatNodeId,
            ServerRpcParams rpcParams = default)
        {
            if (rpcParams.Receive.SenderClientId != OwnerClientId)
            {
                Debug.LogWarning("[NetworkAdventurePlayer] Rejected online battle request from a non-owner sender.", this);
                return;
            }

            if (s_pendingOnlineBattle != null)
            {
                RejectOnlineBattleClientRpc(
                    new FixedString128Bytes("Another online battle request is already in progress."),
                    BuildClientRpcParams(OwnerClientId));
                return;
            }

            var opponent = FindFirstOpponent(OwnerClientId);
            if (opponent == null)
            {
                RejectOnlineBattleClientRpc(
                    new FixedString128Bytes("No opponent is available for online battle."),
                    BuildClientRpcParams(OwnerClientId));
                return;
            }

            s_pendingOnlineBattle = new PendingOnlineBattleState(
                OwnerClientId,
                opponent.OwnerClientId,
                dialogId,
                victoryNodeId,
                defeatNodeId);

            Debug.Log($"[NetworkAdventurePlayer] Starting online battle request. Initiator={OwnerClientId}, Opponent={opponent.OwnerClientId}.", this);

            RequestBattleArmyClientRpc(BuildClientRpcParams(OwnerClientId));
            opponent.RequestBattleArmyClientRpc(BuildClientRpcParams(opponent.OwnerClientId));
        }

        [ClientRpc]
        private void RequestBattleArmyClientRpc(ClientRpcParams rpcParams = default)
        {
            if (!IsOwner)
                return;

            var armyData = BuildArmyPayload(ResolveLocalArmy());
            SubmitBattleArmyServerRpc(armyData);
        }

        [ServerRpc]
        private void SubmitBattleArmyServerRpc(BattleArmyStackNetworkData[] armyData, ServerRpcParams rpcParams = default)
        {
            if (s_pendingOnlineBattle == null)
            {
                Debug.LogWarning("[NetworkAdventurePlayer] Ignoring army submission because there is no pending online battle.");
                return;
            }

            var senderClientId = rpcParams.Receive.SenderClientId;
            if (!s_pendingOnlineBattle.IsParticipant(senderClientId))
            {
                Debug.LogWarning($"[NetworkAdventurePlayer] Ignoring army submission from unrelated client {senderClientId}.");
                return;
            }

            s_pendingOnlineBattle.StoreArmy(senderClientId, armyData);
            Debug.Log($"[NetworkAdventurePlayer] Received army for client {senderClientId}.", this);

            if (!s_pendingOnlineBattle.IsReady)
                return;

            DispatchPendingOnlineBattle();
        }

        [ClientRpc]
        private void PrepareOnlineBattleClientRpc(
            BattleArmyStackNetworkData[] bottomArmy,
            BattleArmyStackNetworkData[] topArmy,
            Team localTeam,
            Team battlefieldBottomTeam,
            bool resumeDialogAfterBattle,
            FixedString128Bytes dialogId,
            FixedString128Bytes victoryNodeId,
            FixedString128Bytes defeatNodeId,
            ClientRpcParams rpcParams = default)
        {
            if (!IsOwner)
                return;

            if (!TryPrepareOnlineBattle(
                    bottomArmy,
                    topArmy,
                    localTeam,
                    battlefieldBottomTeam,
                    resumeDialogAfterBattle,
                    dialogId.ToString(),
                    victoryNodeId.ToString(),
                    defeatNodeId.ToString()))
            {
                Debug.LogError("[NetworkAdventurePlayer] Failed to prepare local GridFight configuration for online battle.", this);
            }
        }

        [ClientRpc]
        private void RejectOnlineBattleClientRpc(FixedString128Bytes reason, ClientRpcParams rpcParams = default)
        {
            if (!IsOwner)
                return;

            Debug.LogWarning($"[NetworkAdventurePlayer] Online battle request rejected: {reason}", this);
        }

        private void DispatchPendingOnlineBattle()
        {
            var pendingState = s_pendingOnlineBattle;
            s_pendingOnlineBattle = null;

            if (pendingState == null)
                return;

            var initiator = FindPlayerByClientId(pendingState.InitiatorClientId);
            var opponent = FindPlayerByClientId(pendingState.OpponentClientId);
            if (initiator == null || opponent == null)
            {
                Debug.LogError("[NetworkAdventurePlayer] Unable to resolve online battle participants before scene transfer.", this);
                return;
            }

            var initiatorArmy = pendingState.GetArmy(pendingState.InitiatorClientId);
            var opponentArmy = pendingState.GetArmy(pendingState.OpponentClientId);
            if (!TryResolveParticipantOrder(
                    pendingState,
                    initiatorArmy,
                    opponentArmy,
                    out var bottomArmy,
                    out var topArmy,
                    out var initiatorTeam,
                    out var opponentTeam,
                    out var battlefieldBottomTeam))
            {
                Debug.LogError("[NetworkAdventurePlayer] Unable to resolve online battle team order.", this);
                return;
            }

            initiator.PrepareOnlineBattleClientRpc(
                bottomArmy,
                topArmy,
                initiatorTeam,
                battlefieldBottomTeam,
                resumeDialogAfterBattle: !pendingState.DialogId.IsEmpty,
                pendingState.DialogId,
                pendingState.VictoryNodeId,
                pendingState.DefeatNodeId,
                BuildClientRpcParams(pendingState.InitiatorClientId));

            opponent.PrepareOnlineBattleClientRpc(
                bottomArmy,
                topArmy,
                opponentTeam,
                battlefieldBottomTeam,
                resumeDialogAfterBattle: false,
                default,
                default,
                default,
                BuildClientRpcParams(pendingState.OpponentClientId));

            StartCoroutine(LoadGridFightSceneNextFrame());
        }

        private IEnumerator LoadGridFightSceneNextFrame()
        {
            yield return null;

            var networkManager = NetworkManager;
            if (networkManager == null || !networkManager.IsServer)
            {
                Debug.LogError("[NetworkAdventurePlayer] Cannot load GridFight because the local instance is not the server.", this);
                yield break;
            }

            networkManager.SceneManager.LoadScene(SceneLoader.Scene.GridFight.ToString(), LoadSceneMode.Single);
        }

        private bool TryPrepareOnlineBattle(
            BattleArmyStackNetworkData[] bottomArmyData,
            BattleArmyStackNetworkData[] topArmyData,
            Team localTeam,
            Team battlefieldBottomTeam,
            bool resumeDialogAfterBattle,
            string dialogId,
            string victoryNodeId,
            string defeatNodeId)
        {
            var gridGateway = ResolveGridConfigurationGateway();
            if (gridGateway == null)
            {
                Debug.LogError("[NetworkAdventurePlayer] GridConfigurationGateway is not available for online battle preparation.", this);
                return false;
            }

            var bottomArmy = ToUnitStackData(bottomArmyData);
            var topArmy = ToUnitStackData(topArmyData);
            var localArmy = localTeam == battlefieldBottomTeam ? bottomArmy : topArmy;
            var enemyArmy = localTeam == battlefieldBottomTeam ? topArmy : bottomArmy;

            var startConfiguration = ResolveStartConfiguration();
            startConfiguration?.SetGameMode(GameMode.Multiplayer);
            startConfiguration?.SetTeam(localTeam);
            startConfiguration?.SetBattlefieldBottomTeam(battlefieldBottomTeam);

            var playerMovement = ResolveLocalPlayerProvider()?.MovementController;
            if (playerMovement != null)
            {
                BattleStateCache.CapturePlayerTransform(playerMovement.transform);
            }

            BattleStateCache.StoreInventorySnapshot(localArmy);

            if (resumeDialogAfterBattle && !string.IsNullOrWhiteSpace(dialogId))
            {
                var enemyLineup = ArmyLineupSO.CreateRuntimeLineup(enemyArmy);
                BattleStateCache.ScheduleBattleDialog(
                    dialogId,
                    enemyLineup,
                    EmptyToNull(victoryNodeId),
                    EmptyToNull(defeatNodeId));
            }

            var payload = new BattleSetupPayload(
                new BattleArmies(bottomArmy, topArmy),
                SceneLoader.Scene.Adventure,
                localTeam,
                battlefieldBottomTeam);

            gridGateway.PrepareBattle(payload, ResolveFormationResolver());
            return true;
        }

        private void ConfigureOwnership()
        {
            if (IsOwner)
            {
                EnsureOwnerDependenciesReady();
                _displayName.Value = ResolveLocalPlayerName();

                if (movementController != null)
                    movementController.enabled = true;

                if (interactionController != null)
                    interactionController.enabled = true;

                if (characterController != null)
                    characterController.enabled = true;

                if (ownerCamera != null)
                    ownerCamera.gameObject.SetActive(true);

                if (ownerAudioListener != null)
                    ownerAudioListener.enabled = true;

                if (nameplateView != null)
                    nameplateView.SetVisible(false);

                _lastSyncedPosition = transform.position;
                return;
            }

            if (movementController != null)
                movementController.enabled = false;

            if (interactionController != null)
                interactionController.enabled = false;

            if (characterController != null)
                characterController.enabled = false;

            if (ownerCamera != null)
                ownerCamera.gameObject.SetActive(false);

            if (ownerAudioListener != null)
                ownerAudioListener.enabled = false;

            if (nameplateView != null)
                nameplateView.SetVisible(true);
        }

        private void EnsureOwnerDependenciesReady()
        {
            if (!IsOwner)
                return;

            TryResolveLocalPlayerProvider();

            if (_registeredAsLocalPlayer)
                return;

            if (_localPlayerProvider == null || movementController == null)
                return;

            _localPlayerProvider.Register(movementController, interactionController);
            _registeredAsLocalPlayer = true;
        }

        private void UnregisterLocalPlayer()
        {
            if (!_registeredAsLocalPlayer || _localPlayerProvider == null || movementController == null)
                return;

            _localPlayerProvider.Unregister(movementController);
            _registeredAsLocalPlayer = false;
        }

        private void TryResolveLocalPlayerProvider()
        {
            if (_localPlayerProvider != null)
                return;

            var sceneContext = FindSceneContext();
            _localPlayerProvider = sceneContext != null
                ? sceneContext.Container.TryResolve<ILocalAdventurePlayerProvider>()
                : null;
        }

        private void SyncMovementState()
        {
            var currentPosition = transform.position;
            var distance = Vector3.Distance(currentPosition, _lastSyncedPosition);
            var isMovingNow = distance > movementSyncThreshold;

            if (Time.deltaTime > Mathf.Epsilon)
            {
                var speed = distance / Time.deltaTime;
                if (!Mathf.Approximately(_movementSpeed.Value, speed))
                    _movementSpeed.Value = speed;
            }

            if (_isMoving.Value != isMovingNow)
                _isMoving.Value = isMovingNow;

            _lastSyncedPosition = currentPosition;
        }

        private void ApplyRemoteVisualState()
        {
            if (animator == null || IsOwner)
                return;

            if (!string.IsNullOrWhiteSpace(movingParam))
                animator.SetBool(movingParam, _isMoving.Value);

            if (!string.IsNullOrWhiteSpace(moveSpeedParam))
                animator.SetFloat(moveSpeedParam, _movementSpeed.Value);
        }

        private void OnDisplayNameChanged(FixedString128Bytes _, FixedString128Bytes current)
        {
            if (nameplateView == null || IsOwner)
                return;

            nameplateView.SetDisplayName(current.ToString());
        }

        private void OnMovementSpeedChanged(float _, float current)
        {
            if (animator == null || IsOwner || string.IsNullOrWhiteSpace(moveSpeedParam))
                return;

            animator.SetFloat(moveSpeedParam, current);
        }

        private void OnMovingChanged(bool _, bool current)
        {
            if (animator == null || IsOwner || string.IsNullOrWhiteSpace(movingParam))
                return;

            animator.SetBool(movingParam, current);
        }

        private IReadOnlyList<UnitStackData> ResolveLocalArmy()
        {
            if (_inventoryModel != null)
                return _inventoryModel.GetData();

            var sceneContext = FindSceneContext();
            _inventoryModel = sceneContext != null
                ? sceneContext.Container.TryResolve<MushroomInventoryModel>()
                : null;

            if (_inventoryModel != null)
                return _inventoryModel.GetData();

            if (BattleStateCache.TryGetInventorySnapshot(out var snapshot) && snapshot != null)
                return snapshot;

            return Array.Empty<UnitStackData>();
        }

        private IGridConfigurationGateway ResolveGridConfigurationGateway()
        {
            if (_gridConfigurationGateway != null)
                return _gridConfigurationGateway;

            var sceneContext = FindSceneContext();
            _gridConfigurationGateway = sceneContext != null
                ? sceneContext.Container.TryResolve<IGridConfigurationGateway>()
                : null;

            if (_gridConfigurationGateway != null)
                return _gridConfigurationGateway;

#if UNITY_2023_1_OR_NEWER
            return FindFirstObjectByType<GridConfigurationGateway>();
#else
            return FindObjectOfType<GridConfigurationGateway>();
#endif
        }

        private ArmyFormationResolver ResolveFormationResolver()
        {
            if (_formationResolver != null)
                return _formationResolver;

            var sceneContext = FindSceneContext();
            _formationResolver = sceneContext != null
                ? sceneContext.Container.TryResolve<ArmyFormationResolver>()
                : null;

            return _formationResolver;
        }

        private SinglePlayerStartConfigurationSO ResolveStartConfiguration()
        {
            if (_startConfiguration != null)
                return _startConfiguration;

            var sceneContext = FindSceneContext();
            _startConfiguration = sceneContext != null
                ? sceneContext.Container.TryResolve<SinglePlayerStartConfigurationSO>()
                : null;

            return _startConfiguration;
        }

        private ILocalAdventurePlayerProvider ResolveLocalPlayerProvider()
        {
            TryResolveLocalPlayerProvider();
            return _localPlayerProvider;
        }

        private FixedString128Bytes ResolveLocalPlayerName()
        {
            var value = PlayerPrefs.GetString(PlayerNamePrefsKey, string.Empty);
            if (string.IsNullOrWhiteSpace(value))
            {
                value = $"Player {OwnerClientId}";
            }

            return new FixedString128Bytes(value);
        }

        private static SceneContext FindSceneContext()
        {
#if UNITY_2023_1_OR_NEWER
            return FindFirstObjectByType<SceneContext>();
#else
            return FindObjectOfType<SceneContext>();
#endif
        }

        private static NetworkAdventurePlayer FindPlayerByClientId(ulong clientId)
        {
            var players = FindPlayers();
            for (int i = 0; i < players.Length; i++)
            {
                var player = players[i];
                if (player != null && player.IsSpawned && player.OwnerClientId == clientId)
                    return player;
            }

            return null;
        }

        private static NetworkAdventurePlayer FindFirstOpponent(ulong requesterClientId)
        {
            var players = FindPlayers();
            Array.Sort(players, ComparePlayersByOwnerClientId);

            for (int i = 0; i < players.Length; i++)
            {
                var player = players[i];
                if (player == null || !player.IsSpawned || player.OwnerClientId == requesterClientId)
                    continue;

                return player;
            }

            return null;
        }

        private static NetworkAdventurePlayer[] FindPlayers()
        {
#if UNITY_2023_1_OR_NEWER
            return FindObjectsByType<NetworkAdventurePlayer>(FindObjectsSortMode.None);
#else
            return FindObjectsOfType<NetworkAdventurePlayer>();
#endif
        }

        private static int ComparePlayersByOwnerClientId(NetworkAdventurePlayer left, NetworkAdventurePlayer right)
        {
            if (ReferenceEquals(left, right))
                return 0;
            if (left == null)
                return 1;
            if (right == null)
                return -1;

            return left.OwnerClientId.CompareTo(right.OwnerClientId);
        }

        private static BattleArmyStackNetworkData[] BuildArmyPayload(IReadOnlyList<UnitStackData> stacks)
        {
            if (stacks == null || stacks.Count == 0)
                return Array.Empty<BattleArmyStackNetworkData>();

            var payload = new BattleArmyStackNetworkData[stacks.Count];
            for (int i = 0; i < stacks.Count; i++)
            {
                var stack = stacks[i];
                payload[i] = new BattleArmyStackNetworkData((int)stack.UnitType, stack.Amount);
            }

            return payload;
        }

        private static List<UnitStackData> ToUnitStackData(IReadOnlyList<BattleArmyStackNetworkData> payload)
        {
            if (payload == null || payload.Count == 0)
                return new List<UnitStackData>();

            var stacks = new List<UnitStackData>(payload.Count);
            for (int i = 0; i < payload.Count; i++)
            {
                var entry = payload[i];
                if (entry.Amount <= 0)
                    continue;

                stacks.Add(new UnitStackData((UnitType)entry.UnitType, entry.Amount));
            }

            return stacks;
        }

        private static FixedString128Bytes ToFixedString(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? default : new FixedString128Bytes(value);
        }

        private static string EmptyToNull(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        private static ClientRpcParams BuildClientRpcParams(ulong clientId)
        {
            return new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new[] { clientId }
                }
            };
        }

        private bool TryResolveParticipantOrder(
            PendingOnlineBattleState pendingState,
            BattleArmyStackNetworkData[] initiatorArmy,
            BattleArmyStackNetworkData[] opponentArmy,
            out BattleArmyStackNetworkData[] bottomArmy,
            out BattleArmyStackNetworkData[] topArmy,
            out Team initiatorTeam,
            out Team opponentTeam,
            out Team battlefieldBottomTeam)
        {
            bottomArmy = Array.Empty<BattleArmyStackNetworkData>();
            topArmy = Array.Empty<BattleArmyStackNetworkData>();
            initiatorTeam = Team.None;
            opponentTeam = Team.None;
            battlefieldBottomTeam = Team.None;

            var initiatorIsServer = NetworkManager != null && pendingState.InitiatorClientId == NetworkManager.ServerClientId;
            battlefieldBottomTeam = ResolveStartConfiguration()?.PlayerTeam ?? Team.Blue;
            opponentTeam = ResolveEnemyBattleTeam(battlefieldBottomTeam);
            initiatorTeam = initiatorIsServer ? battlefieldBottomTeam : opponentTeam;
            opponentTeam = initiatorIsServer ? opponentTeam : battlefieldBottomTeam;

            bottomArmy = initiatorIsServer ? initiatorArmy : opponentArmy;
            topArmy = initiatorIsServer ? opponentArmy : initiatorArmy;
            return battlefieldBottomTeam != Team.None && initiatorTeam != Team.None && opponentTeam != Team.None;
        }

        private static Team ResolveEnemyBattleTeam(Team bottomTeam)
        {
            return bottomTeam switch
            {
                Team.Red => Team.Blue,
                Team.Green => Team.Yellow,
                Team.Yellow => Team.Green,
                _ => Team.Red
            };
        }

        private sealed class PendingOnlineBattleState
        {
            private readonly Dictionary<ulong, BattleArmyStackNetworkData[]> _armiesByClientId = new();

            public PendingOnlineBattleState(
                ulong initiatorClientId,
                ulong opponentClientId,
                FixedString128Bytes dialogId,
                FixedString128Bytes victoryNodeId,
                FixedString128Bytes defeatNodeId)
            {
                InitiatorClientId = initiatorClientId;
                OpponentClientId = opponentClientId;
                DialogId = dialogId;
                VictoryNodeId = victoryNodeId;
                DefeatNodeId = defeatNodeId;
            }

            public ulong InitiatorClientId { get; }
            public ulong OpponentClientId { get; }
            public FixedString128Bytes DialogId { get; }
            public FixedString128Bytes VictoryNodeId { get; }
            public FixedString128Bytes DefeatNodeId { get; }

            public bool IsReady =>
                _armiesByClientId.ContainsKey(InitiatorClientId) &&
                _armiesByClientId.ContainsKey(OpponentClientId);

            public bool IsParticipant(ulong clientId)
            {
                return clientId == InitiatorClientId || clientId == OpponentClientId;
            }

            public void StoreArmy(ulong clientId, BattleArmyStackNetworkData[] armyData)
            {
                _armiesByClientId[clientId] = armyData ?? Array.Empty<BattleArmyStackNetworkData>();
            }

            public BattleArmyStackNetworkData[] GetArmy(ulong clientId)
            {
                return _armiesByClientId.TryGetValue(clientId, out var armyData)
                    ? armyData ?? Array.Empty<BattleArmyStackNetworkData>()
                    : Array.Empty<BattleArmyStackNetworkData>();
            }
        }
    }

    public struct BattleArmyStackNetworkData : INetworkSerializable
    {
        public BattleArmyStackNetworkData(int unitType, int amount)
        {
            UnitType = unitType;
            Amount = amount;
        }

        public int UnitType;
        public int Amount;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref UnitType);
            serializer.SerializeValue(ref Amount);
        }
    }
}
