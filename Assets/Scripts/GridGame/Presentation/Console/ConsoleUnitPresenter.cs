using System;
using UniRx;
using UnityEngine;

/// <summary>
/// Bridges a UnitViewModel to the console grid state, keeping textual data in sync.
/// </summary>
public sealed class ConsoleUnitPresenter : IDisposable
{
    private readonly UnitViewModel _viewModel;
    private readonly ConsoleGridState _gridState;
    private readonly Action<string> _logger;
    private readonly CompositeDisposable _disposables = new();
    private bool _disposed;

    public ConsoleUnitPresenter(UnitViewModel viewModel, ConsoleGridState gridState, Action<string> logger)
    {
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _gridState = gridState ?? throw new ArgumentNullException(nameof(gridState));
        _logger = logger ?? (_ => { });

        _gridState.UpsertUnit(_viewModel);
        Subscribe();
    }

    private void Subscribe()
    {
        _viewModel.OnMoveByRoute.Subscribe(route =>
        {
            if (route == null || route.Count == 0)
            {
                return;
            }

            var destination = route[route.Count - 1];
            _gridState.MoveUnit(_viewModel, destination);
            _logger($"[ConsoleView] {_viewModel.Model.UnitType.Value} moved to {destination}.");
        }).AddTo(_disposables);

        _viewModel.Model.Position.Subscribe(position =>
        {
            _gridState.MoveUnit(_viewModel, position);
        }).AddTo(_disposables);

        _viewModel.OnHealthChanged.Subscribe(_ =>
        {
            _gridState.UpsertUnit(_viewModel);
        }).AddTo(_disposables);

        _viewModel.OnTeamChangedEnum.Subscribe(_ =>
        {
            _gridState.UpsertUnit(_viewModel);
        }).AddTo(_disposables);

        _viewModel.OnAmountChanged.Subscribe(_ =>
        {
            _gridState.UpsertUnit(_viewModel);
        }).AddTo(_disposables);

        _viewModel.OnDeath.Subscribe(_ =>
        {
            _logger($"[ConsoleView] {_viewModel.Model.UnitType.Value} died at {_viewModel.Model.Position.Value}.");
            Dispose();
        }).AddTo(_disposables);

        _viewModel.OnTurnStarted.Subscribe(_ =>
        {
            _logger($"[ConsoleView] {_viewModel.Model.UnitType.Value} turn started.");
        }).AddTo(_disposables);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        _disposed = true;
        _disposables.Dispose();
        _gridState.RemoveUnit(_viewModel);
    }
}

