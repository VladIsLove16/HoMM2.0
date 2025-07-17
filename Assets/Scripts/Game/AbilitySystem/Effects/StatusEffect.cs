// Model/StatusEffect.cs
using System;
using System.Linq;

public class StatusEffect
{
    public readonly StatusEffectData Data;
    private readonly IEffectable target;
    private readonly IEffectApplier source;
    private readonly ExpirationConditionBase[] conditions;

    public StatusEffectType Type => Data.Type;

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
        }
    }

    public void HandleApply()
    {
        var ctx = new EffectContext(target, source, Data);
        foreach (var r in Data.OnApply)
            r.Execute(ctx);

        foreach (var cond in conditions)
            cond.OnApply();

        CheckExpire();
    }

    public void HandleTurnStart()
    {
        var ctx = new EffectContext(target, source, Data);
        foreach (var r in Data.OnTurnStart)
            r.Execute(ctx);

        foreach (var cond in conditions)
            cond.OnTurn();

        CheckExpire();
    }


    public void HandleRemove()
    {
        // Реакции OnRemove
        var ctx = new EffectContext(target, source, Data);
        foreach (var r in Data.OnRemove)
            r.Execute(ctx);
    }

    /// <summary>
    /// Для UI: сколько ходов / уронов осталось до снятия
    /// </summary>

    public void HandleOutDamage(DamageContext ctx)
    {
        foreach (var r in Data.OnOutDamage)
            r.Execute(ctx);

        foreach (var cond in conditions)
            cond.OnOutDamage(ctx);

        CheckExpire();
    }

    public void HandleInDamage(DamageContext ctx)
    {
        foreach (var r in Data.OnInDamage)
            r.Execute(ctx);

        foreach (var cond in conditions)
            cond.OnInDamage(ctx);

        CheckExpire();
    }
    public string[] GetExpirations()
        => conditions.Select(c => c.GetRemaining()).ToArray();

    private void CheckExpire()
    {
        if (conditions.Any(c => c.ShouldRemove))
            HandleRemove();
    }
}
