using Adventure.Infrastructure.Movement;
using Adventure.Infrastructure.Players;
using Adventure.Infrastructure.State;
using System.Collections.Generic;
using UnityEngine;
using Adventure.Domain.Dialog;

namespace Adventure.Infrastructure.Dialog
{
    public sealed class NpcBehaviorGraphRegistry
    {
        private const string DebugPrefix = "[NpcBehaviorGraphRegistry]";
        private static readonly bool VerboseLogging = false;
        private readonly ILocalAdventurePlayerProvider _localPlayerProvider;
        private readonly Dictionary<string, List<NpcBehaviorGraphBridge>> _registry = new();
        private readonly Dictionary<string, NpcBehaviorGraphBridge> _registryByNpcKey = new();

        private string _pendingDialogId;
        private string _pendingNpcKey;
        private string _activeDialogId;
        private string _activeNpcKey;
        private NpcBehaviorGraphBridge _activeBridge;

        public NpcBehaviorGraphRegistry(ILocalAdventurePlayerProvider localPlayerProvider)
        {
            _localPlayerProvider = localPlayerProvider;
            if (_localPlayerProvider != null)
                _localPlayerProvider.PlayerChanged += HandlePlayerChanged;
        }

        public void Register(string dialogId, NpcBehaviorGraphBridge bridge)
        {
            Register(dialogId, null, bridge);
        }

        public void Register(string dialogId, string npcKey, NpcBehaviorGraphBridge bridge)
        {
            if (string.IsNullOrEmpty(dialogId) || bridge == null)
            {
                return;
            }

            if (!_registry.TryGetValue(dialogId, out var bridges))
            {
                bridges = new List<NpcBehaviorGraphBridge>();
                _registry[dialogId] = bridges;
            }

            if (!bridges.Contains(bridge))
            {
                bridges.Add(bridge);
            }

            if (!string.IsNullOrWhiteSpace(npcKey))
            {
                _registryByNpcKey[npcKey] = bridge;
            }

            bridge.AssignDialogueId(dialogId);
            bridge.ConfigurePlayer(_localPlayerProvider?.MovementController);
            if (VerboseLogging)
            {
                Debug.Log($"{DebugPrefix} Register dialogId='{dialogId}' npcKey='{npcKey ?? "null"}' bridge='{bridge.name}' count={_registry.Count}");
            }

            if (_activeBridge == null && _activeDialogId == dialogId && (string.IsNullOrWhiteSpace(_activeNpcKey) || _activeNpcKey == npcKey))
            {
                _activeBridge = bridge;
                if (VerboseLogging)
                {
                    Debug.Log($"{DebugPrefix} Register restored active bridge for dialogId='{dialogId}' npcKey='{npcKey ?? "null"}' bridge='{bridge.name}'");
                }
            }
        }

        public void Unregister(string dialogId, NpcBehaviorGraphBridge bridge)
        {
            Unregister(dialogId, null, bridge);
        }

        public void Unregister(string dialogId, string npcKey, NpcBehaviorGraphBridge bridge)
        {
            if (string.IsNullOrEmpty(dialogId))
            {
                return;
            }

            if (_registry.TryGetValue(dialogId, out var bridges))
            {
                bridges.Remove(bridge);
                if (bridges.Count == 0)
                {
                    _registry.Remove(dialogId);
                }
            }

            if (!string.IsNullOrWhiteSpace(npcKey) && _registryByNpcKey.TryGetValue(npcKey, out var keyedBridge) && keyedBridge == bridge)
            {
                _registryByNpcKey.Remove(npcKey);
            }

            if (VerboseLogging)
            {
                Debug.Log($"{DebugPrefix} Unregister dialogId='{dialogId}' npcKey='{npcKey ?? "null"}' bridge='{bridge?.name ?? "null"}' count={_registry.Count}");
            }

            if (_activeBridge == bridge && (string.IsNullOrEmpty(dialogId) || _activeDialogId == dialogId))
            {
                _activeBridge = null;
                _activeDialogId = null;
                _activeNpcKey = null;
            }

            if (_pendingDialogId == dialogId)
            {
                _pendingDialogId = null;
                _pendingNpcKey = null;
            }
        }

        public void SetPendingDialog(string dialogId)
        {
            SetPendingDialog(dialogId, null);
        }

        public void SetPendingDialog(string dialogId, string npcKey)
        {
            _pendingDialogId = dialogId;
            _pendingNpcKey = npcKey;
        }

        public void ClearPending(string dialogId)
        {
            if (string.IsNullOrEmpty(dialogId) || _pendingDialogId == dialogId)
            {
                _pendingDialogId = null;
                _pendingNpcKey = null;
            }
        }

        public void ForceActivate(string dialogId)
        {
            ForceActivate(dialogId, null);
        }

        public void ForceActivate(string dialogId, string npcKey)
        {
            if (string.IsNullOrEmpty(dialogId))
            {
                return;
            }

            _activeDialogId = dialogId;
            _activeNpcKey = npcKey;

            if (TryResolveBridge(dialogId, npcKey, out var bridge))
            {
                _activeBridge = bridge;
                if (VerboseLogging)
                {
                    Debug.Log($"{DebugPrefix} ForceActivate dialogId='{dialogId}' npcKey='{npcKey ?? "null"}' bridge='{bridge.name}'");
                }

                return;
            }

            Debug.LogWarning($"{DebugPrefix} ForceActivate could not find bridge for dialogId='{dialogId}' npcKey='{npcKey ?? "null"}'. Registered ids: {string.Join(", ", _registry.Keys)}");
        }

