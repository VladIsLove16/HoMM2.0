
/// <summary>
/// Логика статус эффектов
/// </summary>
public interface ISpellCaster : IDamageSource, IEffectApplier
{
    public int SpellPower {  get; set; }
}
