using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Threading.Tasks;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CustomAuthStateProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        // On récupère le User depuis le contexte
        var user = _httpContextAccessor.HttpContext?.User;

        // Correction du CS8602 : 
        // On vérifie que 'user', 'user.Identity' et le flag 'IsAuthenticated' ne sont pas nulls
        if (user?.Identity?.IsAuthenticated == true)
        {
            return Task.FromResult(new AuthenticationState(user));
        }

        // Sinon, on retourne un utilisateur vide (anonyme)
        return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
    }

    public void MarkUserAsAuthenticated(string username, int profilId)
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}