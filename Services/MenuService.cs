using BGTA.Server.Data.Services;
using BGTA.Server.Models;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Collections.Concurrent; // Nécessaire pour ConcurrentDictionary

namespace BGTA.Server.Services;

public class MenuService : IMenuService
{
    private readonly BapiService _bapiService;
    private readonly ILogger<MenuService> _logger;
    private readonly AuthenticationStateProvider _authStateProvider;

    // ðŸ”‘ LE CACHE STATIQUE : Partagé par l'application mais cloisonné par ProfilId
    private static readonly ConcurrentDictionary<int, List<MenuItem>> _globalMenuCache = new();

    public MenuService(BapiService bapiService, ILogger<MenuService> logger, AuthenticationStateProvider authStateProvider)
    {
        _bapiService = bapiService;
        _logger = logger;
        _authStateProvider = authStateProvider;
    }

    public async Task<List<MenuItem>> GetMenuItemsAsync(string? applicationId = null)
    {
        // --- ÉTAPE B : AUTHENTIFICATION (Sécurité) ---
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        var profilIdClaim = user.FindFirst("ProfilId")?.Value;

        if (string.IsNullOrEmpty(profilIdClaim) || !int.TryParse(profilIdClaim, out int profilId))
        {
            _logger.LogError("MenuService : Aucun ProfilId valide trouvé.");
            return GetFallbackMenu();
        }

        // --- ÉTAPE A : VÉRIFICATION DU CACHE STATIQUE ---
        if (_globalMenuCache.TryGetValue(profilId, out var cachedMenu))
        {
            _logger.LogDebug("Menu retourne depuis le cache global pour le profil {ProfilId}", profilId);
            return cachedMenu;
        }

        try
        {
            _logger.LogInformation("Chargement des menus BAPI pour le profil {ProfilId}...", profilId);

            var allowedMenuIds = await GetAllowedMenuIdsForProfilAsync(profilId);
            var allMenus = await LoadAllMenusAsync();

            var filteredMenus = allMenus
                .Where(m => allowedMenuIds.Count == 0 || allowedMenuIds.Contains(m.Id))
                .ToList();

            var hierarchicalMenus = BuildMenuHierarchy(filteredMenus);

            _globalMenuCache.TryAdd(profilId, hierarchicalMenus);

            _logger.LogInformation("Cache menu cree : {RootCount} racines pour le profil {ProfilId}", 
                hierarchicalMenus.Count, profilId);

            return hierarchicalMenus;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du chargement des menus pour le profil {ProfilId}", profilId);
            return GetFallbackMenu();
        }
    }

