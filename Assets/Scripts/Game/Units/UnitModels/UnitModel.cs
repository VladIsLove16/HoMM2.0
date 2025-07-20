using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEditor;
using UnityEngine;
public class UnitModel : IEffectable, IEffectApplier, IDamageable, IDamageSource, ICombatUnit, IGridContent
{
    public UnitModel(UnitStats stats, int x, int y, int amount, bool isPlayer)
    {
        Position.Value = new Vector2Int(x, y);
        Amount.Value = amount;
        IsBlueTeam.Value = isPlayer;
        UnitType.Value = stats.UnitType;
        InvulnerableEffects = stats.InvulnerableEffects;
        ModifiedStats = new UnitStats
        {
            UnitType = stats.UnitType,
            Health = stats.Health,
            MaxHealth = stats.MaxHealth,
            Damage = stats.Damage,
            SpellPower = stats.SpellPower,
            Offense = stats.Offense,
            Defense = stats.Defense,
            MoveSpeed = stats.MoveSpeed,
            AttackRange = stats.AttackRange,
            CanFly = stats.CanFly,
            InvulnerableEffects = stats.InvulnerableEffects
        };
    }
    public UnitStats BaseUnitStats { get; }
    public UnitStats ModifiedStats { get; set; }
    public ReactiveProperty<Vector2Int> Position { get; } = new();
    public int X => Position.Value.x;
    public int Y => Position.Value.y;
    public ReactiveProperty<int> Amount { get; } = new();
    public ReactiveProperty<bool> CanAct { get; } = new(true);
    public ReactiveProperty<bool> CanMove { get; } = new(true);
    public ReactiveProperty<bool> IsBlueTeam { get; } = new(true);
    public List<StatusEffectType> InvulnerableEffects;

    public ReactiveProperty<UnitType> UnitType;
    private StatusEffectManager statusEffectManager = new();

    bool ICombatUnit.IsBlueTeam => IsBlueTeam.Value;

    UnitType ICombatUnit.UnitType => UnitType.Value;

    Vector2Int IGridContent.Position
    {
        get { return Position.Value; }
        set { Position.SetValueAndForceNotify(value); }
    }
    public GridContentType GridContentType => GridContentType.unit;
    public Action Died { get; internal set; }
    public Action<int> Hitted { get; internal set; }
    public Action Attacked { get; internal set; }
    public Action TurnStarted { get; internal set; }
    public Action<int> HealthChanged { get; internal set; }

    public void ApplyStatusEffect(StatusEffect effect)
    {
        if (!InvulnerableEffects.Contains(effect.Type))
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
    public int CalculateDamageTo(UnitModel defender)
    {
        var ctx = new DamageContext(
            ModifiedStats.Damage,
            DamageType.physical,
            this
        );

        var simulationCtx = new DamageContext(ctx.DamageAmount, ctx.Type, ctx.Source);

        statusEffectManager.HandleInDamage(simulationCtx,true);

        defender.SimulateReceiveDamage(simulationCtx);

        return simulationCtx.DamageAmount;
    }
    public void SimulateReceiveDamage(DamageContext ctx)
    {
        statusEffectManager.HandleOutDamage(ctx,true);
    }

    public void TakeTurn()
    {
        statusEffectManager.HandleTurnStart();
    }

    public void EndTurn()
    {
        statusEffectManager.HandleTurnEnd();
    }

    public void RemoveEffect(StatusEffect statusEffect)
    {
        statusEffectManager.Remove(statusEffect);
    }
}
