using System.Collections.Generic;
using System.Linq;
using Adventure.Infrastructure.Persistence;
using Adventure.Integration.Battle;
using UnityEngine;

namespace Adventure.Infrastructure.State
{

    public static class BattleStateCache
    {
        private const float PositionPrecision = 100f;

        private static readonly HashSet<Vector3Int> _collectedMushrooms = new HashSet<Vector3Int>();
        private static readonly HashSet<string> _claimedBattleRewardIds = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
        private static List<UnitStackData> _inventorySnapshot;
        private static Vector3? _playerPosition;
        private static Quaternion? _playerRotation;
        private static SceneLoader.Scene? _returnScene;
        private static IDataRepository<GameStateSaveData> _gameStateRepository;
        private static bool _clearClaimedBattleRewardsOnNextPersistenceLoad;

        private static string _pendingDialogId;
        private static ArmyLineupSO _pendingDialogLineup;
        private static string _pendingVictoryNodeId;
        private static string _pendingDefeatNodeId;
        private static string _pendingFallbackNodeId;
        private static string _pendingResumeNodeId;
        private static string _pendingReturnNpcKey;
        private static bool _pendingInitialBattleReturnNpcDialog;
        private static ArmyLineupSO _pendingInitialBattleReturnNpcLineup;
        private static bool _initialBattleReturnNpcDialogConsumed;
        private static BattleOutcome _pendingOutcome = BattleOutcome.Unknown;
        private static List<UnitStackData> _pendingVictoryReward;
        private static string _pendingVictoryRewardId;

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
                PersistGameState();
                return;
            }

            _inventorySnapshot = new List<UnitStackData>(units.Count);
            for (int i = 0; i < units.Count; i++)
            {
                var stack = units[i];
                _inventorySnapshot.Add(new UnitStackData(stack.UnitType, stack.Amount));
            }

