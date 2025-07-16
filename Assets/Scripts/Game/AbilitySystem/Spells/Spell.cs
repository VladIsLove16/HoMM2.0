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
                     Func<Vector2Int, IEffectable> getUnitAt,
                     ISpellCaster source)
    {
        var zone = _zoneFactory.Create(data.ZoneType);
        foreach (var offset in zone.GetCells())
        {
            var pos = origin + offset;
            var target = getUnitAt(pos);
            if (target == null) continue;
            DamageContext damageContext = new(data.Damage, data.DamageType, source);
            target.ReceiveDamage(damageContext);
            //_combatController.DealDamage(target, data.Damage, source);

            foreach (var se in data.StatusEffects)
            {
                target.ApplyEffect(se, source);
            }
        }
    }
}
