// Unit/StatusEffect.cs
using System;
using System.Linq;
using UnityEngine;

public class StatusEffect
{
    /// <summary>
    /// Образующие эффект данные
    /// </summary>
    private readonly StatusEffectData _data;
    
    /// <summary>
    /// Тип эффекта
    /// </summary>
    public StatusEffectType Type => _data.Type;
    
    /// <summary>
    /// Цель эффекта (обладатель)
    /// </summary>
    private readonly IEffectable _target;
    
    /// <summary>
    /// Кастер эффекта (накладыватель)
    /// </summary>
    private readonly IEffectApplier _source;
    
    /// <summary>
    /// Условия исчезновения эффекта
    /// </summary>
    private readonly ExpirationConditionBase[] _conditions;
    
    public string Name => _data.name;

    public StatusEffect(StatusEffectData data, IEffectable target, IEffectApplier source = null)
    {
        // Валидация входных параметров
        if (data == null) 
            throw new ArgumentNullException(nameof(data), "StatusEffectData cannot be null");
        if (target == null) 
            throw new ArgumentNullException(nameof(target), "Target cannot be null");

        _data = data;
        _source = source;
        _target = target;

        // Безопасная инициализация условий
        try
        {
            _conditions = data.ExpirationConditions?
                .Select(c => c.CreateRuntimeInstance())
                .ToArray() ?? Array.Empty<ExpirationConditionBase>();
                
            foreach (var condition in _conditions)
            {
                condition?.Initialize();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to initialize StatusEffect conditions: {ex.Message}");
            _conditions = Array.Empty<ExpirationConditionBase>();
        }
    }

    // ГРУППА 1: Публичное API - координация
    public void HandleOutDamage(DamageContext ctx, bool simulation = false)
    {
        if (ctx == null) return;
        
        ProcessDamageReactions(ctx, simulation);
        UpdateConditionsOnDamage(ctx, simulation);
        CheckExpireIfNeeded(simulation);
    }

    public void HandleInDamage(DamageContext ctx, bool simulation = false)
    {
        if (ctx == null) return;
        
        ProcessDamageReactions(ctx, simulation);
        UpdateConditionsOnDamage(ctx, simulation);
        CheckExpireIfNeeded(simulation);
    }

    public void HandleTurnStart()
    {
        var ctx = new EffectReactionContext(_target, _source);
        
        ProcessTurnReactions(ctx, _data.OnTurnStart);
        UpdateConditionsOnTurnStart();
        CheckExpireIfNeeded(false);
    }

    internal void HandleTurnEnd()
    {
        var ctx = new EffectReactionContext(_target, _source);
        
        ProcessTurnReactions(ctx, _data.OnTurnEnd);
        UpdateConditionsOnTurnEnd();
        CheckExpireIfNeeded(false);
    }

    public void HandleApply()
    {
        var applyCtx = new EffectReactionContext(_target, _source);
        
        ProcessTurnReactions(applyCtx, _data.OnApply);
        UpdateConditionsOnApply();
        CheckExpireIfNeeded(false);
    }

    public void HandleRemove()
    {
        var ctx = new EffectReactionContext(_target, _source);
        ProcessTurnReactions(ctx, _data.OnRemove);
    }

    /// <summary>
    /// Для UI: сколько ходов / уронов осталось до снятия
    /// </summary>
    public string[] GetExpirations()
    {
        try
        {
            return _conditions?.Select(c => c?.GetRemaining() ?? "Unknown")?.ToArray() 
                   ?? Array.Empty<string>();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error getting expirations: {ex.Message}");
            return Array.Empty<string>();
        }
    }

    // ГРУППА 2: Обработка реакций - отдельная ответственность
    private void ProcessDamageReactions(DamageContext ctx, bool simulation)
    {
        if (_data.OnOutDamage == null) return;
        
        foreach (var reaction in _data.OnOutDamage)
        {
            try
            {
                reaction.Execute(ctx, simulation);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error executing damage reaction: {ex.Message}");
            }
        }
    }

    private void ProcessTurnReactions(EffectReactionContext ctx, System.Collections.Generic.List<EffectReactionBase> reactions)
    {
        if (reactions == null) return;
        
        foreach (var reaction in reactions)
        {
            try
            {
                reaction.Execute(ctx);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error executing turn reaction: {ex.Message}");
            }
        }
    }

    // ГРУППА 3: Управление условиями - отдельная ответственность
    private void UpdateConditionsOnDamage(DamageContext ctx, bool simulation)
    {
        if (simulation || _conditions == null) return;
        
        foreach (var condition in _conditions)
        {
            try
            {
                condition?.OnOutDamage(ctx);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error updating condition: {ex.Message}");
            }
        }
    }

    private void UpdateConditionsOnTurnStart()
    {
        if (_conditions == null) return;
        
        foreach (var condition in _conditions)
        {
            try
            {
                condition?.OnTurnStarted();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error updating condition on turn: {ex.Message}");
            }
        }
    }

    private void UpdateConditionsOnTurnEnd()
    {
        if (_conditions == null) return;
        
        foreach (var condition in _conditions)
        {
            try
            {
                condition?.OnTurnEnded();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error updating condition on turn end: {ex.Message}");
            }
        }
    }

    private void UpdateConditionsOnApply()
    {
        if (_conditions == null) return;
        
        foreach (var condition in _conditions)
        {
            try
            {
                condition?.OnApply();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error updating condition on apply: {ex.Message}");
            }
        }
    }

    // ГРУППА 4: Управление жизненным циклом - отдельная ответственность
    private void CheckExpireIfNeeded(bool simulation)
    {
        if (simulation) return;
        
        if (_conditions?.Any(c => c?.ShouldRemove == true) == true)
        {
            HandleRemove();
        }
    }
}
