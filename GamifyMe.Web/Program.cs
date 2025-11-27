using GamifyMe.Web.Components;
using MudBlazor.Services;
using GamifyMe.UI.Shared.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// 1. Services Razor Components
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    });

builder.Services.AddCascadingAuthenticationState();

// 2. Services MudBlazor & UI
builder.Services.AddMudServices();
builder.Services.AddScoped<GamifyMe.UI.Shared.Services.TokenStorageService>();
builder.Services.AddScoped<GamifyMe.UI.Shared.Services.ThemeService>();

// 3. HttpClient pour le serveur
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "http://gamifyme-api:8080")
});

// Configure ForwardedHeaders for reverse proxy
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor |
                               Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto;
});

var app = builder.Build();

app.UseForwardedHeaders();
app.UseCors("AllowBlazorClient");

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

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();   // uniquement en dev
}
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