using System;
using UniRx;
using Unity.VisualScripting;

public class UnitStatsViewModel : IDisposable
{
    private readonly CompositeDisposable _disposables = new();
    public ReactiveProperty<int> Health { get; }
    public ReactiveProperty<int> MaxHealth { get; }
    public ReactiveProperty<int> AttackDamage { get; }
    public ReactiveProperty<int> MoveSpeed { get; }
    public ReactiveProperty<int> Amount { get; }
    public ReactiveProperty<bool> IsVisible { get; }
    public ReactiveCollection<StatusEffectViewModel> StatusEffects { get; }

    public readonly UnitModel Model;

    public UnitStatsViewModel(UnitModel model)
    {
        if (model == null)
            throw new ArgumentException();
        Model = model;
        if (model.ModifiedStats == null)
            return;
        var stats = model.ModifiedStats;

        Health = new ReactiveProperty<int>(stats.Health).AddTo(_disposables);
        MaxHealth = new ReactiveProperty<int>(stats.MaxHealth).AddTo(_disposables);
        AttackDamage = new ReactiveProperty<int>(stats.Damage).AddTo(_disposables);
        MoveSpeed = new ReactiveProperty<int>(stats.MoveSpeed).AddTo(_disposables);
        Amount = new ReactiveProperty<int>(model.Amount.Value).AddTo(_disposables);

        StatusEffects = new ReactiveCollection<StatusEffectViewModel>();

        UpdateStatusEffects();

        model.HealthChanged += () => Health.Value = model.ModifiedStats.Health;
        model.StatsChanged += () =>
        {
            MaxHealth.Value = model.ModifiedStats.MaxHealth;
            AttackDamage.Value = model.ModifiedStats.Damage;
            MoveSpeed.Value = model.ModifiedStats.MoveSpeed;
        };

        model.StatusEffectsChanged += UpdateStatusEffects;
    }

    private void UpdateStatusEffects()
    {
        StatusEffects.Clear();
        foreach (var effect in Model.ActiveEffects)
        {
            StatusEffects.Add(new StatusEffectViewModel(effect));
        }
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}