            PersistGameState();
        }

        public static bool TryGetInventorySnapshot(out IReadOnlyList<UnitStackData> units)
        {
            units = _inventorySnapshot;
            return _inventorySnapshot != null;
        }

        public static void RegisterCollectedMushroom(Vector3 position)
        {
            _collectedMushrooms.Add(Quantize(position));
            PersistGameState();
        }

        public static bool IsMushroomCollected(Vector3 position)
        {
            return _collectedMushrooms.Contains(Quantize(position));
        }

        public static void SetReturnScene(SceneLoader.Scene scene)
        {
            _returnScene = scene;
        }

        public static SceneLoader.Scene GetReturnSceneOrDefault()
        {
            return _returnScene ?? SceneLoader.Scene.Adventure;
        }

        public static void ScheduleBattleDialog(
            string dialogId,
            ArmyLineupSO lineup,
            string victoryNodeId,
            string defeatNodeId,
            string returnNpcKey = null,
            string fallbackNodeId = null)
        {
            ClearPendingInitialBattleReturnNpcDialog();
            _pendingDialogId = dialogId;
            _pendingDialogLineup = lineup;
            _pendingVictoryNodeId = NormalizeKey(victoryNodeId);
            _pendingDefeatNodeId = NormalizeKey(defeatNodeId);
            _pendingFallbackNodeId = NormalizeKey(fallbackNodeId);
            _pendingResumeNodeId = null;
            _pendingReturnNpcKey = NormalizeKey(returnNpcKey);
            _pendingOutcome = BattleOutcome.Unknown;
        }

        public static void ScheduleInitialBattleReturnNpcDialog(ArmyLineupSO lineup)
        {
            if (_initialBattleReturnNpcDialogConsumed)
            {
                return;
            }

            ClearPendingDialog();
            _pendingInitialBattleReturnNpcDialog = true;
            _pendingInitialBattleReturnNpcLineup = lineup;
            _pendingOutcome = BattleOutcome.Unknown;
        }

        public static bool CanScheduleInitialBattleReturnNpcDialog()
        {
            return !_initialBattleReturnNpcDialogConsumed;
        }

        public static void ResetInitialBattleReturnNpcDialogFlow()
        {
            _initialBattleReturnNpcDialogConsumed = false;
            ClearPendingInitialBattleReturnNpcDialog();
        }

        public static void ClearPendingPostBattleDialog()
        {
            ClearPendingDialog();
            ClearPendingInitialBattleReturnNpcDialog();
            _pendingOutcome = BattleOutcome.Unknown;
        }

        public static void ScheduleBattleReward(IReadOnlyList<UnitStackData> reward, string rewardId = null)
        {
            _pendingVictoryRewardId = NormalizeRewardId(rewardId);

            if (!string.IsNullOrEmpty(_pendingVictoryRewardId) && _claimedBattleRewardIds.Contains(_pendingVictoryRewardId))
            {
                _pendingVictoryReward = null;
                return;
            }

            _pendingVictoryReward = reward != null
                ? reward.Where(stack => stack.Amount > 0).Select(stack => new UnitStackData(stack.UnitType, stack.Amount)).ToList()
                : null;
        }

        public static void CompleteBattle(bool playerWon)
        {
            _pendingOutcome = playerWon ? BattleOutcome.PlayerWon : BattleOutcome.PlayerLost;
            var resultNode = playerWon ? _pendingVictoryNodeId : _pendingDefeatNodeId;
            _pendingResumeNodeId = string.IsNullOrEmpty(resultNode)
                ? _pendingFallbackNodeId
                : resultNode;

            if (string.IsNullOrEmpty(_pendingResumeNodeId))
            {
                ClearPendingDialog();
            }
        }

        public static bool TryConsumePendingDialog(out string dialogId, out string resumeNodeId, out ArmyLineupSO lineup, out BattleOutcome outcome)
        {
            return TryConsumePendingDialog(out dialogId, out resumeNodeId, out lineup, out outcome, out _);
        }

        public static bool TryConsumePendingDialog(
            out string dialogId,
            out string resumeNodeId,
            out ArmyLineupSO lineup,
            out BattleOutcome outcome,
            out string returnNpcKey)
        {
            dialogId = _pendingDialogId;
            resumeNodeId = _pendingResumeNodeId;
            lineup = _pendingDialogLineup;
            outcome = _pendingOutcome;
            returnNpcKey = _pendingReturnNpcKey;

            if (string.IsNullOrEmpty(dialogId) || string.IsNullOrEmpty(resumeNodeId))
            {
                dialogId = null;
                resumeNodeId = null;
                lineup = null;
                outcome = BattleOutcome.Unknown;
                returnNpcKey = null;
                return false;
            }

            _pendingDialogId = null;
            _pendingDialogLineup = null;
            _pendingVictoryNodeId = null;
            _pendingDefeatNodeId = null;
            _pendingFallbackNodeId = null;
            _pendingResumeNodeId = null;
            _pendingReturnNpcKey = null;
            ClearOutcomeIfNoPendingPostBattleWork();
            return true;
        }

        public static bool TryConsumePendingInitialBattleReturnNpcDialog(out ArmyLineupSO lineup, out BattleOutcome outcome)
        {
            lineup = _pendingInitialBattleReturnNpcLineup;
            outcome = _pendingOutcome;

            if (!_pendingInitialBattleReturnNpcDialog || _pendingOutcome == BattleOutcome.Unknown)
            {
                lineup = null;
                outcome = BattleOutcome.Unknown;
                return false;
            }

            ClearPendingInitialBattleReturnNpcDialog();
            _initialBattleReturnNpcDialogConsumed = true;
            ClearOutcomeIfNoPendingPostBattleWork();
            return true;
        }

        public static bool TryConsumePendingBattleReward(out IReadOnlyList<UnitStackData> reward)
        {
            reward = null;

            if (_pendingOutcome == BattleOutcome.Unknown)
            {
                return false;
            }

            if (_pendingOutcome != BattleOutcome.PlayerWon || _pendingVictoryReward == null || _pendingVictoryReward.Count == 0)
            {
                _pendingVictoryReward = null;
                _pendingVictoryRewardId = null;
                ClearOutcomeIfNoPendingPostBattleWork();
                return false;
            }

            if (!string.IsNullOrEmpty(_pendingVictoryRewardId))
            {
                if (!_claimedBattleRewardIds.Add(_pendingVictoryRewardId))
                {
                    _pendingVictoryReward = null;
                    _pendingVictoryRewardId = null;
                    ClearOutcomeIfNoPendingPostBattleWork();
                    return false;
                }

                PersistGameState();
            }

            reward = _pendingVictoryReward;
            _pendingVictoryReward = null;
            _pendingVictoryRewardId = null;
            ClearOutcomeIfNoPendingPostBattleWork();
            return true;
        }

        public static bool HasPendingPostBattleFlow()
        {
            return HasPendingPostBattleDialog() || HasPendingBattleReward();
        }

        public static bool HasPendingRegularPostBattleDialog()
        {
            return !string.IsNullOrEmpty(_pendingDialogId);
        }

        public static bool HasPendingInitialBattleReturnNpcDialog()
        {
            return _pendingInitialBattleReturnNpcDialog;
        }

        public static bool HasPendingBattleReward()
        {
            return _pendingVictoryReward != null && _pendingVictoryReward.Count > 0;
        }

        public static void ClearClaimedBattleRewards()
        {
            _claimedBattleRewardIds.Clear();
            if (_gameStateRepository == null)
            {
                _clearClaimedBattleRewardsOnNextPersistenceLoad = true;
                return;
            }

            _clearClaimedBattleRewardsOnNextPersistenceLoad = false;
            PersistGameState();
        }

        internal static void ConfigurePersistence(IDataRepository<GameStateSaveData> repository)
        {
            _gameStateRepository = repository;
            LoadGameState();
            if (_clearClaimedBattleRewardsOnNextPersistenceLoad)
            {
                _claimedBattleRewardIds.Clear();
                _clearClaimedBattleRewardsOnNextPersistenceLoad = false;
                PersistGameState();
            }
        }

        private static void LoadGameState()
        {
            if (_gameStateRepository == null)
                return;

            var data = _gameStateRepository.Load() ?? new GameStateSaveData();

            _collectedMushrooms.Clear();
            _claimedBattleRewardIds.Clear();
            if (data.CollectedMushrooms != null)
            {
                foreach (var record in data.CollectedMushrooms)
                {
                    _collectedMushrooms.Add(new Vector3Int(record.X, record.Y, record.Z));
                }
            }

            if (data.Inventory != null && data.Inventory.Count > 0)
            {
                _inventorySnapshot = data.Inventory
                    .Select(record => new UnitStackData(record.UnitType, record.Amount))
                    .ToList();
            }
            else
            {
                _inventorySnapshot = null;
            }

            if (data.ClaimedBattleRewardIds != null)
            {
                foreach (var rewardId in data.ClaimedBattleRewardIds)
                {
                    var normalized = NormalizeRewardId(rewardId);
                    if (!string.IsNullOrEmpty(normalized))
                    {
                        _claimedBattleRewardIds.Add(normalized);
                    }
                }
            }
        }

        private static void PersistGameState()
        {
            if (_gameStateRepository == null)
                return;

            var existing = _gameStateRepository.Load() ?? new GameStateSaveData();
            var snapshot = new GameStateSaveData
            {
                Currency = existing.Currency,
                Inventory = _inventorySnapshot != null
                    ? _inventorySnapshot.Select(stack => new UnitStackRecord
                    {
                        UnitType = stack.UnitType,
                        Amount = stack.Amount
                    }).ToList()
                    : new List<UnitStackRecord>(),
                CollectedMushrooms = _collectedMushrooms
                    .Select(Vector3IntRecord.From)
                    .ToList(),
                ClaimedBattleRewardIds = _claimedBattleRewardIds
                    .ToList()
            };

            _gameStateRepository.Save(snapshot);
        }

        private static Vector3Int Quantize(Vector3 position)
        {
            return new Vector3Int(
                Mathf.RoundToInt(position.x * PositionPrecision),
                Mathf.RoundToInt(position.y * PositionPrecision),
                Mathf.RoundToInt(position.z * PositionPrecision));
        }

        private static string NormalizeRewardId(string rewardId)
        {
            return string.IsNullOrWhiteSpace(rewardId) ? null : rewardId.Trim();
        }

        private static string NormalizeKey(string key)
        {
            return string.IsNullOrWhiteSpace(key) ? null : key.Trim();
        }

        private static void ClearPendingDialog()
        {
            _pendingDialogId = null;
            _pendingDialogLineup = null;
            _pendingVictoryNodeId = null;
            _pendingDefeatNodeId = null;
            _pendingFallbackNodeId = null;
            _pendingResumeNodeId = null;
            _pendingReturnNpcKey = null;
        }

        private static void ClearPendingInitialBattleReturnNpcDialog()
        {
            _pendingInitialBattleReturnNpcDialog = false;
            _pendingInitialBattleReturnNpcLineup = null;
        }

        private static void ClearOutcomeIfNoPendingPostBattleWork()
        {
            if (HasPendingPostBattleDialog() || HasPendingBattleReward())
                return;

            _pendingOutcome = BattleOutcome.Unknown;
        }

        private static bool HasPendingPostBattleDialog()
        {
            return (!string.IsNullOrEmpty(_pendingDialogId) && !string.IsNullOrEmpty(_pendingResumeNodeId))
                || _pendingInitialBattleReturnNpcDialog;
        }

    }
}
