public interface IEffectable : IDamagable
{
    void ApplyEffect(StatusEffect statusEffect);
    void RemoveEffect(StatusEffect statusEffect);
    UnitStats ModifiedStats { get; set; }
}
