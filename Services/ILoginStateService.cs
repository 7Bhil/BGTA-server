namespace BGTA.Server.Services;

public interface ILoginStateService
{
    string CreateSession(LoginState state);
    LoginState? GetSession(string sessionId);
    void RemoveSession(string sessionId);
}