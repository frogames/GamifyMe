using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
// On garde ce using pour le JwtHandler
using GamifyMe.Web.Client.Services;

namespace GamifyMe.Web.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            // --- 1. GESTION DU TOKEN JWT ---
            builder.Services.AddTransient<JwtHandler>();

            builder.Services.AddHttpClient("GamifyMeApi", client =>
            {
                // DYNAMIQUE : Localhost en dev, Domaine réel en prod
                var baseAddress = builder.HostEnvironment.BaseAddress;
                
                // Si l'URL contient "localhost", on suppose qu'on est en dev local
                if (baseAddress.Contains("localhost"))
                {
                    // En local, on tape directement sur l'API (port 5000)
                    client.BaseAddress = new Uri("http://localhost:5000");
                }
                else
                {
                    // En production (gamifyme.fun), on utilise l'URL d'origine
                    // Nginx se chargera de router /api vers le backend
                    client.BaseAddress = new Uri(baseAddress);
                }
            })
            .AddHttpMessageHandler<JwtHandler>();

            builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("GamifyMeApi"));

            // --- 2. Services UI ---
            builder.Services.AddMudServices();

            // --- 3. Services d'Authentification (CORRECTION ICI) ---

            // On force l'utilisation du namespace UI.Shared pour lever l'ambiguïté
            builder.Services.AddScoped<GamifyMe.UI.Shared.Services.TokenStorageService>();

            builder.Services.AddAuthorizationCore();

            // Idem ici, on force la version UI.Shared
            builder.Services.AddScoped<GamifyMe.UI.Shared.Services.ApiAuthenticationStateProvider>();

            builder.Services.AddScoped<AuthenticationStateProvider>(provider =>
                provider.GetRequiredService<GamifyMe.UI.Shared.Services.ApiAuthenticationStateProvider>());

            builder.Services.AddScoped<GamifyMe.UI.Shared.Services.ThemeService>();
            builder.Services.AddScoped<GamifyMe.UI.Shared.Services.UserStateService>();

            await builder.Build().RunAsync();
        }
    }
}