using System;
using UnityEngine;
public class SpellCasterService
{
    private readonly SpellZoneFactory _zoneFactory;

    public SpellCasterService(
        SpellZoneFactory zoneFactory)
    {
        _zoneFactory = zoneFactory;
    }

    public void Cast(SpellData data,
                     Vector2Int origin,
                     Func<Vector2Int, IEffectable> getUnitAt,
                     IEffectApplier source)
    {
        var zone = _zoneFactory.Create(data.ZoneType);
        foreach (var offset in zone.GetCells())
        {
            var pos = origin + offset;
            var target = getUnitAt(pos);
            if (target == null) continue;
            DamageContext damageContext = new(data.Damage, data.DamageType, source);
            target.RecieveDamage(damageContext);
            //_combatController.DealDamage(target, data.Damage, source);

            foreach (var seData in data.StatusEffects)
            {
                StatusEffect statusEffect = new(seData, target, source);
                target.ApplyEffect(statusEffect);
            }
        }
    }
}
