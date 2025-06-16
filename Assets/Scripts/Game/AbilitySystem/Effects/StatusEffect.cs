// Model/StatusEffect.cs
using System;
using System.Linq;

public class StatusEffect
{
    public readonly StatusEffectData Data;
    private readonly IEffectable target;
    private readonly IEffectApplier source;
    private readonly ExpirationConditionBase[] conditions;

    public StatusEffect(
        StatusEffectData data,
        IEffectable target,
        IEffectApplier source = null)
    {
        Data = data;
        this.source = source;
        this.target = target;

        // Копируем и инициализируем условия истечения
        conditions = data.ExpirationConditions
                         .Select(c => c.CreateRuntimeInstance())
                         .ToArray();
        foreach (var cond in conditions)
        {
            cond.Initialize();
            cond.OnApply();
        }

        // Подписываемся на события модели
        target.OnTurnStart += HandleTurnStart;
        target.OnDeath += HandleRemove;

        // Реакции OnApply
        var applyCtx = new EffectContext(target, source, data);
        foreach (var r in data.OnApply)
            r.Execute(applyCtx);

        CheckExpire();
    }

    private void HandleTurnStart()
    {
        var ctx = new EffectContext(target, source, Data);
        foreach (var r in Data.OnTurnStart)
            r.Execute(ctx);

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
        var ctx = new EffectContext(target, source, Data);
        foreach (var r in Data.OnRemove)
            r.Execute(ctx);

        // Отписка
        target.OnTurnStart -= HandleTurnStart;
        target.OnDeath -= HandleRemove;
        // Убираем себя из менеджера эффектов
        target.StatusEffectManager.Remove(this);
    }

    /// <summary>
    /// Для UI: сколько ходов / уронов осталось до снятия
    /// </summary>
    public string[] GetExpirations()
        => conditions.Select(c => c.GetRemaining()).ToArray();

    internal void OnOut(DamageContext ctx)
    {
        foreach (var r in Data.OnOutDamage)
            r.Execute(ctx);

        foreach (var cond in conditions)
            cond.OnOutDamage(ctx);

        CheckExpire();
    }

    internal void OnIn(DamageContext ctx)
    {
        foreach (var r in Data.OnInDamage)
            r.Execute(ctx);

        foreach (var cond in conditions)
            cond.OnInDamage(ctx);

        CheckExpire();
    }
}
