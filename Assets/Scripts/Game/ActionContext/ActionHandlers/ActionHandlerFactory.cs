using UnityEngine;
using Zenject;

/// <summary>
/// Factory for creating ActionHandler instances
/// </summary>
public class ActionHandlerFactory
{
    [Inject] private DiContainer _container;

    public IActionHandler CreateMoveHandler(ICombatObject unit)
    {
        var handler = new MoveActionHandler(unit, _container.Resolve<MovementSystem>());
        _container.Inject(handler);
        return handler;
    }

    public IActionHandler CreateRangedAttackHandler(ICombatObject unit)
    {
        var handler = new RangedAttackHandler(unit);
        _container.Inject(handler);
        return handler;
    }

    public IActionHandler CreateMoveThenAttackHandler(ICombatObject unit)
    {
        var handler = new MoveThenAttackHandler(unit);
        _container.Inject(handler);
        return handler;
    }

    public IActionHandler CreateSpellHandler(IEffectApplier caster)
    {
        var handler = new SpellActionHandler(_container.Resolve<SpellCasterService>());
        _container.Inject(handler);
        return handler;
    }
}
