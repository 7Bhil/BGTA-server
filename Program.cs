using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using BGTA.Server.Data.Services;
using BGTA.Server.Services;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseStaticWebAssets();

// --- 1. SERVICES DE BASE ---
builder.Services.AddControllers();
builder.Services.AddRazorPages();
// Dans Program.cs - Section services (après builder.Services.AddRazorPages())
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.LogoutPath = "/Logout";
        
        // ðŸ”‘ TIMEOUT 1 HEURE après inactivité
        options.ExpireTimeSpan = TimeSpan.FromHours(1);
        
        // ðŸ”‘ RÉINITIALISATION du timer Ã  chaque requête utilisateur (sliding window)
        options.SlidingExpiration = true;
        
        // Security Cookie Config
        options.Cookie.HttpOnly = true;              // Bloque l'accès JavaScript au cookie
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // Compatible HTTP (développement) et HTTPS
        options.Cookie.SameSite = SameSiteMode.Lax; // Protection CSRF adaptée aux redirections
        options.Cookie.Name = "BGTA.Auth";         // Nom explicite
        
        // ðŸ”‘ GESTION PERSONNALISÉE des redirections
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.Redirect("/Login?expired=true");
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToLogin = context =>
        {
            // Préserve l'URL demandée pour redirection après login
            var returnUrl = context.Request.Path + context.Request.QueryString;
            context.Response.Redirect($"/Login?returnUrl={Uri.EscapeDataString(returnUrl)}&expired=true");
            return Task.CompletedTask;
        };
    });



builder.Services.AddServerSideBlazor()
    .AddCircuitOptions(options => { options.DetailedErrors = true; }); 
builder.Services.AddMudServices();
builder.Services.AddServerSideBlazor()
    .AddHubOptions(options =>
    {
        options.MaximumReceiveMessageSize = 30 * 1024 * 1024; // 10 Mo par exemple
    });

// --- 2. CONFIGURATION BAPI ---
builder.Services.Configure<BapiSettings>(builder.Configuration.GetSection("BapiSettings"));
builder.Services.AddHttpClient("BapiClient", (sp, client) =>
{
    var settings = sp.GetRequiredService<IOptions<BapiSettings>>().Value;
    client.BaseAddress = new Uri(settings.BaseUrl);
    
    var apiKey = builder.Configuration["BGTA_BAPI_API_KEY"] 
              ?? builder.Configuration["BapiSettings:ApiKey"]
              ?? Environment.GetEnvironmentVariable("BGTA_BAPI_API_KEY");

    if (!string.IsNullOrEmpty(apiKey))
    {
        client.DefaultRequestHeaders.Add("X-API-Key", apiKey);
    }
});

// --- 3. AUTHENTIFICATION & ÉTAT ---
// Ajout crucial pour la propagation de l'état dans les composants Razor
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => 
    sp.GetRequiredService<CustomAuthStateProvider>());


// --- 4. SERVICES MÉTIER ---
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<BapiService>();
builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<ILoginStateService, LoginStateService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddSingleton<LoginTransitionService>();
builder.Services.AddScoped<NavigationService>();
builder.Services.AddScoped<NavigationState>();

var app = builder.Build();

// --- 5. PIPELINE (Ordre strict) ---
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// L'ordre ici est impératif : Authentication -> Authorization -> Hub/Endpoints
app.UseAuthentication();
app.UseAuthorization();


app.MapControllers(); 
app.MapBlazorHub(); 
app.MapFallbackToPage("/_Host");

app.Run();