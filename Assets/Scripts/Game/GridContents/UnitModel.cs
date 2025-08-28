using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEditor;
using UnityEngine;
public class UnitModel : IEffectable, IEffectApplier, IDamagable, IDamageSource, ICombatObject, IGridContent, IBlockable, IMoveable
{
    public UnitModel(UnitStats stats,UnitType unitType, int x, int y, int amount, bool isPlayer)
    {
        Position.Value = new Vector2Int(x, y);
        Amount.Value = amount;
        IsBlueTeam.Value = isPlayer;
        UnitType.Value = unitType;
        InvulnerableEffects = stats.InvulnerableEffects;
        BaseUnitStats = stats;
        ModifiedStats = ScriptableObject.CreateInstance<UnitStats>();
        ModifiedStats += BaseUnitStats;
        //{
        //    ModifiedStats.Health = stats.Health;
        //    MaxHealth = stats.MaxHealth,
        //    Damage = stats.Damage,
        //    SpellPower = stats.SpellPower,
        //    Offense = stats.Offense,
        //    Defense = stats.Defense,
        //    MoveSpeed = stats.MoveSpeed,
        //    AttackRange = stats.AttackRange,
        //    CanFly = stats.CanFly,
        //    InvulnerableEffects = stats.InvulnerableEffects
        //}
        //;
    }
    public UnitStats BaseUnitStats { get; }
    public UnitStats ModifiedStats { get; set; }
    public ReactiveProperty<Vector2Int> Position { get; private set; } = new();
    public int X => Position.Value.x;
    public int Y => Position.Value.y;
    public ReactiveProperty<int> Amount { get; } = new();
    public ReactiveProperty<bool> CanAct { get; } = new(true);
    public ReactiveProperty<bool> CanMove { get; } = new(true);
    public ReactiveProperty<bool> IsBlueTeam { get; } = new(true);
    public List<StatusEffectType> InvulnerableEffects;
    public Action StatusEffectsChanged;
    public Action<List<Vector2Int>> MovedByRoute;
    public IReadOnlyList<StatusEffect> ActiveEffects => _statusEffectManager.ActiveEffects;

    public ReactiveProperty<UnitType> UnitType=new();
    private StatusEffectManager _statusEffectManager = new();

    bool ICombatObject.IsBlueTeam => IsBlueTeam.Value;
    bool IDamagable.IsBlueTeam => IsBlueTeam.Value;
    UnitStats ICombatObject.Stats => ModifiedStats;


    UnitType ICombatObject.UnitType => UnitType.Value;

    Vector2Int IGridContent.Position
    {
        get { return Position.Value; }
        set { Position.SetValueAndForceNotify(value); }
    }
    public GridContentType GridContentType => GridContentType.unit;
    public Action Died { get; internal set; }
    public Action<DamageContext> Hitted { get; internal set; }
    public Action<DamageContext> Attacked { get; internal set; }
    public Action TurnStarted { get; internal set; }
    public Action HealthChanged { get; internal set; }
    public Action<List<Vector2Int>> Moved { get; internal set; }
    public Action StatsChanged { get; internal set; }

    public void MoveByRoute(List<Vector2Int> route)
    {
        Position.SetValueAndForceNotify(route[route.Count-1]);
        Moved?.Invoke(route);
    }
    public void ApplyStatusEffect(StatusEffect effect)
    {
        if (!InvulnerableEffects.Contains(effect.Type))
            _statusEffectManager.Apply(effect);
    }

    public DamageContext SendDamage(AttackContext ctx)
    {
        DamageContext damageContext = new(ModifiedStats.Damage * Amount.Value, DamageType.physical, this);
        _statusEffectManager.HandleOutDamage(damageContext);
        ctx.Target.RecieveDamage(damageContext);
        Attacked?.Invoke(damageContext);
        return damageContext;
    }
    public DamageContext SimulateSendDamage(AttackContext defender)
    {
        DamageContext damageContext = new(ModifiedStats.Damage * Amount.Value, DamageType.physical, this);
        _statusEffectManager.HandleOutDamage(damageContext, true);
        defender.Target.SimulateRecieveDamage(damageContext);
        return damageContext;
    }

    public void RecieveDamage(DamageContext ctx)
    {
        _statusEffectManager.HandleOutDamage(ctx);
        int damageLeft = ctx.DamageAmount;

        while (damageLeft > 0 && Amount.Value > 0)
        {
            if (ModifiedStats.Health > damageLeft)
            {
                ModifiedStats.Health -= damageLeft;
                damageLeft = 0;
            }
            else
            {
                damageLeft -= ModifiedStats.Health;
                Amount.Value--;
                ctx.DieAmount++;
                if (Amount.Value > 0)
                {
                    // Восстанавливаем здоровье следующего юнита в стэке на максимум
                    ModifiedStats.Health = ModifiedStats.MaxHealth;
                }
                else
                {
                    ModifiedStats.Health = 0;
                    break;
                }
            }
        }
        HealthChanged?.Invoke();
        Hitted?.Invoke(ctx);
        if (Amount.Value <= 0)
        {
            Died?.Invoke();
        }
    }
    public void SimulateRecieveDamage(DamageContext ctx)
    {
        _statusEffectManager.HandleOutDamage(ctx,true);
    }

    public void TakeTurn()
    {
        _statusEffectManager.HandleTurnStart();
        Debug.Log(UnitType + " takes turn");
    }

    public void EndTurn()
    {
        _statusEffectManager.HandleTurnEnd();

    }

    public void RemoveEffect(StatusEffect statusEffect)
    {
        _statusEffectManager.Remove(statusEffect);
    }

    public bool CanMoveThrough()
    {
        return false;
    }
    public override string ToString()
    {
        return UnitType.ToString();
    }
}
