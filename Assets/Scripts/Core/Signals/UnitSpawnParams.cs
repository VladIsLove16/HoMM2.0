public class UnitSpawnParams
{
    public int X {  get; }
    public int Y {  get; }
    public int Amount {  get; }
    public UnitType UnitType {  get; }
    public Team Team { get; }
    public UnitSpawnParams(int x,int y,UnitType unitType = 0, int amount = 1, Team team = Team.Blue)
    {
        X= x;
        Y=y;
        Amount= amount; 
        UnitType= unitType;
        Team = team;
    }

    // Backwards compatible constructor for code/tests that still pass bool
    public UnitSpawnParams(int x,int y,UnitType unitType, int amount, bool isPlayer)
        : this(x,y,unitType,amount, isPlayer ? Team.Blue : Team.Red)
    {
    }

    public override string ToString()
    {
        return $"UnitSpawnParams(X={X}, Y={Y}, UnitType={UnitType}, Amount={Amount}, Team={Team})";
    }
}
