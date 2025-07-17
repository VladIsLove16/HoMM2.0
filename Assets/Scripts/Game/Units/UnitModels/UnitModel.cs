using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UIElements;
public interface IEffectable
{
    public void ApplyStatusEffect(StatusEffect statusEffect);
}
public interface IEffectApplier
{
    public int SpellPower { get; }
}
public class UnitModel : IEffectable, IEffectApplier, IDamageable, IDamageSource
{
    public UnitModel(UnitStats stats, int x, int y, int amount, bool isPlayer)
    {
        Position.Value = new Vector2Int(x, y);
        Amount.Value = amount;
        IsBlueTeam.Value = isPlayer;
        UnitType.Value = stats.UnitType;
        Health.Value = stats.Health;
        MaxHealth.Value = stats.MaxHealth;
        Damage.Value = stats.Damage;
        SpellPower.Value = stats.SpellPower;
        Damage.Value = stats.Offense;
        Defense.Value = stats.Defense;
        MoveSpeed.Value = stats.MoveSpeed;
        AttackRange.Value = stats.AttackRange;
        CanFly.Value = stats.CanFly;
        InvulnerableEffects = stats.InvulnerableEffects;
    }
    public UnitStats UnitStats { get; } 
    public ReactiveProperty<Vector2Int> Position { get; } = new();
    public ReactiveProperty<int> Amount { get; } = new();
    public ReactiveProperty<int> MovementRange { get; } = new();
    public ReactiveProperty<int> AttackRange { get; } = new();
    public ReactiveProperty<int> MaxHealth { get; } = new();
    public ReactiveProperty<int> Health { get; } = new();
    public ReactiveProperty<int> MoveSpeed { get; } = new();
    public ReactiveProperty<int> Offense { get; } = new();
    public ReactiveProperty<int> Defense { get; } = new();
    public ReactiveProperty<int> Damage { get; } = new();
    public ReactiveProperty<int> SpellPower { get; } = new();
    public ReactiveProperty<bool> CanAct { get; } = new(true);
    public ReactiveProperty<bool> CanMove { get; } = new(true);
    public ReactiveProperty<bool> CanFly { get; } = new(true);
    public ReactiveProperty<bool> IsBlueTeam { get; } = new(true);
    public List<StatusEffectType> InvulnerableEffects;

    public ReactiveProperty< UnitType> UnitType;
    private StatusEffectManager statusEffectManager = new();
    int IEffectApplier.SpellPower => SpellPower.Value;
    public void ApplyStatusEffect(StatusEffect effect)
    {
        if(!InvulnerableEffects.Contains(effect.Type))
            statusEffectManager.Apply(effect);
    }

    public void SendDamage(DamageContext ctx)
    {
        statusEffectManager.HandleInDamage(ctx);
    }

    public void ReceiveDamage(DamageContext ctx)
    {
        statusEffectManager.HandleOutDamage(ctx);
    }
}

