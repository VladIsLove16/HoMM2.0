using System;
using System.Collections.Generic;
using System.Linq;
using Adventure.Domain.Inventory;
using Adventure.Infrastructure.State;
using CustomEventBus;
using Game.Events;
using UnityEngine;

namespace Adventure.Application.Rewards
{
    public sealed class BattleRewardService
    {
        private readonly MushroomInventoryModel _inventory;
        private readonly AdventureMushroomAssetMap _unitDefinitions;
        private readonly BattleRewardPopupViewModel _rewardPopup;
        private readonly EventBus _gameplayEvents;

        public BattleRewardService(
            MushroomInventoryModel inventory,
            AdventureMushroomAssetMap unitDefinitions,
            BattleRewardPopupViewModel rewardPopup,
            EventBus gameplayEvents)
        {
            _inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            _unitDefinitions = unitDefinitions;
            _rewardPopup = rewardPopup ?? throw new ArgumentNullException(nameof(rewardPopup));
            _gameplayEvents = gameplayEvents;
        }

        public bool TryShowPendingReward(Action onClosed)
        {
            if (!BattleStateCache.TryConsumePendingBattleReward(out var reward))
                return false;

            var viewData = ApplyReward(reward);
            if (viewData.Count == 0)
                return false;

            _rewardPopup.Open(viewData, onClosed);
            return true;
        }

        private List<BattleRewardItemViewData> ApplyReward(IReadOnlyList<UnitStackData> reward)
        {
            var result = new List<BattleRewardItemViewData>();
            if (reward == null)
                return result;

            foreach (var stack in reward)
            {
                if (stack.Amount <= 0)
                    continue;

                _inventory.Add(stack.UnitType, stack.Amount);
                PublishCollectedEvent(stack.UnitType);

                result.Add(CreateViewData(stack));
            }

            return result;
        }

        private BattleRewardItemViewData CreateViewData(UnitStackData stack)
        {
            string displayName = stack.UnitType.ToString();
            Sprite icon = null;

            if (_unitDefinitions != null && _unitDefinitions.TryGetDefinition(stack.UnitType, out var definition) && definition != null)
            {
                var shared = definition.SharedData;
                if (shared != null)
                {
                    if (!string.IsNullOrWhiteSpace(shared.DisplayName))
                    {
                        displayName = shared.DisplayName;
                    }

                    icon = shared.Icon;
                }
            }

            return new BattleRewardItemViewData(stack.UnitType, stack.Amount, displayName, icon);
        }

        private void PublishCollectedEvent(UnitType unitType)
        {
            var totals = _inventory.Items?.ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value)
                ?? new Dictionary<string, int>();
            _gameplayEvents?.Invoke(new MushroomCollectedCustomEvent(unitType.ToString(), totals));
        }
    }
}
