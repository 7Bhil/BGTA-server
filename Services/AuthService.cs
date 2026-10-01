using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace BGTA.Server.Services;

public class AuthService : IAuthService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<bool> LoginAsync(string compte, string nomPrenom, int profilId, string profilLibelle)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return false;
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, compte),
            new Claim(ClaimTypes.Name, compte),
            new Claim("NomPrenom", nomPrenom),
            new Claim("ProfilId", profilId.ToString()),
            new Claim("ProfilLibelle", profilLibelle),
            new Claim(ClaimTypes.Role, profilLibelle)
        };
        
        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2)
        };
        
        await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
        return true;
    }
    
    public async Task LogoutAsync()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context != null)
        {
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}