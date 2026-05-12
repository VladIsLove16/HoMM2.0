using System.Collections.Generic;
using Adventure.Application.Rewards;
using SharedView;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Adventure.Presentation.Rewards
{
    public sealed class BattleRewardPopupView : CanvasGroupVisibilityPanelBase
    {
        [SerializeField] private Transform itemsRoot;
        [SerializeField] private BattleRewardItemView rewardItemPrefab;
        [SerializeField] private Button closeButton;

        private readonly CompositeDisposable _subscriptions = new();
        private BattleRewardPopupViewModel _viewModel;

        protected override void Awake()
        {
            base.Awake();
            closeButton?.onClick.AddListener(Close);
        }

        protected override void OnDestroy()
        {
            closeButton?.onClick.RemoveListener(Close);
            _subscriptions.Dispose();
            base.OnDestroy();
        }

        [Inject]
        public void Construct(BattleRewardPopupViewModel viewModel)
        {
            _viewModel = viewModel;
            _viewModel.IsOpen
                .Subscribe(OnOpenStateChanged)
                .AddTo(_subscriptions);
        }

        private void OnOpenStateChanged(bool isOpen)
        {
            if (isOpen)
            {
                Show(_viewModel.Rewards);
            }
            else
            {
                HidePanelImmediate();
            }
        }

        private void Show(IReadOnlyList<BattleRewardItemViewData> rewards)
        {
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            transform.SetAsLastSibling();
            ClearItems();

            if (rewards != null)
            {
                foreach (var reward in rewards)
                {
                    CreateItem(reward);
                }
            }

            if (itemsRoot is RectTransform itemsRect)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(itemsRect);
            }

            ShowPanelImmediate();
        }

        private void Close()
        {
            if (_viewModel != null)
            {
                _viewModel.Close();
                return;
            }

            HidePanelImmediate();
        }

        private void CreateItem(BattleRewardItemViewData reward)
        {
            if (itemsRoot == null || rewardItemPrefab == null)
            {
                Debug.LogWarning("[BattleRewardPopupView] Items root or reward item prefab is not assigned.", this);
                return;
            }

            var item = Instantiate(rewardItemPrefab, itemsRoot, false);
            item.name = reward.UnitType.ToString();
            item.Bind(reward);
        }

        private void ClearItems()
        {
            if (itemsRoot == null)
            {
                return;
            }

            for (var i = itemsRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(itemsRoot.GetChild(i).gameObject);
            }
        }
    }
}
