public class UnitSpawnParams
{
    public int X {  get; }
    public int Y {  get; }
    public int Amount {  get; }
    public UnitType UnitType {  get; }
    public bool IsPlayer { get; }
    public UnitSpawnParams()
    {

    }
    public UnitSpawnParams(int x,int y,UnitType unitType = 0, int amount = 1, bool isPlayer = true)
    {
        X= x;
        Y=y;
        Amount= amount; 
        UnitType= unitType;
        IsPlayer = isPlayer;
    }

    public override string ToString()
    {
        return $"UnitSpawnParams(X={X}, Y={Y}, UnitType={UnitType}, Amount={Amount}, IsPlayer={IsPlayer})";
    }
}
