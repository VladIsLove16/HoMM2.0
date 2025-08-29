public interface ICombatant : IDamageSource, IEffectApplier
{
    // Объединяет возможности нанесения урона и применения эффектов
    // Следует принципу композиции вместо наследования
}
