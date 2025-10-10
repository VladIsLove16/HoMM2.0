namespace Adventure.Integration.Battle
{
    public readonly struct GridSlot
    {
        public GridSlot(int x, int y, UnitType unitType, int amount, Team team)
        {
            X = x;
            Y = y;
            UnitType = unitType;
            Amount = amount;
            Team = team;
        }

        public int X { get; }
        public int Y { get; }
        public UnitType UnitType { get; }
        public int Amount { get; }
        public Team Team { get; }
    }
}
