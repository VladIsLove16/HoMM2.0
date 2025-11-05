using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Zenject;

/// <summary>
/// Console-friendly view that renders game state to logs instead of 3D objects.
/// Acts as an alternative presentation layer without modifying existing view models.
/// </summary>
public class ConsoleGameView : IInitializable, IDisposable
{
    private readonly GameViewModel _gameViewModel;
    private readonly ITurnStateViewModel _turnState;
    private readonly ConsoleGridState _gridState;
    private readonly IDeveloperConsoleOutput _consoleOutput;
    private readonly Dictionary<UnitViewModel, UnitBinding> _unitBindings = new();
    private readonly CompositeDisposable _disposables = new();

    public ConsoleGameView(
        GameViewModel gameViewModel,
        ITurnStateViewModel turnState,
        ConsoleGridState gridState,
        [Inject(Optional = true)] IDeveloperConsoleOutput consoleOutput = null)
    {
        _gameViewModel = gameViewModel ?? throw new ArgumentNullException(nameof(gameViewModel));
        _turnState = turnState ?? throw new ArgumentNullException(nameof(turnState));
        _gridState = gridState ?? throw new ArgumentNullException(nameof(gridState));
        _consoleOutput = consoleOutput;
    }

    public void Initialize()
    {
        _gameViewModel.GridInited += OnGridInitialized;
        _gameViewModel.PreviewChanged += OnPreviewChanged;
        _gameViewModel.PreviewUpdated += OnPreviewUpdated;
        _gameViewModel.UnitSpawned += OnUnitSpawned;
        _gameViewModel.DamageContextPreviewChanged += OnDamagePreviewChanged;

        _turnState.ActiveObject.Subscribe(OnActiveObjectChanged).AddTo(_disposables);

        Log("Console game view initialized.");
    }

    private void OnGridInitialized(int width, int height)
    {
        _gridState.Initialize(width, height);
        LogGrid($"Grid initialized {width}x{height}");
    }

    private void OnPreviewChanged(PreviewResult preview)
    {
        if (preview == null)
        {
            return;
        }

        _gridState.UpdatePreview(preview.ToDictionary(), replace: true);
        LogGrid("Preview changed");
    }

    private void OnPreviewUpdated(PreviewResult preview)
    {
        if (preview == null)
        {
            return;
        }

        _gridState.UpdatePreview(preview.ToDictionary(), replace: false);
        LogGrid("Preview updated");
    }

    private void OnDamagePreviewChanged(DamageContextPreview preview)
    {
        if (preview?.Damage == null)
        {
            return;
        }

        var damage = preview.Damage;
        Log($"Damage preview: {damage.DamageAmount} {damage.Type}");
    }

    private void OnUnitSpawned(UnitViewModel viewModel)
    {
        if (viewModel == null)
        {
            return;
        }

        if (_unitBindings.ContainsKey(viewModel))
        {
            return;
        }

        var presenter = new ConsoleUnitPresenter(viewModel, _gridState, Log);
        var deathSubscription = viewModel.OnDeath.Subscribe(_ =>
        {
            RemoveUnitPresenter(viewModel);
            LogGrid($"Unit removed: {viewModel.Model.UnitType.Value} {viewModel.Model.Team.Value}");
        });

        _unitBindings.Add(viewModel, new UnitBinding(presenter, deathSubscription));
        LogGrid($"Unit spawned: {viewModel.Model.UnitType.Value} [{viewModel.Model.Team.Value}] at {viewModel.Model.Position.Value}");
    }

    private void OnActiveObjectChanged(ICombatObject combatObject)
    {
        if (combatObject == null)
        {
            Log("Active unit: none");
            return;
        }

        var position = combatObject.Position;
        Log($"Active unit: {combatObject.UnitType} [{combatObject.Team}] at ({position.x},{position.y})");
    }

    private void RemoveUnitPresenter(UnitViewModel viewModel)
    {
        if (!_unitBindings.TryGetValue(viewModel, out var binding))
        {
            return;
        }

        binding.Dispose();
        _unitBindings.Remove(viewModel);
    }

    private void LogGrid(string header)
    {
        Log(header);
        Log(_gridState.BuildRepresentation());
    }

    private void Log(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return;
        }

        if (_consoleOutput != null)
        {
            _consoleOutput.AppendLine(message);
        }
        else
        {
            Debug.Log(message);
        }
    }

    public void Dispose()
    {
        _gameViewModel.GridInited -= OnGridInitialized;
        _gameViewModel.PreviewChanged -= OnPreviewChanged;
        _gameViewModel.PreviewUpdated -= OnPreviewUpdated;
        _gameViewModel.UnitSpawned -= OnUnitSpawned;
        _gameViewModel.DamageContextPreviewChanged -= OnDamagePreviewChanged;

        _disposables.Dispose();

        foreach (var binding in _unitBindings.Values)
        {
            binding.Dispose();
        }
        _unitBindings.Clear();
    }

    private sealed class UnitBinding : IDisposable
    {
        private readonly ConsoleUnitPresenter _presenter;
        private readonly IDisposable _deathSubscription;

        public UnitBinding(ConsoleUnitPresenter presenter, IDisposable deathSubscription)
        {
            _presenter = presenter;
            _deathSubscription = deathSubscription;
        }

        public void Dispose()
        {
            _deathSubscription?.Dispose();
            _presenter?.Dispose();
        }
    }
}

