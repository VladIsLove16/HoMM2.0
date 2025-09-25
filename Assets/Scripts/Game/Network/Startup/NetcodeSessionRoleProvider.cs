using Unity.Netcode;

public class NetcodeSessionRoleProvider : ISessionRoleProvider
{
    public SessionRole CurrentRole
    {
        get
        {
            if (NetworkManager.Singleton == null)
            {
                return SessionRole.Local;
            }
            if (NetworkManager.Singleton.IsHost)
            {
                return SessionRole.Host;
            }
            return SessionRole.Client;
        }
    }
}


