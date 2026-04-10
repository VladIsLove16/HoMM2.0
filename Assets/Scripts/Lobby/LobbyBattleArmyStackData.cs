using Unity.Netcode;

public struct LobbyBattleArmyStackData : INetworkSerializable
{
    public LobbyBattleArmyStackData(int unitType, int amount)
    {
        UnitType = unitType;
        Amount = amount;
    }

    public int UnitType;
    public int Amount;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref UnitType);
        serializer.SerializeValue(ref Amount);
    }
}
