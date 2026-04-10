using Adventure.Infrastructure.Movement;
using Adventure.Infrastructure.Players;
using Adventure.Infrastructure.State;
using System.Collections.Generic;
using UnityEngine;

namespace Adventure.Infrastructure.Dialog
{
    public sealed class NpcBehaviorGraphRegistry
    {
        private readonly ILocalAdventurePlayerProvider _localPlayerProvider;
        private readonly Dictionary<string, NpcBehaviorGraphBridge> _registry = new();

        private string _pendingDialogId;
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
            bridge.ConfigurePlayer(_localPlayerProvider?.PlayerTransform);
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
            }

            _pendingDialogId = null;
        }

        private void HandlePlayerChanged()
        {
            var playerTransform = _localPlayerProvider?.PlayerTransform;
            foreach (var bridge in _registry.Values)
            {
                bridge?.ConfigurePlayer(playerTransform);
            }
        }

        public void ConfigureDialogPlayer(string dialogId, Transform playerTransform)
        {
            if (string.IsNullOrEmpty(dialogId) || playerTransform == null)
                return;

            if (_registry.TryGetValue(dialogId, out var bridge))
            {
                bridge.ConfigurePlayer(playerTransform);
            }
        }
    }
}
