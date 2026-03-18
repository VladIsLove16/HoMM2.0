using System.Collections.Generic;
using DG.Tweening;
using UniRx;
using UnityEngine;
using Zenject;

public class UnitTurnPanelView : MonoBehaviour
{
    [SerializeField] private UnitIconController unitIconPrefab;
    [SerializeField] private RectTransform parent;
    [SerializeField] private bool enableOnStart = true;
    [SerializeField] private float padding = 15f;
    [SerializeField] private float moveDuration = 0.25f;
    [SerializeField] private Ease moveEase = Ease.OutCubic;
    [SerializeField] private float activeScale = 1.2f;
    [SerializeField] private float inactiveScale = 1f;
    [SerializeField, Min(1)] private int lookAheadTurns = 3;

    private readonly List<UnitIconController> _displayIcons = new();
    private readonly List<UnitPortraitViewModel> _displayPortraits = new();
    private readonly CompositeDisposable _disposables = new();

    private UnitTurnPanelViewModel _viewModel;
    private UnitPortraitViewModel _currentActive;
    private float _iconWidth;
    private Sequence _advanceSequence;
    private bool _initialized;

    [Inject]
    public void Initialize(UnitTurnPanelViewModel viewModel)
    {
        _viewModel = viewModel;
        _viewModel.PortraitEnqueued += OnPortraitEnqueued;
        _viewModel.PortraitDequeued += OnPortraitDequeued;
        _viewModel.PortraitRemoved += OnPortraitRemoved;

        _viewModel.ActivePortrait
            .Subscribe(OnActivePortraitChanged)
            .AddTo(_disposables);

        ToggleStartingVisibility();
    }

    private void ToggleStartingVisibility()
    {
        if (parent == null)
            return;

        parent.gameObject.SetActive(enableOnStart);
    }

    private void OnPortraitEnqueued(UnitPortraitViewModel _)
    {
        if (!_initialized && _viewModel.ActivePortrait.Value != null)
        {
            RebuildImmediate();
        }
    }

    private void OnPortraitDequeued(UnitPortraitViewModel _)
    {
        if (!_initialized && _viewModel.ActivePortrait.Value != null)
        {
            RebuildImmediate();
        }
    }

    private void OnPortraitRemoved(UnitPortraitViewModel portrait)
    {
        if (portrait == null)
            return;

        if (_currentActive == portrait)
        {
            _currentActive = null;
        }

        if (_displayPortraits.Contains(portrait))
        {
            RebuildImmediate();
        }
    }

    private void OnActivePortraitChanged(UnitPortraitViewModel nextActive)
    {
        if (nextActive == null)
        {
            _currentActive = null;
            RebuildImmediate();
            return;
        }

        if (!_initialized || _displayIcons.Count == 0 || _currentActive == null)
        {
            _currentActive = nextActive;
            RebuildImmediate();
            return;
        }

        if (_currentActive == nextActive)
            return;

        var previousActive = _currentActive;
        _currentActive = nextActive;
        AnimateAdvance(previousActive);
    }

    private void AnimateAdvance(UnitPortraitViewModel previousActive)
    {
        var desired = BuildDesiredPortraits();
        if (desired.Count == 0 ||
            _displayIcons.Count == 0 ||
            _displayPortraits.Count != desired.Count ||
            _displayPortraits[0] != previousActive)
        {
            RebuildImmediate();
            return;
        }

        EnsureIconWidth();
        KillAdvanceSequence();

        var appendedPortrait = desired[^1];
        var appendedIcon = CreateIcon(appendedPortrait);
        if (appendedIcon == null)
        {
            RebuildImmediate();
            return;
        }

        _displayIcons.Add(appendedIcon);
        _displayPortraits.Add(appendedPortrait);

        var appendedRect = appendedIcon.GetComponent<RectTransform>();
        EnsureRectSetup(appendedRect);
        if (appendedRect != null)
        {
            appendedRect.anchoredPosition = new Vector2(_displayIcons.Count * GetStep(), appendedRect.anchoredPosition.y);
            appendedRect.localScale = Vector3.one * inactiveScale;
        }

        _advanceSequence = DOTween.Sequence();
        for (var i = 0; i < _displayIcons.Count; i++)
        {
            var icon = _displayIcons[i];
            if (icon == null)
                continue;

            var rect = icon.GetComponent<RectTransform>();
            EnsureRectSetup(rect);
            if (rect == null)
                continue;

            var targetIndex = i - 1;
            var targetPosition = new Vector2(targetIndex * GetStep(), rect.anchoredPosition.y);
            var targetScale = i == 1 ? activeScale : inactiveScale;

            _advanceSequence.Join(rect.DOAnchorPos(targetPosition, moveDuration).SetEase(moveEase));
            _advanceSequence.Join(rect.DOScale(targetScale, moveDuration).SetEase(moveEase));
        }

        _advanceSequence.OnComplete(() =>
        {
            if (_displayIcons.Count == 0)
                return;

            var leavingIcon = _displayIcons[0];
            if (leavingIcon != null)
            {
                leavingIcon.Unbind();
                Destroy(leavingIcon.gameObject);
            }

            _displayIcons.RemoveAt(0);
            _displayPortraits.Clear();
            _displayPortraits.AddRange(desired);
            LayoutCurrentIcons(true);
        });
    }

