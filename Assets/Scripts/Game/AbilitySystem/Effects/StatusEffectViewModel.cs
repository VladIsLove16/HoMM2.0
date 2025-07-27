using UnityEngine;

public class StatusEffectViewModel
{
    
    private StatusEffect StatusEffect { get; set; }
    public ExpirtationConditionViewModel expirtationConditionViewModel { get; }
    public StatusEffectType Type { get; }
    public string Name { get; }

    public StatusEffectViewModel(StatusEffect statusEffect)
    {
        this.StatusEffect = statusEffect;
        Type = statusEffect.Type;
        Name = statusEffect.Name;
    }
}
