using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using Zenject;

public class UnitViewModel : IViewModel
{
    public UnitModel Model { get; }

    public Material TeamMaterial { get; }
    public Material HoveredTeamMaterial { get; }
    public IObservable<Unit> OnAttacked => _onAttacked;
    public IObservable<Unit> OnHit => _onHit;
    public IObservable<Unit> OnDeath => _onDeath;
    public IObservable<Unit> OnTurnStarted => _onTurnStarted;
    public IObservable<Unit> OnHealthChanged => _onHealthChanged;

    private Subject<List<Vector3>> _onMoved = new();
    private Subject<Unit> _onAttacked = new();
    private Subject<Unit> _onHit = new();
    private Subject<Unit> _onDeath = new();
    private Subject<Unit> _onTurnStarted = new();
    private Subject<Unit> _onHealthChanged = new();

    [Inject]  private IWorldToCellProvider _worldToCellProvider;
    public UnitViewModel(UnitModel model, IMaterialProvider definition)
    {
        Model = model;

        // В зависимости от команды выбираем нужные материалы
        if (model.IsBlueTeam.Value)
        {
            TeamMaterial = definition.GetBlueTeamMaterial();
            HoveredTeamMaterial = definition.GetHoveredBlueTeamMaterial();
        }
        else
        {
            TeamMaterial = definition.GetRedTeamMaterial();
            HoveredTeamMaterial = definition.GetHoveredRedTeamMaterial();
        }

        SubscribeToModel();
    }

    private void SubscribeToModel()
    {
        //Model.Moved += OnModelMoved;
        Model.Attacked += _ => _onAttacked.OnNext(Unit.Default);
        Model.Hitted += _ => _onHit.OnNext(Unit.Default);
        Model.Died += () => _onDeath.OnNext(Unit.Default);
        Model.TurnStarted += () => _onTurnStarted.OnNext(Unit.Default);
        Model.HealthChanged += () => _onHealthChanged.OnNext(Unit.Default);
    }
    
}
