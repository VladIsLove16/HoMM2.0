public interface IEffectable : IDamagable
{
    public void ApplyStatusEffect(StatusEffect statusEffect);
    public UnitStats ModifiedStats { get; set; }    
    public void RemoveEffect(StatusEffect statusEffect);
}
