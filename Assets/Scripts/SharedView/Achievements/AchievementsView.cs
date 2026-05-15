using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;
using Zenject;
using SharedView;

namespace Game.Achievements
{
    public sealed class AchievementsView : CanvasGroupPanelViewBase<AchievementsViewModel>
    {
        [SerializeField] private AchievementEntryView itemPrefab;
        [SerializeField] private Transform listRoot;
        [SerializeField] private Button resetButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private TMP_Text totalScoreText;
        [SerializeField] private LocalizedString totalScoreFormatLocalized;
        [SerializeField] private string totalScoreFormatFallback = "Total Score: {0}";

        [Header("Empty State")]
        [SerializeField] private GameObject emptyStateRoot;
        [SerializeField] private TMP_Text emptyStateLabel;
        [SerializeField] private LocalizedString emptyStateLocalized;
        [SerializeField, TextArea] private string emptyStateFallback = "Error loading achievements.";

        private readonly Dictionary<AchievementEntryViewModel, AchievementEntryView> _views = new();

        [Inject]
        public override void Construct(AchievementsViewModel viewModel)
        {
            base.Construct(viewModel);
        }

        public void Setup(AchievementEntryView prefab, Transform rootList, Button reset, Button close, TMP_Text totalScore = null)
        {
            itemPrefab = prefab;
            listRoot = rootList;
            resetButton = reset;
            closeButton = close;
            totalScoreText = totalScore;
        }

        public void Toggle()
        {
            ViewModel?.Toggle();
        }

        protected override void OnInitialized()
        {
            _views.Clear();

            if (resetButton != null)
            {
                resetButton.onClick.RemoveListener(OnResetClicked);
                resetButton.onClick.AddListener(OnResetClicked);
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(OnCloseClicked);
                closeButton.onClick.AddListener(OnCloseClicked);
            }

            BuildList();
            BindTotalScore();
        }

        protected override void OnDestroy()
        {
            if (resetButton != null)
            {
                resetButton.onClick.RemoveListener(OnResetClicked);
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(OnCloseClicked);
            }

            base.OnDestroy();
        }

        private void BuildList()
        {
            if (itemPrefab == null || listRoot == null || ViewModel == null)
                return;

            foreach (Transform child in listRoot)
            {
                Destroy(child.gameObject);
            }

            _views.Clear();

            foreach (var entry in ViewModel.Entries)
            {
                if (entry == null)
                    continue;

                var instance = Instantiate(itemPrefab, listRoot);
                instance.Bind(entry);
                _views[entry] = instance;
            }

            UpdateEmptyState();
        }

        private void OnResetClicked()
        {
            ViewModel?.ResetAll();
        }

        private void OnCloseClicked()
        {
            ViewModel?.Close();
        }

        private void UpdateEmptyState()
        {
            var hasEntries = _views.Count > 0;
            if (emptyStateRoot != null)
            {
                emptyStateRoot.SetActive(!hasEntries);
            }

            if (!hasEntries && emptyStateLabel != null)
            {
                emptyStateLabel.text = ResolveLocalized(emptyStateLocalized, emptyStateFallback);
            }
        }

        private void BindTotalScore()
        {
            if (ViewModel == null || totalScoreText == null)
                return;

            ViewModel.TotalScore
                .Subscribe(UpdateTotalScoreText)
                .AddTo(Bindings);

            UpdateTotalScoreText(ViewModel.TotalScore.Value);
        }

        private void UpdateTotalScoreText(int totalScore)
        {
            if (totalScoreText == null)
                return;

            var format = ResolveLocalized(totalScoreFormatLocalized, totalScoreFormatFallback);
            if (string.IsNullOrWhiteSpace(format))
                format = "{0}";

            totalScoreText.text = string.Format(format, totalScore);
        }

        private static string ResolveLocalized(LocalizedString localized, string fallback)
        {
            if (localized != null && !localized.IsEmpty)
            {
                var value = localized.GetLocalizedString();
                if (!string.IsNullOrEmpty(value))
                    return value;
            }

            return fallback ?? string.Empty;
        }
    }
}
