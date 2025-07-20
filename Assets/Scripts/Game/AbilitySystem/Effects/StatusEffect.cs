// Model/StatusEffect.cs
using System;
using System.Linq;
using static UnityEngine.InputSystem.HID.HID;

public class StatusEffect
{
    /// <summary>
    /// Образующие эффект данные
    /// </summary>
    private readonly StatusEffectData data;
    /// <summary>
    /// Тип эффекта
    /// </summary>
    public readonly StatusEffectType Type;
    /// <summary>
    /// Цель эффекта (обладатель)
    /// </summary>
    private readonly IEffectable target;
    /// <summary>
    /// Кастер эффекта (накладыватель)
    /// </summary>
    private readonly IEffectApplier source;
    /// <summary>
    /// Условия исчезновения эффекта
    /// </summary>
    private readonly ExpirationConditionBase[] conditions;
    public string Name { get => data.name; }
    public StatusEffect(
       
        StatusEffectData data,
        IEffectable target,
        IEffectApplier source = null)
    {
        this.data = data;
        this.source = source;
        this.target = target;

        // Копируем и инициализируем условия истечения
        conditions = data.ExpirationConditions
                         .Select(c => c.CreateRuntimeInstance())
                         .ToArray();
        foreach (var cond in conditions)
        {
            cond.Initialize();
        }
    }

    public void HandleOutDamage(DamageContext ctx, bool simulation = false)
    {
        foreach (var reaction in data.OnOutDamage)
            reaction.Execute(ctx, simulation);
        if(!simulation)
        {
            foreach (var cond in conditions)
                cond.OnOutDamage(ctx);

            CheckExpire();
        }
    }

    public void HandleInDamage(DamageContext ctx, bool simulation = false)
    {
        foreach (var reaction in data.OnInDamage)
            reaction.Execute(ctx, simulation);
        if (!simulation)
        {
            foreach (var cond in conditions)
                cond.OnInDamage(ctx);

            CheckExpire();
        }
    }
    public void HandleTurnStart()
    {
        EffectReactionContext ctx = new EffectReactionContext(target, source);
        foreach (var reaction in data.OnTurnStart)
            reaction.Execute(ctx);
        foreach (var cond in conditions)
                cond.OnTurn();

        CheckExpire();
    }
    internal void HandleTurnEnd()
    {
        EffectReactionContext ctx = new EffectReactionContext(target, source);
        foreach (var reaction in data.OnTurnEnd)
            reaction.Execute(ctx);

        foreach (var cond in conditions)
            cond.OnTurnEnd();

        CheckExpire();
    }
    public void HandleApply()
    {
        var applyCtx = new EffectReactionContext(target, source);
        foreach (var reaction in data.OnApply)
            reaction.Execute(applyCtx);

        foreach (var cond in conditions)
            cond.OnApply();

        CheckExpire();
    }

    public void HandleRemove()
    {
        // Реакции OnRemove
        var ctx = new EffectReactionContext(target, source);
        foreach (var reaction in data.OnRemove)
            reaction.Execute(ctx);

        target.RemoveEffect(this);
    }

    /// <summary>
    /// Для UI: сколько ходов / уронов осталось до снятия
    /// </summary>
    public string[] GetExpirations()
        => conditions.Select(c => c.GetRemaining()).ToArray();
    


    private void CheckExpire()
    {
        if (conditions.Any(c => c.ShouldRemove))
            HandleRemove();
    }

   
}
