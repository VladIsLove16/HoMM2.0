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
        private readonly ILocalAdventurePlayerProvider _localPlayerProvider;
        private readonly Dictionary<string, NpcBehaviorGraphBridge> _registry = new();

        private string _pendingDialogId;
        private string _activeDialogId;
        private NpcBehaviorGraphBridge _activeBridge;

        public NpcBehaviorGraphRegistry(ILocalAdventurePlayerProvider localPlayerProvider)
        {
            _localPlayerProvider = localPlayerProvider;
            if (_localPlayerProvider != null)
                _localPlayerProvider.PlayerChanged += HandlePlayerChanged;
        }

        public void Register(string dialogId, NpcBehaviorGraphBridge bridge)
        {
            if (string.IsNullOrEmpty(dialogId) || bridge == null)
            {
                return;
            }

            _registry[dialogId] = bridge;
            bridge.AssignDialogueId(dialogId);
            bridge.ConfigurePlayer(_localPlayerProvider?.MovementController);
            Debug.Log($"{DebugPrefix} Register dialogId='{dialogId}' bridge='{bridge.name}' count={_registry.Count}");

            if (_activeBridge == null && _activeDialogId == dialogId)
            {
                _activeBridge = bridge;
                Debug.Log($"{DebugPrefix} Register restored active bridge for dialogId='{dialogId}' bridge='{bridge.name}'");
            }
        }

        public void Unregister(string dialogId, NpcBehaviorGraphBridge bridge)
        {
            if (string.IsNullOrEmpty(dialogId))
            {
                return;
            }

            if (_registry.TryGetValue(dialogId, out var current) && current == bridge)
            {
                _registry.Remove(dialogId);
                Debug.Log($"{DebugPrefix} Unregister removed dialogId='{dialogId}' bridge='{bridge.name}' count={_registry.Count}");
            }

            if (_activeBridge == bridge && _activeDialogId != dialogId)
            {
                _activeBridge = null;
            }

            if (_pendingDialogId == dialogId)
            {
                _pendingDialogId = null;
            }
        }

        public void SetPendingDialog(string dialogId)
        {
            _pendingDialogId = dialogId;
        }

        public void ClearPending(string dialogId)
        {
            if (string.IsNullOrEmpty(dialogId) || _pendingDialogId == dialogId)
            {
                _pendingDialogId = null;
            }
        }

        public void ForceActivate(string dialogId)
        {
            if (string.IsNullOrEmpty(dialogId))
            {
                return;
            }

            _activeDialogId = dialogId;

            if (_registry.TryGetValue(dialogId, out var bridge))
            {
                _activeBridge = bridge;
                Debug.Log($"{DebugPrefix} ForceActivate dialogId='{dialogId}' bridge='{bridge.name}'");
                return;
            }

            Debug.LogWarning($"{DebugPrefix} ForceActivate could not find bridge for dialogId='{dialogId}'. Registered ids: {string.Join(", ", _registry.Keys)}");
        }

        public void Activate(string dialogId, NpcBehaviorGraphBridge bridge)
        {
            if (string.IsNullOrEmpty(dialogId) || bridge == null)
            {
                return;
            }

            _activeDialogId = dialogId;
            _activeBridge = bridge;
            _pendingDialogId = null;
            Debug.Log($"{DebugPrefix} Activate direct dialogId='{dialogId}' bridge='{bridge.name}'");
        }

        public void ClearActive(string dialogId, NpcBehaviorGraphBridge bridge)
        {
            if (bridge == null)
            {
                return;
            }

            if (_activeBridge == bridge && (string.IsNullOrEmpty(dialogId) || _activeDialogId == dialogId))
            {
                Debug.Log($"{DebugPrefix} ClearActive dialogId='{dialogId ?? "null"}' bridge='{bridge.name}'");
                _activeBridge = null;
                _activeDialogId = null;
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
            Debug.Log(
                $"{DebugPrefix} NotifyDialogClosed activeBridge='{_activeBridge?.name ?? "null"}' activeDialogId='{_activeDialogId ?? "null"}' pendingDialog='{_pendingDialogId ?? "null"}'");
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
            if (outcome == default)
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

            if (_registry.TryGetValue(_pendingDialogId, out var bridge))
            {
                _activeBridge = bridge;
                _activeDialogId = _pendingDialogId;
            }

            _pendingDialogId = null;
        }

        private void ActivateBridge(string dialogId)
        {
            if (string.IsNullOrEmpty(dialogId))
            {
                return;
            }

            ForceActivate(dialogId);
        }

        private void HandlePlayerChanged()
        {
            var playerMovementController = _localPlayerProvider?.MovementController;
            foreach (var bridge in _registry.Values)
            {
                bridge?.ConfigurePlayer(playerMovementController);
            }
        }

        public void ConfigureDialogPlayer(string dialogId, Transform playerTransform)
        {
            if (string.IsNullOrEmpty(dialogId))
                return;

            if (_registry.TryGetValue(dialogId, out var bridge))
            {
                var movementController = playerTransform != null
                    ? playerTransform.GetComponent<PlayerMovementController>()
                    : null;

                bridge.ConfigurePlayer(movementController ?? _localPlayerProvider?.MovementController);
            }
        }
    }
}
