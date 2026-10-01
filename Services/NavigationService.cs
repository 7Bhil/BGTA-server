using System;
using System.Collections.Generic;
using System.Linq;

namespace BGTA.Server.Services
{
    public class NavigationService
    {
        private readonly Dictionary<string, Type> _typeCache = new();

        public Type? GetPageType(string? formName)
        {
            if (string.IsNullOrEmpty(formName)) 
                return null;

            // ðŸ”‘ 1. Vérification du cache (performances)
            if (_typeCache.TryGetValue(formName, out var cachedType))
                return cachedType;

            // ðŸ”‘ 2. Récupération de l'assembly courant (BGTA.Server)
            var assembly = typeof(Program).Assembly;

            // ðŸ”‘ 3. Recherche FLEXIBLE avec variantes Razor
            // Les composants .razor sont compilés en classes avec suffixes spécifiques
            var possibleNames = new[]
            {
                formName,                      // "Profils"
                $"{formName}_razor",           // "Profils_razor" (suffixe standard Razor)
                $"{formName}Razor",            // "ProfilsRazor" (variantes possibles)
                $"{formName}_View"             // Variantes anciennes
            };

            Type? foundType = null;
            foreach (var name in possibleNames)
            {
                foundType = assembly.GetTypes()
                    .FirstOrDefault(t => 
                        t.Name.Equals(name, StringComparison.OrdinalIgnoreCase) &&
                        t.Namespace?.StartsWith("BGTA.Server.Pages") == true &&
                        !t.IsAbstract);
                
                if (foundType != null)
                    break;
            }

            // ðŸ”‘ 4. Mise en cache si trouvé
            if (foundType != null)
            {
                _typeCache[formName] = foundType;
                Console.WriteLine($"âœ… NavigationService : Type trouvé pour '{formName}' â†’ {foundType.FullName}");
            }
            else
            {
                // ðŸ”‘ 5. Diagnostic détaillé si échec (crucial pour le debug)
                Console.WriteLine($"âŒ NavigationService : Type NON TROUVÉ pour '{formName}'");
                Console.WriteLine($"   Noms testés : {string.Join(", ", possibleNames)}");
                
                // ðŸ”‘ 6. Liste des types Pages disponibles (pour identifier le nom exact)
                var pageTypes = assembly.GetTypes()
                    .Where(t => t.Namespace?.StartsWith("BGTA.Server.Pages") == true && !t.IsAbstract)
                    .Select(t => t.Name)
                    .OrderBy(t => t)
                    .ToList();
                
                Console.WriteLine($"   Types Pages disponibles ({pageTypes.Count}) :");
                foreach (var typeName in pageTypes.Take(20)) // Limite Ã  20 pour lisibilité
                {
                    Console.WriteLine($"     â€¢ {typeName}");
                }
                if (pageTypes.Count > 20)
                    Console.WriteLine($"     ... et {pageTypes.Count - 20} autres");
            }

            return foundType;
        }
    }
}