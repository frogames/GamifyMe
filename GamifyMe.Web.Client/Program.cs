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

            // --- Enregistrement des services ---

            // 1. HttpClient pour parler à l'API (on utilise l'adresse relative
            //    car le projet Web va héberger le Client)
            builder.Services.AddScoped(sp => new HttpClient
            {
                BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
            });

            // 2. Services MudBlazor
            builder.Services.AddMudServices();

            // 3. Services d'Authentification (les mêmes que dans MauiProgram)
            builder.Services.AddAuthorizationCore();
            builder.Services.AddScoped<TokenStorageService>();
            builder.Services.AddScoped<ApiAuthenticationStateProvider>();
            builder.Services.AddScoped<AuthenticationStateProvider>(provider =>
                provider.GetRequiredService<ApiAuthenticationStateProvider>());

            // On dit à Blazor de démarrer en utilisant notre composant App
            // (qui se trouve dans GamifyMe.Web/Components/App.razor)
            // Mais ici, on pointe vers le composant App du projet Client s'il existe,
            // ou on suppose que le projet Web s'en charge.
            // Pour un projet client "pur" hébergé, on définit le RootComponent.
            // Dans notre cas (Blazor Web App), le projet Web s'en charge.
            // Mais le fichier Program.cs doit exister.

            // Correction : Dans un projet Blazor Web App, le Program.cs du client
            // n'a pas besoin de "builder.RootComponents.Add".
            // Il ne fait QUE enregistrer les services.

            await builder.Build().RunAsync();
        }
    }
}