    private async Task<HashSet<int>> GetAllowedMenuIdsForProfilAsync(int profilId)
    {
        var allowedIds = new HashSet<int>();
        try
        {
            var result = await _bapiService.ExecuteProcedureAsync(1, "PS20026", new object?[] { profilId }, CancellationToken.None);
            foreach (var row in result)
            {
                if (int.TryParse(row.GetValueOrDefault("NumMen")?.ToString(), out var numMen) && numMen > 0)
                {
                    allowedIds.Add(numMen);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Impossible de charger les autorisations de menu pour profil {ProfilId}, chargement global", profilId);
        }
        return allowedIds;
    }

    private async Task<List<MenuItem>> LoadAllMenusAsync()
    {
        var result = await _bapiService.ExecuteProcedureAsync(1, "PS20004", Array.Empty<object?>(), CancellationToken.None);

        var menus = new List<MenuItem>();
        foreach (var row in result)
        {
            try
            {
                var level = int.TryParse(row.GetValueOrDefault("NivMen")?.ToString(), out var lvl) ? lvl : 1;
                var menuItem = CreateMenuItemFromRow(row, level);
                if (menuItem.Id > 0 && menuItem.IsVisible)
                {
                    menus.Add(menuItem);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erreur lors de la creation d'un MenuItem");
            }
        }
        return menus;
    }

    private MenuItem CreateMenuItemFromRow(Dictionary<string, object?> row, int level)
    {
        int? GetInt(string col) => int.TryParse(row.GetValueOrDefault(col)?.ToString(), out var v) ? v : null;
        string? GetString(string col) => row.GetValueOrDefault(col)?.ToString()?.Trim();
        bool GetBool(string col) => bool.TryParse(row.GetValueOrDefault(col)?.ToString(), out var v) ? v : false;

        return new MenuItem
        {
            Id = GetInt("NumMen") ?? 0,
            Text = GetString("LibMen") ?? "Sans titre",
            Level = level,
            ParentId = GetInt("NumNivSup"),
            Icon = GetString("Image1") ?? "fas fa-circle",
            IconSelected = GetString("Image2"),
            ImageInbox = GetString("ImageInbox"),
            ImageListview = GetString("ImageListview"),
            DescriptionMenu = GetString("DescriptionMenu"),
            Order = GetInt("Ordre") ?? 0,
            CodeMenu = GetString("CodeMenu"),
            AfficheInbox = GetBool("AfficheInbox"),
            IdWFStepInValue = GetInt("IdWFStepInValue"),
            Formulaire = GetString("Formulaire"),
            MasquerFrmTitre = GetBool("MasquerFrmTitre"),
            DescriptionFormulaire = GetString("DescriptionFormulaire"),
            AffichageMultiple = GetString("AffichageMultiple"),
            NomTable = GetString("NomTable"),
            Supprimer = GetBool("Supprimer"),
            OrdGen = GetInt("OrdGen"),
            CodeControle = GetString("CodeControle"),
            OptionEtat = GetString("OptionEtat"),
            NomEtat = GetString("NomEtat"),
            NumeroPS = GetString("NumeroPS"),
            Category = GetString("CategorieMenu"),
            TagListview = GetString("TagListview"),
            MenuRaccourci = GetBool("MenuRaccourci"),
            APT_ID_FK = GetInt("APT_ID_FK"),
            BeforeConnection = GetBool("BeforeConnection"),
            AfterConnection = GetBool("AfterConnection"),
            Image1Nom = GetString("Image1Nom"),
            Image2Nom = GetString("Image2Nom"),
            IsVisible = !GetBool("Supprimer"),
            Url = GenerateUrl(row)
        };
    }

    private string GenerateUrl(Dictionary<string, object?> row)
    {
        var formulaire = row.GetValueOrDefault("Formulaire")?.ToString()?.Trim();
        var idWFStepInValue = int.TryParse(row.GetValueOrDefault("IdWFStepInValue")?.ToString(), out var id) ? id : (int?)null;
        bool.TryParse(row.GetValueOrDefault("AfficheInbox")?.ToString(), out bool afficheInbox);

        if (!string.IsNullOrEmpty(formulaire) && !afficheInbox)
        {
            try
            {
                var formParts = formulaire.Split('.');
                var formName = formParts.Length > 0 ? formParts[^1] : formulaire;
                return idWFStepInValue.HasValue ? $"/{formName.ToLower()}?etat={idWFStepInValue.Value}" : $"/{formName.ToLower()}";
            }
            catch (Exception ex) { _logger.LogWarning(ex, "Erreur génération URL pour formulaire={Formulaire}", formulaire); }
        }
        var codeMenu = row.GetValueOrDefault("CodeMenu")?.ToString()?.Trim();
        return !string.IsNullOrEmpty(codeMenu) ? $"#menu-{codeMenu}" : "#";
    }

    private List<MenuItem> BuildMenuHierarchy(List<MenuItem> flatMenus)
    {
        if (flatMenus == null || !flatMenus.Any()) 
            return new List<MenuItem>();

        var cleanList = flatMenus
            .Where(m => m.Id > 0)
            .GroupBy(m => m.Id)
            .Select(g => g.First())
            .ToList();

        var lookup = new Dictionary<int, MenuItem>();
        foreach (var menu in cleanList)
        {
            if (!lookup.ContainsKey(menu.Id))
            {
                lookup[menu.Id] = menu;
                menu.Children = new List<MenuItem>();
            }
        }

        var rootMenus = new List<MenuItem>();
        var orphans = new List<MenuItem>();

        foreach (var menu in cleanList)
        {
            if (menu.ParentId.HasValue && lookup.TryGetValue(menu.ParentId.Value, out var parent))
            {
                if (!parent.Children.Any(c => c.Id == menu.Id))
                {
                    parent.Children.Add(menu);
                    menu.Parent = parent;
                }
            }
            else
            {
                if (!menu.ParentId.HasValue) rootMenus.Add(menu);
                else orphans.Add(menu);
            }
        }

        rootMenus.AddRange(orphans);

        foreach (var menu in lookup.Values)
        {
            menu.Children = menu.Children.OrderBy(c => c.Order).ToList();
        }

        rootMenus = rootMenus.OrderBy(m => m.Order).ToList();

        _logger.LogInformation("=== RÉSUMÉ HIÉRARCHIE ===");
        foreach (var root in rootMenus.Where(m => !m.ParentId.HasValue).OrderBy(m => m.Order))
        {
            LogMenuTree(root, 0);
        }

        return rootMenus;
    }

    private void LogMenuTree(MenuItem menu, int indent)
    {
        var prefix = new string(' ', indent * 2);
        _logger.LogInformation("{Prefix}â”œâ”€ [{Id}] {Text} (Enfants: {ChildCount})", 
            prefix, menu.Id, menu.Text, menu.Children.Count);
        
        foreach (var child in menu.Children.OrderBy(c => c.Order))
        {
            LogMenuTree(child, indent + 1);
        }
    }

 private List<MenuItem> GetFallbackMenu()
    {
        return new List<MenuItem>
        {
            new MenuItem { Id = 1, Text = "Accueil", Url = "/", Icon = "fas fa-home", Order = 1 },
            new MenuItem { Id = 2, Text = "PTA", Url = "/pta", Icon = "fas fa-tasks", Order = 2 },
            new MenuItem { Id = 3, Text = "Plans", Url = "/plan", Icon = "fas fa-file-contract", Order = 3 }
        };
    }

   
}

