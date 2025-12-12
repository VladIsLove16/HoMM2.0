using Adventure.Infrastructure.Movement;
using Adventure.Infrastructure.State;
using System.Collections.Generic;
using UnityEngine;

namespace Adventure.Infrastructure.Dialog
{
    public sealed class NpcBehaviorGraphRegistry
    {
        private readonly Transform _playerTransform;
        private readonly Dictionary<string, NpcBehaviorGraphBridge> _registry = new();

        private string _pendingDialogId;
        private NpcBehaviorGraphBridge _activeBridge;

        public NpcBehaviorGraphRegistry(PlayerMovementController playerMovementController)
        {
            _playerTransform = playerMovementController != null
                ? playerMovementController.transform
                : null;
        }

        public void Register(string dialogId, NpcBehaviorGraphBridge bridge)
        {
            if (string.IsNullOrEmpty(dialogId) || bridge == null)
            {
                return;
            }

            _registry[dialogId] = bridge;
            bridge.AssignDialogueId(dialogId);
            bridge.ConfigurePlayer(_playerTransform);
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
            }

            if (_activeBridge == bridge)
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

            if (_registry.TryGetValue(dialogId, out var bridge))
            {
                _activeBridge = bridge;
            }
        }

        public void NotifyDialogOpened()
        {
            EnsureActiveBridge();
            _activeBridge?.NotifyDialogOpened();
        }

        public void NotifyDialogClosed()
        {
            _activeBridge?.NotifyDialogClosed();
            _activeBridge = null;
        }

        public void NotifyChoiceWindowState(bool isOpen)
        {
            _activeBridge?.NotifyChoiceWindowState(isOpen);
        }

        public void NotifyBattleRequested()
        {
            _activeBridge?.NotifyBattleRequested();
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

            if (_registry.TryGetValue(_pendingDialogId, out var bridge))
            {
                _activeBridge = bridge;
            }

            _pendingDialogId = null;
        }
    }
}
