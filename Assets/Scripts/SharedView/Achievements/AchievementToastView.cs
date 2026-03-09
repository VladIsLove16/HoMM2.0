using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;
using Zenject;

namespace Game.Achievements
{
    public sealed class AchievementToastView : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private TMP_Text rewardText;
        [SerializeField] private Image iconImage;

        [Header("Timing")]
        [SerializeField] private float showDurationSeconds = 3f;
        [SerializeField] private bool useUnscaledTime = true;

        [Header("Audio")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioClip unlockSfx;

        private readonly Queue<AchievementUnlockResult> _queue = new();
        private IAchievementService _service;
        private Coroutine _activeRoutine;

        [Inject]
        public void Construct(IAchievementService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _service.AchievementUnlocked += OnAchievementUnlocked;
        }

        private void Awake()
        {
            SetVisible(false);
        }

        private void OnDestroy()
        {
            if (_service != null)
            {
                _service.AchievementUnlocked -= OnAchievementUnlocked;
            }
        }

        private void OnAchievementUnlocked(AchievementUnlockResult result)
        {
            _queue.Enqueue(result);
            if (_activeRoutine == null)
            {
                _activeRoutine = StartCoroutine(ShowQueue());
            }
        }

        private IEnumerator ShowQueue()
        {
            while (_queue.Count > 0)
            {
                var result = _queue.Dequeue();
                ApplyResult(result);
                PlaySfx();
                SetVisible(true);
                if (useUnscaledTime)
                {
                    yield return new WaitForSecondsRealtime(showDurationSeconds);
                }
                else
                {
                    yield return new WaitForSeconds(showDurationSeconds);
                }
                SetVisible(false);
            }

            _activeRoutine = null;
        }

        private void ApplyResult(AchievementUnlockResult result)
        {
            var definition = result.Definition;
            if (definition == null)
                return;

            if (titleText != null)
                titleText.text = ResolveLocalized(definition.TitleLocalized, definition.Title);
            if (descriptionText != null)
                descriptionText.text = ResolveLocalized(definition.DescriptionLocalized, definition.Description);
            if (rewardText != null)
                rewardText.text = result.RewardCurrency > 0 ? $"+{result.RewardCurrency}" : string.Empty;
            if (iconImage != null)
                iconImage.sprite = definition.Icon;
        }

        private void SetVisible(bool visible)
        {
            if (root != null)
                root.SetActive(visible);
            else
                gameObject.SetActive(visible);
        }

        private void PlaySfx()
        {
            if (sfxSource == null || unlockSfx == null)
                return;
            sfxSource.PlayOneShot(unlockSfx);
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