    private void RebuildImmediate()
    {
        KillAdvanceSequence();

        var desired = BuildDesiredPortraits();
        _initialized = desired.Count > 0;

        while (_displayIcons.Count > desired.Count)
        {
            DestroyLastIcon();
        }

        while (_displayIcons.Count < desired.Count)
        {
            var icon = CreateIcon(desired[_displayIcons.Count]);
            if (icon == null)
                break;
            _displayIcons.Add(icon);
        }

        for (var i = 0; i < _displayIcons.Count; i++)
        {
            var icon = _displayIcons[i];
            var portrait = desired[i];
            if (icon == null)
                continue;

            icon.Bind(portrait);
        }

        _displayPortraits.Clear();
        _displayPortraits.AddRange(desired);
        LayoutCurrentIcons(true);
    }

    private void LayoutCurrentIcons(bool immediate)
    {
        EnsureIconWidth();
        for (var i = 0; i < _displayIcons.Count; i++)
        {
            var icon = _displayIcons[i];
            if (icon == null)
                continue;

            var rect = icon.GetComponent<RectTransform>();
            EnsureRectSetup(rect);
            if (rect == null)
                continue;

            var targetPosition = new Vector2(i * GetStep(), rect.anchoredPosition.y);
            var targetScale = i == 0 ? activeScale : inactiveScale;

            if (immediate || moveDuration <= 0f)
            {
                rect.DOKill();
                rect.anchoredPosition = targetPosition;
                rect.localScale = Vector3.one * targetScale;
                continue;
            }

            rect.DOKill();
            rect.DOAnchorPos(targetPosition, moveDuration).SetEase(moveEase);
            rect.DOScale(targetScale, moveDuration).SetEase(moveEase);
        }
    }

    private List<UnitPortraitViewModel> BuildDesiredPortraits()
    {
        var result = new List<UnitPortraitViewModel>();
        var cycle = new List<UnitPortraitViewModel>();

        if (_currentActive != null)
        {
            cycle.Add(_currentActive);
        }

        if (_viewModel != null)
        {
            foreach (var portrait in _viewModel.TurnQueue)
            {
                if (portrait != null)
                {
                    cycle.Add(portrait);
                }
            }
        }

        if (cycle.Count == 0)
            return result;

        var desiredCount = cycle.Count * Mathf.Max(1, lookAheadTurns);
        while (result.Count < desiredCount)
        {
            foreach (var portrait in cycle)
            {
                result.Add(portrait);
                if (result.Count >= desiredCount)
                    break;
            }
        }

        return result;
    }

    private UnitIconController CreateIcon(UnitPortraitViewModel portrait)
    {
        if (unitIconPrefab == null || parent == null || portrait == null)
            return null;

        var icon = Instantiate(unitIconPrefab, parent);
        var rect = icon.GetComponent<RectTransform>();
        EnsureRectSetup(rect);
        icon.Bind(portrait);
        return icon;
    }

    private void DestroyLastIcon()
    {
        var lastIndex = _displayIcons.Count - 1;
        if (lastIndex < 0)
            return;

        var icon = _displayIcons[lastIndex];
        if (icon != null)
        {
            icon.Unbind();
            Destroy(icon.gameObject);
        }

        _displayIcons.RemoveAt(lastIndex);
    }

    private void EnsureIconWidth()
    {
        if (_iconWidth > 0f)
            return;

        if (unitIconPrefab == null)
        {
            _iconWidth = 80f;
            return;
        }

        var rect = unitIconPrefab.GetComponent<RectTransform>();
        _iconWidth = rect != null && rect.rect.width > 0f ? rect.rect.width : 80f;
    }

    private float GetStep()
    {
        return _iconWidth + padding;
    }

    private void EnsureRectSetup(RectTransform rect)
    {
        if (rect == null)
            return;

        rect.anchorMin = new Vector2(0f, 0.5f);
        rect.anchorMax = new Vector2(0f, 0.5f);
        rect.pivot = new Vector2(0f, 0.5f);
    }

    private void KillAdvanceSequence()
    {
        _advanceSequence?.Kill();
        _advanceSequence = null;

        foreach (var icon in _displayIcons)
        {
            if (icon == null)
                continue;

            var rect = icon.GetComponent<RectTransform>();
            rect?.DOKill();
        }
    }

    private void OnDestroy()
    {
        KillAdvanceSequence();

        if (_viewModel != null)
        {
            _viewModel.PortraitEnqueued -= OnPortraitEnqueued;
            _viewModel.PortraitDequeued -= OnPortraitDequeued;
            _viewModel.PortraitRemoved -= OnPortraitRemoved;
        }

        _disposables.Dispose();

        foreach (var icon in _displayIcons)
        {
            if (icon == null)
                continue;

            icon.Unbind();
            Destroy(icon.gameObject);
        }

        _displayIcons.Clear();
        _displayPortraits.Clear();
    }
}
