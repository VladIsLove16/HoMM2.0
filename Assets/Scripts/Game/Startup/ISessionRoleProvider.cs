public enum SessionRole { Local, Host, Client }

public interface ISessionRoleProvider
{
    SessionRole CurrentRole { get; }
}


