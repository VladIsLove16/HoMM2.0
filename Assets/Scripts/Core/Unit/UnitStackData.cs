public readonly struct UnitStackData
{
    public UnitStackData(UnitType unitType, int amount)
    {
        UnitType = unitType;
        Amount = amount;
    }

    public UnitType UnitType { get; }
    public int Amount { get; }
}
