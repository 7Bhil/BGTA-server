namespace BGTA.Server.Services;

public interface IAuthService
{
    Task<bool> LoginAsync(string compte, string nomPrenom, int profilId, string profilLibelle);
    Task LogoutAsync();
}