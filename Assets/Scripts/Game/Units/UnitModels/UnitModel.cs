using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEditor;
using UnityEngine;
public class UnitModel : IGridContent,  IDamageSource, IDamagable, IEffectable, IEffectApplier, ICombatUnit
{
    public event Action<DamageContext> BeforeInDamage;
    public event Action<DamageContext> BeforeOutDamage;
    public event Action<int> HealthChanged;
    public event Action TurnStarted;
    public event Action TurnEnded;
    public event Action Died;
    public event Action Attacked;
    public event Action<int> Hitted;
    public IReadOnlyList<StatusEffect> ActiveEffects => _activeEffects.AsReadOnly();
    private UnitState _baseStats;
    private UnitState _currentStats;
    public UnitState UnitState => _currentStats;
    private List<StatusEffect> _activeEffects = new();
    private List<StatusEffectData> _invulnerableEffects = new();
    private int x;
    private int y;
    public int X => x;
    public int Y => y;
    public Vector2Int Coodrs => new(x,y);
    public int Amount { get; internal set; }
    public UnitType UnitType { get; }
    public string Name { get; }
    public bool IsBlueTeam { get; }

    public UnitModel(UnitDefinitionSO unitDefinitionSO, int x,int y, int amount, bool isPlayer)
    {
        _baseStats = new UnitState(unitDefinitionSO);
        _currentStats = _baseStats;
        _invulnerableEffects = unitDefinitionSO.InvulnerableEffects.ToList();
        foreach (var effectData in unitDefinitionSO.StartingEffects)
        {
            ApplyEffect(effectData,this);
        }
        this.x = x;
        this.y = y;
        Amount = amount;
        UnitType = unitDefinitionSO.UnitType;
        Name = unitDefinitionSO.Name;
        IsBlueTeam = isPlayer;
    }

    public void ReceiveDamage(DamageContext damageCtx)
    {
        int prev = UnitState.Health;
        BeforeInDamage?.Invoke(damageCtx);

        UnitState.LastDamageAmount = damageCtx.DamageAmount;
        UnitState.Health = Mathf.Max(UnitState.Health - damageCtx.DamageAmount, 0);
        HealthChanged?.Invoke(UnitState.Health);
        Hitted?.Invoke(damageCtx.DamageAmount);

        if (UnitState.Health == 0)
            Died?.Invoke();
    }

    public void Heal(int amount)
    {
        UnitState.Health = Mathf.Min(UnitState.Health + amount, UnitState.MaxHealth);
        HealthChanged?.Invoke(UnitState.Health);
    }

    public void AddMaxHP(int amount)
    {
        UnitState.MaxHealth += amount;
    }

    internal void TriggerAttack() => Attacked?.Invoke();

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
        TurnStarted?.Invoke();
    }

    public void EndTurn()
    {
        TurnEnded?.Invoke();
    }

    public void ApplyEffect(StatusEffectData statusEffect,IEffectApplier effectApplier)
    {
        if(!_invulnerableEffects.Contains(statusEffect))
        {
            StatusEffect se = new StatusEffect(statusEffect, this, effectApplier);
            _activeEffects.Add(se);
        }
    }

    public void RemoveEffect(StatusEffect statusEffect)
    {
        _activeEffects.Remove(statusEffect);
    }
}
