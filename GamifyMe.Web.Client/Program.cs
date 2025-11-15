using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using GamifyMe.UI.Shared.Services;

namespace GamifyMe.Web.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            // --- 1. HttpClient (LA CORRECTION EST ICI) ---
            // On ne peut plus utiliser l'adresse de base de l'hôte.
            // On doit "en dur" pointer vers le sous-domaine de notre API.
            builder.Services.AddScoped(sp => new HttpClient
            {
                // Le port :8080 est celui que nous avons défini dans le docker-compose.yml
                BaseAddress = new Uri("http://api.gamifyme.fun:8080")
            });

            // --- 2. Services MudBlazor ---
            builder.Services.AddMudServices();

            // --- 3. Services d'Authentification ---
            builder.Services.AddAuthorizationCore();
            builder.Services.AddScoped<TokenStorageService>();
            builder.Services.AddScoped<ApiAuthenticationStateProvider>();
            builder.Services.AddScoped<AuthenticationStateProvider>(provider =>
                provider.GetRequiredService<ApiAuthenticationStateProvider>());

            await builder.Build().RunAsync();
        }
    }
}