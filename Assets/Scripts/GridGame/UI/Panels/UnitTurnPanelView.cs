using System.Collections.Generic;
using UnityEngine;
using Zenject;
using UniRx;

public class UnitTurnPanelView : MonoBehaviour
{
    [SerializeField] private UnitIconController unitIconPrefab;
    [SerializeField] private Transform parent;
    [SerializeField] private UnitIconController activeCharacterController;

    private readonly Dictionary<UnitPortraitViewModel, UnitIconController> _iconBindings = new();
    private readonly List<UnitIconController> _icons = new();
    private readonly CompositeDisposable _disposables = new();

    private UnitTurnPanelViewModel _viewModel;
    private UnitPortraitViewModel _currentActive;

    [Inject]
    public void Initialize(UnitTurnPanelViewModel viewModel)
    {
        _viewModel = viewModel;

        _viewModel.PortraitEnqueued += OnPortraitEnqueued;
        _viewModel.PortraitDequeued += OnPortraitDequeued;
        _viewModel.PortraitRemoved += OnPortraitRemoved;

        _viewModel.ActivePortrait
            .Subscribe(UpdateActiveCharacter)
            .AddTo(_disposables);

        foreach (var portrait in _viewModel.TurnQueue)
        {
            OnPortraitEnqueued(portrait);
        }
    }

    private void OnPortraitEnqueued(UnitPortraitViewModel portrait)
    {
        if (portrait == null || unitIconPrefab == null || parent == null)
        {
            return;
        }

        if (_iconBindings.ContainsKey(portrait))
        {
            return;
        }

        var icon = Instantiate(unitIconPrefab, parent);
        icon.Bind(portrait);
        _icons.Add(icon);
        _iconBindings[portrait] = icon;
    }

    private void OnPortraitDequeued(UnitPortraitViewModel portrait)
    {
        RemoveIcon(portrait);
    }

    private void OnPortraitRemoved(UnitPortraitViewModel portrait)
    {
        RemoveIcon(portrait);
        if (_currentActive == portrait && activeCharacterController != null)
        {
            activeCharacterController.Unbind();
            _currentActive = null;
        }
    }

    private void RemoveIcon(UnitPortraitViewModel portrait)
    {
        if (portrait == null)
        {
            return;
        }

        if (_iconBindings.TryGetValue(portrait, out var icon))
        {
            _iconBindings.Remove(portrait);
            _icons.Remove(icon);
            icon.Unbind();
            Destroy(icon.gameObject);
        }
    }

    private void UpdateActiveCharacter(UnitPortraitViewModel portrait)
    {
        _currentActive = portrait;
        if (activeCharacterController == null)
        {
            return;
        }

        if (portrait == null)
        {
            activeCharacterController.Unbind();
            return;
        }

        activeCharacterController.Bind(portrait);
        RemoveIcon(portrait);
    }

    private void OnDestroy()
    {
        _disposables.Dispose();

        if (_viewModel != null)
        {
            _viewModel.PortraitEnqueued -= OnPortraitEnqueued;
            _viewModel.PortraitDequeued -= OnPortraitDequeued;
            _viewModel.PortraitRemoved -= OnPortraitRemoved;
        }

        foreach (var icon in _icons)
        {
            if (icon != null)
            {
                icon.Unbind();
                Destroy(icon.gameObject);
            }
        }

        if (activeCharacterController != null)
        {
            activeCharacterController.Unbind();
        }
    }
}
