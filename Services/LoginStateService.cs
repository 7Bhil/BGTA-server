using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;

namespace BGTA.Server.Services;

// ðŸ”‘ Définitions partagées (accessibles depuis Login.razor via @using)
public class LoginState
{
    public string Compte { get; set; } = string.Empty;
    public string HashBcrypt { get; set; } = string.Empty;
    public string NomPrenom { get; set; } = string.Empty;
    public List<ProfilItem> Profils { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class ProfilItem // âœ… Déplacé ici (accessible partout dans le namespace)
{
    public int IdProfil { get; set; }
    public string LibProfil { get; set; } = string.Empty;
    public bool EstDefaut { get; set; }
}

public class LoginStateService : ILoginStateService
{
    private readonly IMemoryCache _cache;
    private const int SESSION_TIMEOUT_MINUTES = 5;

    public LoginStateService(IMemoryCache cache) => _cache = cache;

    public string CreateSession(LoginState state)
    {
        var sessionId = Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
        _cache.Set(sessionId, state, TimeSpan.FromMinutes(SESSION_TIMEOUT_MINUTES));
        return sessionId;
    }

    public LoginState? GetSession(string sessionId) => 
        _cache.TryGetValue(sessionId, out LoginState? state) && 
          state != null &&
        (DateTime.UtcNow - state.CreatedAt <= TimeSpan.FromMinutes(SESSION_TIMEOUT_MINUTES)) 
            ? state 
            : null;

    public void RemoveSession(string sessionId) => _cache.Remove(sessionId);
}