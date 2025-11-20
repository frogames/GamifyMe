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
                if (builder.HostEnvironment.IsDevelopment())
                {
                    client.BaseAddress = new Uri("http://localhost:5000");
                }
                else
                {
                    client.BaseAddress = new Uri("https://api.gamifyme.fun");
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

            await builder.Build().RunAsync();
        }
    }
}