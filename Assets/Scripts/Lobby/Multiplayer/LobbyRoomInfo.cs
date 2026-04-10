using System;

public readonly struct LobbyRoomInfo
{
    public LobbyRoomInfo(
        string sessionId,
        string name,
        string hostName,
        string flow,
        int playerCount,
        int maxPlayers,
        int availableSlots,
        bool isLocked,
        bool hasPassword,
        DateTime lastUpdated)
    {
        SessionId = sessionId;
        Name = name;
        HostName = hostName;
        Flow = flow ?? string.Empty;
        PlayerCount = playerCount;
        MaxPlayers = maxPlayers;
        AvailableSlots = availableSlots;
        IsLocked = isLocked;
        HasPassword = hasPassword;
        LastUpdated = lastUpdated;
    }

    public string SessionId { get; }
    public string Name { get; }
    public string HostName { get; }
    public string Flow { get; }
    public int PlayerCount { get; }
    public int MaxPlayers { get; }
    public int AvailableSlots { get; }
    public bool IsLocked { get; }
    public bool HasPassword { get; }
    public DateTime LastUpdated { get; }
}
