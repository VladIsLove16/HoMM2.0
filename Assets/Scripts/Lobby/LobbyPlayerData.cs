using System;
using Unity.Netcode;
using Unity.Collections;
/// <summary>
/// Данные игрока в лобби
/// </summary>
[System.Serializable]
public struct LobbyPlayerData : INetworkSerializable, IEquatable<LobbyPlayerData>
{
    public ulong ClientId;
    public FixedString128Bytes PlayerName;
    public bool IsReady;

    public bool Equals(LobbyPlayerData other)
    {
        return ClientId == other.ClientId;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref PlayerName);
        serializer.SerializeValue(ref IsReady);
    }
}



