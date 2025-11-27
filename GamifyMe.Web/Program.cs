using GamifyMe.Web.Components;
using MudBlazor.Services;
using GamifyMe.UI.Shared.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// --------------------------------------------------------------------
// 1️⃣ Services
// --------------------------------------------------------------------
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

// MudBlazor UI
builder.Services.AddMudServices();
builder.Services.AddScoped<TokenStorageService>();
builder.Services.AddScoped<ThemeService>();

// HttpClient used by the Blazor client to call the API
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(
        builder.Configuration["ApiBaseUrl"] ?? "http://gamifyme-api:8080")
});

// Forwarded‑headers (reverse‑proxy)
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;
});

var app = builder.Build();

// --------------------------------------------------------------------
// 2️⃣ Middleware pipeline
// --------------------------------------------------------------------
app.UseForwardedHeaders();
app.UseCors("AllowBlazorClient");   // <-- CORS must be before static files

// Serve the Blazor WebAssembly static assets
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

// Fallback for client‑side routing (index.html)
app.MapFallbackToFile("index.html");

// --------------------------------------------------------------------
// 3️⃣ Environment‑specific pipeline
// --------------------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

// HTTPS redirection only in development (production handled by proxy)
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAntiforgery();

// --------------------------------------------------------------------
// 4️⃣ Security
// --------------------------------------------------------------------
app.UseAuthentication();
app.UseAuthorization();

// --------------------------------------------------------------------
// 5️⃣ Razor component mapping
// --------------------------------------------------------------------
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(GamifyMe.Web.Client.Program).Assembly)
    .AddAdditionalAssemblies(typeof(GamifyMe.UI.Shared.Layout.MainLayout).Assembly);

app.Run();