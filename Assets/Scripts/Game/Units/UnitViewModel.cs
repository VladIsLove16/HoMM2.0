using System;
using System.Collections.Generic;
using UnityEngine;

public class UnitViewModel
{
    // Самая «тонкая» модель, где лежат stats и эффекты
    public UnitModel Model { get; }
    private readonly CombatController _combatController = new();

    // События для View
    public event Action<DamageContext> OnOutDamage;
    public event Action<DamageContext> OnInDamage;
    public event Action<int> OnTakeDamage;
    public event Action OnTurnStart;
    public event Action OnDeath;

    // «Вычисляемые» свойства для View
    public int Health => Model.UnitStats.Health;
    public int MaxHealth => Model.UnitStats.MaxHealth;
    public int Damage => Model.UnitStats.Damage;
    public int Amount => Model.Amount;
    public string Name => Model.Name;
    public IReadOnlyList<StatusEffect> StatusEffects => Model.StatusEffectManager.Effects;
    public UnitType UnitType => Model.UnitType;
    public int X => Model.X;
    public int Y => Model.Y;

    public UnitViewModel(UnitModel unitModel)
    {
        Model = unitModel;
        SubscribeModelEvents();
    }

    private void SubscribeModelEvents()
    {
        // Когда модель начинает ход
        Model.OnTurnStart += () => OnTurnStart?.Invoke();

        // Когда модель наносит урон (из CombatController приходит контекст)
        Model.OnBeforeDealDamage += ctx => OnOutDamage?.Invoke(ctx);

        // Когда модель получает урон
        Model.OnBeforeTakeDamage += ctx => OnInDamage?.Invoke(ctx);

        // После вычета ХП
        Model.OnHealthChanged += finalHp =>
        {
            // Model.UnitStats.LastDamageAmount — приватно сохраняется в UnitModel
            OnTakeDamage?.Invoke(Model.UnitStats.LastDamageAmount);
        };

        // Смерть
        Model.OnDeath += () => OnDeath?.Invoke();
    }

    /// <summary>
    /// Делаем удар через CombatController — там пройдёт весь pipeline эффектов
    /// </summary>
    public void DealDamage(UnitModel target)
    {
        Model.TriggerAttack();
        _combatController.DealDamage(target, Damage,Model);
    }

    public void Heal(int amount)
    {
        Model.Heal(amount);
    }

    public void ApplyStatusEffect(StatusEffect effect)
    {
        Model.StatusEffectManager.Add(effect);
    }

    public void RemoveStatusEffect(StatusEffect effect)
    {
        Model.StatusEffectManager.Remove(effect);
    }

    public string GetDescription()
    {
        return $"{Name} ({Amount})";
    }

    // Для IGridContent, IBlockable и т.д. просто дефолт
    public bool CanMoveThrough() => false;
}