        public void Activate(string dialogId, NpcBehaviorGraphBridge bridge)
        {
            if (string.IsNullOrEmpty(dialogId) || bridge == null)
            {
                return;
            }

            _activeDialogId = dialogId;
            _activeNpcKey = null;
            _activeBridge = bridge;
            _pendingDialogId = null;
            _pendingNpcKey = null;
            if (VerboseLogging)
            {
                Debug.Log($"{DebugPrefix} Activate direct dialogId='{dialogId}' bridge='{bridge.name}'");
            }
        }

        public void ClearActive(string dialogId, NpcBehaviorGraphBridge bridge)
        {
            if (bridge == null)
            {
                return;
            }

            if (_activeBridge == bridge && (string.IsNullOrEmpty(dialogId) || _activeDialogId == dialogId))
            {
                if (VerboseLogging)
                {
                    Debug.Log($"{DebugPrefix} ClearActive dialogId='{dialogId ?? "null"}' bridge='{bridge.name}'");
                }

                _activeBridge = null;
                _activeDialogId = null;
                _activeNpcKey = null;
            }
        }

        public void NotifyDialogOpened()
        {
            EnsureActiveBridge();
            _activeBridge?.NotifyDialogOpened();
        }

        public void NotifyDialogOpened(string dialogId)
        {
            ActivateBridge(dialogId);
            NotifyDialogOpened();
        }

        public void NotifyDialogClosed()
        {
            if (VerboseLogging)
            {
                Debug.Log(
                    $"{DebugPrefix} NotifyDialogClosed activeBridge='{_activeBridge?.name ?? "null"}' activeDialogId='{_activeDialogId ?? "null"}' pendingDialog='{_pendingDialogId ?? "null"}'");
            }

            _activeBridge?.NotifyDialogClosed();
            _activeBridge = null;
            _activeDialogId = null;
        }

        public void NotifyDialogClosed(string dialogId)
        {
            ActivateBridge(dialogId);
            NotifyDialogClosed();
        }

        public void NotifyChoiceWindowState(bool isOpen)
        {
            _activeBridge?.NotifyChoiceWindowState(isOpen);
        }

        public void NotifyChoiceWindowState(bool isOpen, string dialogId)
        {
            ActivateBridge(dialogId);
            NotifyChoiceWindowState(isOpen);
        }

        public void NotifyBattleRequested()
        {
            _activeBridge?.NotifyBattleRequested();
        }

        public void NotifyBattleRequested(string dialogId)
        {
            ActivateBridge(dialogId);
            NotifyBattleRequested();
        }

        public void NotifyChoiceAction(DialogueChoiceAction action)
        {
            _activeBridge?.NotifyChoiceAction(action);
        }

        public void NotifyChoiceAction(DialogueChoiceAction action, string dialogId)
        {
            ActivateBridge(dialogId);
            NotifyChoiceAction(action);
        }

        public void NotifyBattleOutcome(BattleOutcome outcome)
        {
            if (outcome == BattleOutcome.Unknown)
            {
                return;
            }

            EnsureActiveBridge();
            _activeBridge?.NotifyBattleOutcome(outcome);
        }

        private void EnsureActiveBridge()
        {
            if (_activeBridge != null || string.IsNullOrEmpty(_pendingDialogId))
            {
                return;
            }

            if (TryResolveBridge(_pendingDialogId, _pendingNpcKey, out var bridge))
            {
                _activeBridge = bridge;
                _activeDialogId = _pendingDialogId;
                _activeNpcKey = _pendingNpcKey;
            }

            _pendingDialogId = null;
            _pendingNpcKey = null;
        }

        private void ActivateBridge(string dialogId)
        {
            if (_activeBridge != null && _activeDialogId == dialogId)
            {
                return;
            }

            ForceActivate(dialogId, null);
        }

        private void HandlePlayerChanged()
        {
            var playerMovementController = _localPlayerProvider?.MovementController;
            var configured = new HashSet<NpcBehaviorGraphBridge>();
            foreach (var bridges in _registry.Values)
            {
                if (bridges == null)
                {
                    continue;
                }

                foreach (var bridge in bridges)
                {
                    if (bridge != null && configured.Add(bridge))
                    {
                        bridge.ConfigurePlayer(playerMovementController);
                    }
                }
            }
        }

        public void ConfigureDialogPlayer(string dialogId, Transform playerTransform)
        {
            ConfigureDialogPlayer(dialogId, playerTransform, null);
        }

        public void ConfigureDialogPlayer(string dialogId, Transform playerTransform, string npcKey)
        {
            if (string.IsNullOrEmpty(dialogId))
                return;

            if (TryResolveBridge(dialogId, npcKey, out var bridge))
            {
                var movementController = playerTransform != null
                    ? playerTransform.GetComponent<PlayerMovementController>()
                    : null;

                bridge.ConfigurePlayer(movementController ?? _localPlayerProvider?.MovementController);
            }
        }

        private bool TryResolveBridge(string dialogId, string npcKey, out NpcBehaviorGraphBridge bridge)
        {
            bridge = null;

            if (!string.IsNullOrWhiteSpace(npcKey) && _registryByNpcKey.TryGetValue(npcKey, out bridge) && bridge != null)
            {
                return true;
            }

            if (!_registry.TryGetValue(dialogId, out var bridges) || bridges == null || bridges.Count == 0)
            {
                return false;
            }

            for (var i = bridges.Count - 1; i >= 0; i--)
            {
                if (bridges[i] != null)
                {
                    bridge = bridges[i];
                    return true;
                }
            }

            return false;
        }
    }
}
