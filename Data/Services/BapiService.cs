using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;

namespace BGTA.Server.Data.Services;

public class BapiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BapiService> _logger;
    private readonly int _defaultDatabaseTypeId;

    public BapiService(
        IHttpClientFactory httpClientFactory,
        IOptionsMonitor<BapiSettings> bapiSettings,
        ILogger<BapiService> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("BapiClient");
        
        // On stocke l'ID par défaut (SQL Server généralement) défini dans appsettings
        _defaultDatabaseTypeId = bapiSettings.CurrentValue.DatabaseTypeId;
        
        var apiKey = configuration["BGTA_BAPI_API_KEY"] ?? configuration["BapiSettings:ApiKey"];
        
        if (!string.IsNullOrEmpty(apiKey))
        {
            // Sécurité : on nettoie avant d'ajouter pour éviter les doublons
            _httpClient.DefaultRequestHeaders.Remove("X-API-Key");
            _httpClient.DefaultRequestHeaders.Add("X-API-Key", apiKey);
            _logger.LogInformation("âœ… Clé API BAPI chargée avec succès.");
        }
        else
        {
            _logger.LogWarning("âš ï¸ Clé API manquante ! Vérifiez vos User Secrets.");
        }
    }

    /// <summary>
    /// Exécute une procédure stockée sur une base de données spécifique.
    /// </summary>
    /// <param name="databaseTypeId">ID du type de base (ex: 1 pour SQL, 2 pour Access)</param>
    public async Task<Dictionary<string, object?>[]> ExecuteProcedureAsync(
        int databaseTypeId, 
        string procedureKey,
        object?[] parameters,
        CancellationToken ct = default)
    {
        var request = new
        {
            DatabaseTypeId = databaseTypeId,
            ProcedureKey = procedureKey,
            Parameters = parameters
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/Execution/ExecutePs", request, ct);
            
            // Si l'API renvoie 401, cela sera attrapé ici
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<Dictionary<string, object?>[]>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, ct) 
                ?? Array.Empty<Dictionary<string, object?>>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur BAPI sur DB {DbType} : {Message}", databaseTypeId, ex.Message);
            throw;
        }
    }
}

public class BapiSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public int DatabaseTypeId { get; set; }
}