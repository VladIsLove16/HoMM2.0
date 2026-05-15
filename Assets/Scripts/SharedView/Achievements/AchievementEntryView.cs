using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game.Achievements
{
    public sealed class AchievementEntryView : MonoBehaviour
    {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private TMP_Text progressText;
        [SerializeField] private TMP_Text rewardText;
        [SerializeField] private TMP_Text categoryText;
        [SerializeField] private Image iconImage;
        [SerializeField] private Slider progressSlider;
        [SerializeField] private GameObject unlockedIndicator;
        [SerializeField] private GameObject secretOverlay;
        [SerializeField] private Sprite secretIcon;

        private AchievementEntryViewModel _viewModel;
        private readonly CompositeDisposable _subscriptions = new CompositeDisposable();
        private Sprite _originalIcon;

        public void Bind(AchievementEntryViewModel viewModel)
        {
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            _subscriptions.Clear();
            _viewModel = viewModel;

            _originalIcon = viewModel.Icon;
            ApplyIcon(viewModel.Icon);

            _subscriptions.Add(viewModel.Title.Subscribe(text => { if (titleText != null) titleText.text = text; }));
            _subscriptions.Add(viewModel.Description.Subscribe(text => { if (descriptionText != null) descriptionText.text = text; }));
            _subscriptions.Add(viewModel.ProgressText.Subscribe(text => { if (progressText != null) progressText.text = text; }));
            _subscriptions.Add(viewModel.Progress01.Subscribe(value =>
            {
                if (progressSlider != null)
                {
                    progressSlider.value = value;
                }
            }));
            _subscriptions.Add(viewModel.IsUnlocked.Subscribe(isUnlocked =>
            {
                if (unlockedIndicator != null)
                    unlockedIndicator.SetActive(isUnlocked);
            }));
            _subscriptions.Add(viewModel.Category.Subscribe(text => { if (categoryText != null) categoryText.text = text; }));
            _subscriptions.Add(viewModel.IsSecretLocked.Subscribe(isSecret =>
            {
                if (secretOverlay != null)
                {
                    secretOverlay.SetActive(isSecret);
                }

                if (iconImage != null)
                {
                    if (isSecret && secretIcon != null)
                    {
                        iconImage.sprite = secretIcon;
                    }
                    else
                    {
                        iconImage.sprite = _originalIcon;
                    }
                }
            }));

            if (rewardText != null)
            {
                rewardText.text = viewModel.RewardCurrency > 0 ? $"+{viewModel.RewardCurrency}" : string.Empty;
            }
        }

        public void Setup(
            TMP_Text title,
            TMP_Text description,
            TMP_Text progress,
            TMP_Text reward,
            TMP_Text category,
            Image icon,
            Slider slider,
            GameObject unlocked,
            GameObject overlay,
            Sprite secret)
        {
            titleText = title;
            descriptionText = description;
            progressText = progress;
            rewardText = reward;
            categoryText = category;
            iconImage = icon;
            progressSlider = slider;
            unlockedIndicator = unlocked;
            secretOverlay = overlay;
            secretIcon = secret;
        }

        private void ApplyIcon(Sprite icon)
        {
            if (iconImage != null)
            {
                iconImage.sprite = icon;
            }
        }

        private void OnDisable()
        {
            _subscriptions.Clear();
        }
    }
}
