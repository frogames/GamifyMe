using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor.Services;
using GamifyMe.UI.Shared.Layout;
using GamifyMe.UI.Shared.Services;
using GamifyMe.Web.Components;
using GamifyMe.Web.Services;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// --- 1. Services Côté Serveur ---
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddHttpClient();
builder.Services.AddMudServices();

// --- CORRECTION ---
// Il faut AJOUTER AddAuthentication() pour que app.UseAuthentication() fonctionne.
// On ajoute un schéma "Cookies" par défaut pour la partie serveur.
builder.Services.AddAuthentication("Cookies")
    .AddCookie();
// --- FIN CORRECTION ---

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, ServerSideAuthStateProvider>();

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "http://api.gamifyme.fun:8080")
});

var app = builder.Build();

// --- 2. Pipeline HTTP ---
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseBlazorFrameworkFiles();
app.UseAntiforgery();

app.UseAuthentication(); // Cette ligne a maintenant le service dont elle a besoin
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(MainLayout).Assembly);

app.Run();