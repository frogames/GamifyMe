using GamifyMe.Web.Components;
using MudBlazor.Services;
using GamifyMe.UI.Shared.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies; // <--- AJOUT IMPORTANT

var builder = WebApplication.CreateBuilder(args);

// 1. Services Razor Components
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// --- CORRECTION ICI : ON DÉFINIT LE SCHÉMA PAR DÉFAUT ---
// On dit au serveur : "Par défaut, utilise le système de Cookies pour gérer l'identité".
// C'est ce qui manque et cause l'erreur 500.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // Optionnel : On peut dire où est la page de login, 
        // même si notre composant RedirectToLogin le fait déjà.
        options.LoginPath = "/login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    });

builder.Services.AddCascadingAuthenticationState();
// ---------------------------------------------------------

// 2. Services MudBlazor & UI
builder.Services.AddMudServices();
builder.Services.AddScoped<GamifyMe.UI.Shared.Services.TokenStorageService>();

// 3. HttpClient pour le serveur
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("http://gamifyme-api:8080")
});

var app = builder.Build();

// Configuration du pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

// --- SÉCURITÉ ---
app.UseAuthentication();
app.UseAuthorization();
// ----------------

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(GamifyMe.Web.Client.Program).Assembly)
    .AddAdditionalAssemblies(typeof(GamifyMe.UI.Shared.Layout.MainLayout).Assembly);

app.Run();