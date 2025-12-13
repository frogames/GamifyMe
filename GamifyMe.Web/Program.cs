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

builder.Services.AddLocalization();

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
builder.Services.AddScoped<GamifyMe.UI.Shared.Services.UserStateService>();
builder.Services.AddScoped<GamifyMe.UI.Shared.Services.NotificationService>();

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

var supportedCultures = new[]
{
    new System.Globalization.CultureInfo("fr"),
    new System.Globalization.CultureInfo("en"),
    new System.Globalization.CultureInfo("de"),
    new System.Globalization.CultureInfo("es")
};
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[0].Name)
    .AddSupportedCultures(supportedCultures.Select(c => c.Name).ToArray())
    .AddSupportedUICultures(supportedCultures.Select(c => c.Name).ToArray());

// FORCE the Cookie Provider to be the first one to be checked
localizationOptions.RequestCultureProviders.Clear();
var cookieProvider = new Microsoft.AspNetCore.Localization.CookieRequestCultureProvider
{
    CookieName = "MeritoPassCulture"
};
localizationOptions.RequestCultureProviders.Add(cookieProvider);
localizationOptions.RequestCultureProviders.Add(new Microsoft.AspNetCore.Localization.QueryStringRequestCultureProvider());
localizationOptions.RequestCultureProviders.Add(new Microsoft.AspNetCore.Localization.AcceptLanguageHeaderRequestCultureProvider());

app.UseRequestLocalization(localizationOptions);

app.UseAntiforgery();

// --- SÉCURITÉ ---
app.UseAuthentication();
app.UseAuthorization();
// ----------------

app.MapGet("/Culture/Set", (HttpContext context, string culture, string redirectUri) =>
{
    Console.WriteLine($"[Culture/Set] Request to set culture: {culture}, Redirect: {redirectUri}");
    if (culture != null)
    {
        var cookieName = "MeritoPassCulture";
        var cookieValue = Microsoft.AspNetCore.Localization.CookieRequestCultureProvider.MakeCookieValue(
            new Microsoft.AspNetCore.Localization.RequestCulture(culture, culture));

        context.Response.Cookies.Append(cookieName, cookieValue, new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddYears(1),
            IsEssential = true,
            SameSite = SameSiteMode.Lax,
            HttpOnly = false,
            Secure = false,
            Path = "/"
        });

        context.Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
        context.Response.Headers.Append("Pragma", "no-cache");
        context.Response.Headers.Append("Expires", "0");
    }
    return Results.LocalRedirect(redirectUri);
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(GamifyMe.Web.Client.Program).Assembly)
    .AddAdditionalAssemblies(typeof(GamifyMe.UI.Shared.Layout.MainLayout).Assembly);



app.MapGet("/debug-api", () => "API is working!");
app.MapGet("/version", () => new { Version = "1.0.2", Timestamp = DateTime.UtcNow, Note = "Fix Cookie Persistence" });

app.Run();