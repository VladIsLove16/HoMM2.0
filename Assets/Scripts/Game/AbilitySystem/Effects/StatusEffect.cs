// Model/StatusEffect.cs
using System;
using System.Linq;

public class StatusEffect
{
    /// <summary>
    /// Образующие эффект данные
    /// </summary>
    public readonly StatusEffectData data;
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

        target.TurnStarted += HandleTurnStart;
        target.Died += HandleRemove;
        target.BeforeInDamage += OnIn;
        target.BeforeOutDamage += OnOut;

        HandleApply();
    }

    private void OnOut(DamageContext ctx)
    {
        foreach (var reaction in data.OnOutDamage)
            reaction.Execute(ctx);

        foreach (var cond in conditions)
            cond.OnOutDamage(ctx);

        CheckExpire();
    }

    private void OnIn(DamageContext ctx)
    {
        foreach (var reaction in data.OnInDamage)
            reaction.Execute(ctx);

        foreach (var cond in conditions)
            cond.OnInDamage(ctx);

        CheckExpire();
    }

    /// <summary>
    /// Для UI: сколько ходов / уронов осталось до снятия
    /// </summary>
    public string[] GetExpirations()
        => conditions.Select(c => c.GetRemaining()).ToArray();
    private void HandleApply()
    {
        var applyCtx = new EffectReactionContext(target, source);
        foreach (var reaction in data.OnApply)
            reaction.Execute(applyCtx);

        foreach (var cond in conditions)
            cond.OnApply();

        CheckExpire();
    }

    private void HandleTurnStart()
    {
        EffectReactionContext ctx = new EffectReactionContext(target ,source);
        foreach (var reaction in data.OnTurnStart)
            reaction.Execute(ctx);

        foreach (var cond in conditions)
            cond.OnTurn();

        CheckExpire();
    }

    private void CheckExpire()
    {
        if (conditions.Any(c => c.ShouldRemove))
            HandleRemove();
    }

    private void HandleRemove()
    {
        // Реакции OnRemove
        var ctx = new EffectReactionContext(target, source);
        foreach (var reaction in data.OnRemove)
            reaction.Execute(ctx);

        // Отписка
        target.TurnStarted -= HandleTurnStart;
        target.Died -= HandleRemove;
        // Убираем себя из менеджера эффектов
        target.RemoveEffect(this);
    }
   

    
}
