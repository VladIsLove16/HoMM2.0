using System.Collections.Generic;
using Adventure.Integration.Battle;
using UnityEngine;

namespace Adventure.Infrastructure.State
{
    public enum BattleOutcome
    {
        Unknown = 0,
        Victory = 1,
        Defeat = 2
    }

    public static class AdventureStateCache
    {
        private const float PositionPrecision = 100f;

        private static readonly HashSet<Vector3Int> _collectedMushrooms = new HashSet<Vector3Int>();
        private static List<UnitStackData> _inventorySnapshot;
        private static Vector3? _playerPosition;
        private static Quaternion? _playerRotation;
        private static Loader.Scene? _returnScene;

        private static string _pendingDialogId;
        private static ArmyLineupSO _pendingDialogLineup;
        private static string _pendingVictoryNodeId;
        private static string _pendingDefeatNodeId;
        private static string _pendingResumeNodeId;
        private static BattleOutcome _pendingOutcome = BattleOutcome.Unknown;

        public static void CapturePlayerTransform(Transform playerTransform)
        {
            if (playerTransform == null)
                return;

            _playerPosition = playerTransform.position;
            _playerRotation = playerTransform.rotation;
        }

        public static bool TryGetPlayerTransform(out Vector3 position, out Quaternion rotation)
        {
            if (_playerPosition.HasValue)
            {
                position = _playerPosition.Value;
                rotation = _playerRotation ?? Quaternion.identity;
                return true;
            }

            position = default;
            rotation = default;
            return false;
        }

        public static void StoreInventorySnapshot(IReadOnlyList<UnitStackData> units)
        {
            if (units == null)
            {
                _inventorySnapshot = null;
                return;
            }

            _inventorySnapshot = new List<UnitStackData>(units.Count);
            for (int i = 0; i < units.Count; i++)
            {
                var stack = units[i];
                _inventorySnapshot.Add(new UnitStackData(stack.UnitType, stack.Amount));
            }
        }

        public static bool TryGetInventorySnapshot(out IReadOnlyList<UnitStackData> units)
        {
            units = _inventorySnapshot;
            return _inventorySnapshot != null;
        }

        public static void RegisterCollectedMushroom(Vector3 position)
        {
            _collectedMushrooms.Add(Quantize(position));
        }

        public static bool IsMushroomCollected(Vector3 position)
        {
            return _collectedMushrooms.Contains(Quantize(position));
        }

        public static void SetReturnScene(Loader.Scene scene)
        {
            _returnScene = scene;
        }

        public static Loader.Scene GetReturnSceneOrDefault()
        {
            return _returnScene ?? Loader.Scene.Adventure;
        }

        public static void ScheduleBattleDialog(string dialogId, ArmyLineupSO lineup, string victoryNodeId, string defeatNodeId)
        {
            _pendingDialogId = dialogId;
            _pendingDialogLineup = lineup;
            _pendingVictoryNodeId = string.IsNullOrEmpty(victoryNodeId) ? null : victoryNodeId;
            _pendingDefeatNodeId = string.IsNullOrEmpty(defeatNodeId) ? _pendingVictoryNodeId : defeatNodeId;
            _pendingResumeNodeId = null;
            _pendingOutcome = BattleOutcome.Unknown;
        }

        public static void CompleteBattle(bool playerWon)
        {
            _pendingOutcome = playerWon ? BattleOutcome.Victory : BattleOutcome.Defeat;
            var targetNode = playerWon ? _pendingVictoryNodeId : _pendingDefeatNodeId ?? _pendingVictoryNodeId;
            _pendingResumeNodeId = targetNode;
        }

        public static bool TryConsumePendingDialog(out string dialogId, out string resumeNodeId, out ArmyLineupSO lineup, out BattleOutcome outcome)
        {
            dialogId = _pendingDialogId;
            resumeNodeId = _pendingResumeNodeId;
            lineup = _pendingDialogLineup;
            outcome = _pendingOutcome;

            if (string.IsNullOrEmpty(dialogId) || string.IsNullOrEmpty(resumeNodeId))
            {
                dialogId = null;
                resumeNodeId = null;
                lineup = null;
                outcome = BattleOutcome.Unknown;
                return false;
            }

            _pendingDialogId = null;
            _pendingDialogLineup = null;
            _pendingVictoryNodeId = null;
            _pendingDefeatNodeId = null;
            _pendingResumeNodeId = null;
            _pendingOutcome = BattleOutcome.Unknown;
            return true;
        }

        private static Vector3Int Quantize(Vector3 position)
        {
            return new Vector3Int(
                Mathf.RoundToInt(position.x * PositionPrecision),
                Mathf.RoundToInt(position.y * PositionPrecision),
                Mathf.RoundToInt(position.z * PositionPrecision));
        }
    }
}
