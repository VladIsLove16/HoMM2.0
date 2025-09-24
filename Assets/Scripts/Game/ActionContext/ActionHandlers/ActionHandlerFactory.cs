using Zenject;

public class ActionHandlerFactory
{
    [Inject] private MoveActionHandlerFactory _moveFactory;
    [Inject] private RangedAttackHandlerFactory _rangedFactory;
    [Inject] private MoveThenAttackHandlerFactory _moveThenAttackFactory;
    [Inject] private SpellHandlerFactory _spellFactory;

    public IActionHandler CreateMoveHandler(ICombatObject unit)
        => _moveFactory.Create(unit);

    public IActionHandler CreateRangedAttackHandler(ICombatObject unit)
        => _rangedFactory.Create(unit);

    public IActionHandler CreateMoveThenAttackHandler(ICombatObject unit)
        => _moveThenAttackFactory.Create(unit);

    public IActionHandler CreateSpellHandler(IEffectApplier caster)
        => _spellFactory.Create(caster);
}
