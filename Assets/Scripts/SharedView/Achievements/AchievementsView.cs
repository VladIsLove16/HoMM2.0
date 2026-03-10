using System;
using System.Collections.Generic;
using Game.Achievements;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using TMPro;
using UnityEngine.Localization;

namespace Game.Achievements
{
    public sealed class AchievementsView : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private AchievementEntryView itemPrefab;
        [SerializeField] private Transform listRoot;
        [SerializeField] private Button resetButton;
        [SerializeField] private Button closeButton;
        [Header("Empty State")]
        [SerializeField] private GameObject emptyStateRoot;
        [SerializeField] private TMP_Text emptyStateLabel;
        [SerializeField] private LocalizedString emptyStateLocalized;
        [SerializeField, TextArea] private string emptyStateFallback = "Ошибка загрузки достижений.";

        private AchievementsViewModel _viewModel;
        private readonly Dictionary<AchievementEntryViewModel, AchievementEntryView> _views = new();
        private readonly CompositeDisposable _subscriptions = new CompositeDisposable();

        [Inject]
        public void Construct(AchievementsViewModel viewModel)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            Bind();
        }

        public void Setup(GameObject rootObject, AchievementEntryView prefab, Transform rootList, Button reset, Button close)
        {
            root = rootObject;
            itemPrefab = prefab;
            listRoot = rootList;
            resetButton = reset;
            closeButton = close;
        }

        private void Bind()
        {
            if (_viewModel == null)
                return;

            _subscriptions.Clear();
            _views.Clear();

            _subscriptions.Add(_viewModel.IsOpen.Subscribe(isOpen =>
            {
                if (root != null)
                {
                    root.SetActive(isOpen);
                }
            }));

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
        }

        private void BuildList()
        {
            if (itemPrefab == null || listRoot == null || _viewModel == null)
                return;

            foreach (Transform child in listRoot)
            {
                Destroy(child.gameObject);
            }
            _views.Clear();

            foreach (var entry in _viewModel.Entries)
            {
                if (entry == null)
                    continue;
                var instance = Instantiate(itemPrefab, listRoot);
                instance.Bind(entry);
                _views[entry] = instance;
            }

            UpdateEmptyState();
        }

        public void Toggle()
        {
            if (_viewModel == null)
                return;
            _viewModel.Toggle();
        }

        private void OnResetClicked()
        {
            _viewModel?.ResetAll();
        }

        private void OnCloseClicked()
        {
            _viewModel?.Close();
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

        private void OnDisable()
        {
            _subscriptions.Clear();
        }
    }
}
