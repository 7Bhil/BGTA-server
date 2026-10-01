using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BGTA.Server.Services; // Assurez-vous d'importer le namespace de votre service

namespace BGTA.Server.Controllers
{
    [Route("Account")]
    public class AccountController : Controller
    {
        private readonly LoginTransitionService _transitionService;

        // Injection du service singleton via le constructeur
        public AccountController(LoginTransitionService transitionService)
        {
            _transitionService = transitionService;
        }

        [HttpGet("LoginProcess")]
        public async Task<IActionResult> LoginProcess()
        {
            // Récupération des données depuis le service en mémoire
            var username = _transitionService.Username;
            var profilId = _transitionService.ProfilId;

            // Vérification de sécurité : si le service est vide, redirection forcée
            if (string.IsNullOrEmpty(username) || profilId == 0)
            {
                return Redirect("/Login");
            }

            // Nettoyage immédiat pour ne laisser aucune donnée sensible en mémoire
            _transitionService.Clear();

            var claims = new List<Claim> {
                new Claim(ClaimTypes.Name, username),
                new Claim("ProfilId", profilId.ToString())
            };
            
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties { IsPersistent = true }
            );

            return Redirect("/");
        }


        [HttpGet("LogoutProcess")]
        public async Task<IActionResult> LogoutProcess()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("/Login");
        }
    }
}