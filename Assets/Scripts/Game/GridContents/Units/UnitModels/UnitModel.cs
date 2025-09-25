using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
public class UnitModel : IEffectable, IEffectApplier, IDamagable, IDamageSource, ICombatObject, IGridContent, IBlockable, IMoveable, IRangedAttacker, IAttacker
{
    public Action StatusEffectsChanged;
    public Action<List<Vector2Int>> MovedByRoute;
    Action<ICombatObject> ICombatObject.Died
    {
        get
        {
            return CombatObjectDied;
        }
        set
        {
            CombatObjectDied = value;
        }
    }
   
    public GridContentType GridContentType => GridContentType.unit;
    public Action Died;
    public Action<ICombatObject> CombatObjectDied;
    public Action<DamageContext> Hitted;
    public Action<DamageContext> Attacked;
    public Action TurnStarted;
    public Action HealthChanged;
    public Action StatsChanged;
    Vector2Int IGridContent.Position
    {
        get { return Position.Value; }
        set { Position.SetValueAndForceNotify(value); }
    }
    public UnitStats BaseUnitStats { get; }
    public UnitStats ModifiedStats { get; set; }
    public ReactiveProperty<Vector2Int> Position { get; private set; } = new();
    public int X => Position.Value.x;
    public int Y => Position.Value.y;
    public ReactiveProperty<int> Amount { get; } = new();
    public ReactiveProperty<bool> CanAct { get; } = new(true);
    public ReactiveProperty<bool> CanAttack { get; } = new(true);
    public ReactiveProperty<bool> CanMove { get; } = new(true);
    public ReactiveProperty<bool> IsBlueTeam { get; } = new(true);
    public List<StatusEffectType> InvulnerableEffects;
    public IReadOnlyList<StatusEffect> AppliedEffects => _appliedEffects;
    public IReadOnlyList<StatusEffect> ActiveEffects => _statusEffectManager.ActiveEffects;

    public ReactiveProperty<UnitType> UnitType=new();
    private StatusEffectManager _statusEffectManager = new();
    private List<StatusEffect> _appliedEffects = new();

    bool IGridContent.IsBlueTeam => IsBlueTeam.Value;
    UnitStats ICombatObject.Stats => ModifiedStats;
   
    UnitType ICombatObject.UnitType => UnitType.Value;

    int IMoveable.MoveSpeed => ModifiedStats.MoveSpeed;
    bool IMoveable.CanFly => ModifiedStats.CanFly;
    bool IAttacker.CanAttack => CanAttack.Value;
    int IRangedAttacker.AttackRange => ModifiedStats.AttackRange;

    public UnitModel(UnitStats stats, UnitType unitType, int x, int y, int amount, bool isPlayer)
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
    public void MoveByRoute(List<Vector2Int> route)
    {
        if (route == null)
            throw new ArgumentNullException(nameof(route), "Route cannot be null");
        
        if (route.Count == 0)
            return; // Не меняем позицию если маршрут пустой
            
        Position.SetValueAndForceNotify(route[route.Count-1]);
        // Emit both events to maintain backward compatibility
        MovedByRoute?.Invoke(route);
    }
    
    public void ApplyEffect(StatusEffect effect)
    {
        if (effect == null)
            throw new ArgumentNullException(nameof(effect), "StatusEffect cannot be null");
            
        if (!InvulnerableEffects.Contains(effect.Type))
            _statusEffectManager.Apply(effect);
    }

    public DamageContext SendDamage(AttackContext ctx)
    {
        if (Amount.Value <= 0)
            throw new InvalidOperationException("Cant attack unit with 0 or less amount");
        if (ctx == null)
            throw new ArgumentNullException(nameof(ctx), "AttackContext cannot be null");
            
        if (ModifiedStats.Damage < 0)
            throw new ArgumentException("Damage cannot be negative", nameof(ModifiedStats.Damage));
            
        DamageContext damageContext = new(ModifiedStats.Damage * Amount.Value, DamageType.physical, this);
        _statusEffectManager.HandleOutDamage(damageContext);
        _appliedEffects.AddRange(damageContext.AppliedEffects);
        ctx.Target.RecieveDamage(damageContext);
        Attacked?.Invoke(damageContext);
        return damageContext;
    }
    public DamageContext SimulateSendDamage(AttackContext defender)
    {
        if (defender == null)
            throw new ArgumentNullException(nameof(defender), "AttackContext cannot be null");
            
        if (ModifiedStats.Damage < 0)
            throw new ArgumentException("Damage cannot be negative", nameof(ModifiedStats.Damage));
            
        DamageContext damageContext = new(ModifiedStats.Damage * Amount.Value, DamageType.physical, this);
        _statusEffectManager.HandleOutDamage(damageContext, true);
        defender.Target.SimulateRecieveDamage(damageContext);
        return damageContext;
    }

    /// <summary>
    /// Applies incoming damage to this unit and raises domain events.
    /// </summary>
    public void RecieveDamage(DamageContext ctx)
    {
        if (ctx == null)
            throw new ArgumentNullException(nameof(ctx), "DamageContext cannot be null");
            
        if (ctx.DamageAmount < 0)
            throw new ArgumentException("Damage amount cannot be negative", nameof(ctx.DamageAmount));
            
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
            CombatObjectDied?.Invoke(this);
        }
    }
    
    /// <summary>
    /// Simulates receiving damage without mutating persistent state (used for previews/AI).
    /// </summary>
    public void SimulateRecieveDamage(DamageContext ctx)
    {
        if (ctx == null)
            throw new ArgumentNullException(nameof(ctx), "DamageContext cannot be null");
            
        _statusEffectManager.HandleOutDamage(ctx, true);
    }

    public void TakeTurn()
    {
        _statusEffectManager.HandleTurnStart();
        TurnStarted?.Invoke();
        Debug.Log(UnitType + " takes turn");
    }

    public void EndTurn()
    {
        _statusEffectManager.HandleTurnEnd();
    }

    public void RemoveEffect(StatusEffect statusEffect)
    {
        if (statusEffect == null)
            throw new ArgumentNullException(nameof(statusEffect), "StatusEffect cannot be null");
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

    public IReadOnlyList<StatusEffect> GetAppliedEffects()
    {
        return AppliedEffects;
    }

   
}
