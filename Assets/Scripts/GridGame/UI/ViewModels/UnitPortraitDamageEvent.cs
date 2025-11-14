public readonly struct UnitPortraitDamageEvent
{
    public UnitPortraitDamageEvent(int amount, int died)
    {
        Amount = amount;
        DieAmount = died;
    }

    public int Amount { get; }
    public int DieAmount { get; }
    public bool IsLethal => DieAmount > 0;
}
