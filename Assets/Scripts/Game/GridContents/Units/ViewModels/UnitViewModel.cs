using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using Zenject;

public class UnitViewModel : IViewModel
{
    public UnitModel Model { get; }

    public Material TeamMaterial { get; private set; }
    public Material HoveredTeamMaterial { get; private set; }
    public IObservable<Unit> OnAttacked => _onAttacked;
    public IObservable<Unit> OnHit => _onHit;
    public IObservable<Unit> OnDeath => _onDeath;
    public IObservable<Unit> OnTurnStarted => _onTurnStarted;
    public IObservable<Unit> OnHealthChanged => _onHealthChanged;
    public IObservable<Unit> OnTeamChanged => _onTeamChanged;
    public IObservable<int> OnAmountChanged => _onAmountChanged;

    public float HealthRatio => Model.ModifiedStats.MaxHealth > 0 ? Model.ModifiedStats.Health / Model.ModifiedStats.MaxHealth : 0;

    public int Health => Model.ModifiedStats.Health;

    public int MaxHealth => Model.ModifiedStats.MaxHealth;

    public int Amount => Model.Amount.Value;

    private Subject<List<Vector3>> _onMoved = new();
    private Subject<Unit> _onAttacked = new();
    private Subject<Unit> _onHit = new();
    private Subject<Unit> _onDeath = new();
    private Subject<Unit> _onTurnStarted = new();
    private Subject<Unit> _onHealthChanged = new();
    private Subject<Unit> _onTeamChanged = new();
    private Subject<int> _onAmountChanged = new();
    private readonly IMaterialProvider _materialProvider;
    public UnitViewModel(UnitModel model, IMaterialProvider definition)
    {
        Model = model;
        _materialProvider = definition;

        // В зависимости от команды выбираем нужные материалы
        UpdateTeamMaterials(model.IsBlueTeam.Value);

        SubscribeToModel();
    }

    private void SubscribeToModel()
    {
        // Пробрасываем события модели во ViewModel (MVVM)
        if (Model == null) return;

        Model.Attacked += _ => _onAttacked.OnNext(Unit.Default);
        Model.Hitted += _ => _onHit.OnNext(Unit.Default);
        Model.Died += () => _onDeath.OnNext(Unit.Default);
        Model.TurnStarted += () => _onTurnStarted.OnNext(Unit.Default);
        Model.HealthChanged += () => _onHealthChanged.OnNext(Unit.Default);
        Model.Amount.Subscribe(v => _onAmountChanged.OnNext(v));
        Model.IsBlueTeam.Subscribe(isBlue =>
        {
            UpdateTeamMaterials(isBlue);
            _onTeamChanged.OnNext(Unit.Default);
        });
    }

    private void UpdateTeamMaterials(bool isBlue)
    {
        if (isBlue)
        {
            TeamMaterial = _materialProvider.GetBlueTeamMaterial();
            HoveredTeamMaterial = _materialProvider.GetHoveredBlueTeamMaterial();
        }
        else
        {
            TeamMaterial = _materialProvider.GetRedTeamMaterial();
            HoveredTeamMaterial = _materialProvider.GetHoveredRedTeamMaterial();
        }
    }
    
}
