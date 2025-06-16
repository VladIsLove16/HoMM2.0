using System;

public class SpellZoneFactory
{
    public ISpellZone Create(SpellZoneType type)
    {
        return type switch
        {
            SpellZoneType.Cross => new СrossSpellZone(),
            SpellZoneType.Quad => new QuadSpellZone(),
            _ => throw new NotImplementedException(),
        };
    }
}
