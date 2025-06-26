using NUnit.Framework;
using System;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
//IBlockable, IEffectable, IAttackable,
public class UnitModel : IGridContent,  IDamageSource, IDamageable, IEffectable, IEffectApplier, ICombatUnit
{
    public event Action<DamageContext> OnBeforeDealDamage;
    public event Action<DamageContext> OnBeforeTakeDamage;
    public event Action<int> OnHealthChanged;
    public event Action OnTurnStart;
    public event Action OnDeath;
    public event Action OnAttack;
    public event Action<int> OnHit;

    public UnitStats UnitStats { get; }
    private int x;
    private int y;
    public int X => x;
    public int Y => y;
    public Vector2Int Coodrs => new(x,y);
    public int Amount { get; internal set; }
    public UnitType UnitType { get; }
    public string Name { get; }
    public bool IsBlueTeam { get; }
    public StatusEffectManager StatusEffectManager => UnitStats.StatusEffectManager;

    public event Action<ICombatUnit> OnTurnEnded;

    public event Action<ICombatUnit> OnTurnTaken;

    public UnitModel(UnitDefinitionSO unitDefinitionSO, int x,int y, int amount, bool isPlayer)
    {
        
        UnitStats = new UnitStats(unitDefinitionSO);
        foreach (var effect in unitDefinitionSO.StartingEffects)
        {
            StatusEffect statusEffect = new StatusEffect(effect, this, this);
            StatusEffectManager.Add(statusEffect);
        }
        this.x = x;
        this.y = y;
        Amount = amount;
        UnitType = unitDefinitionSO.UnitType;
        Name = unitDefinitionSO.Name;
        IsBlueTeam = isPlayer;
    }

    public void ReceiveDamage(int damage)
    {
        int prev = UnitStats.Health;
        UnitStats.LastDamageAmount = damage;
        UnitStats.Health = Mathf.Max(UnitStats.Health - damage, 0);
        OnHealthChanged?.Invoke(UnitStats.Health);
        OnHit?.Invoke(damage);

        if (UnitStats.Health == 0)
            OnDeath?.Invoke();
    }

    public void Heal(int amount)
    {
        UnitStats.Health = Mathf.Min(UnitStats.Health + amount, UnitStats.MaxHealth);
        OnHealthChanged?.Invoke(UnitStats.Health);
    }

    public void AddMaxHP(int amount)
    {
        UnitStats.MaxHealth += amount;
    }

    internal void TriggerAttack() => OnAttack?.Invoke();

    public override string ToString()
    {
        return $"{Name} + ({Amount})";
    }

    public string GetDescription()
    {
        return ToString();
    }

    public void SetCoords(int x, int y)
    {
        this.x = x; 
        this.y = y;
    }

    public void TakeTurn()
    {
        OnTurnStart?.Invoke();
    }
}
