using System;
using UnityEngine;
public class SpellCasterService
{
    private readonly CombatController _combatController;
    private readonly SpellZoneFactory _zoneFactory;

    
    public SpellCasterService(
        CombatController combat,
        SpellZoneFactory zoneFactory)
    {
        _combatController = combat;
        _zoneFactory = zoneFactory;
    }

    public void Cast(SpellData data,
                     Vector2Int origin,
                     Func<Vector2Int, IEffectable?> getUnitAt,
                     ISpellCaster source)
    {
        var zone = _zoneFactory.Create(data.ZoneType);
        foreach (var offset in zone.GetCells())
        {
            var pos = origin + offset;
            var target = getUnitAt(pos);
            if (target == null) continue;
            _combatController.DealDamage(target, data.Damage, source);

            foreach (var se in data.StatusEffects)
            {
                var effect = new StatusEffect(se, target, source );
                _combatController.ApplyStatusEffect(target, effect);
            }
        }
    }
}
