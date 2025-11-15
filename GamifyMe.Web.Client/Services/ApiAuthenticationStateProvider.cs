using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;

namespace GamifyMe.Web.Client.Services
{
    public class ApiAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly HttpClient _httpClient;
        private readonly TokenStorageService _tokenStorage;

        public ApiAuthenticationStateProvider(HttpClient httpClient, TokenStorageService tokenStorage)
        {
            _httpClient = httpClient;
            _tokenStorage = tokenStorage;
        }

        // C'est la méthode principale que Blazor appelle
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _tokenStorage.GetTokenAsync();

            if (string.IsNullOrWhiteSpace(token))
            {
                // Personne n'est connecté
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            // Mettre le token dans les en-têtes de TOUTES les futures requêtes HttpClient
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Créer l'identité de l'utilisateur à partir du token
            return new AuthenticationState(new ClaimsPrincipal(
                new ClaimsIdentity(ParseClaimsFromJwt(token), "jwtAuthType")));
        }

        // Marque l'utilisateur comme connecté
        public void MarkUserAsAuthenticated(string token)
        {
            var authenticatedUser = new ClaimsPrincipal(
                new ClaimsIdentity(ParseClaimsFromJwt(token), "jwtAuthType"));

            var authState = Task.FromResult(new AuthenticationState(authenticatedUser));

            // Informe le reste de l'application que l'état a changé
            NotifyAuthenticationStateChanged(authState);
        }

        // Marque l'utilisateur comme déconnecté
        public void MarkUserAsLoggedOut()
        {
            var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
            var authState = Task.FromResult(new AuthenticationState(anonymousUser));

            // Vide l'en-tête HttpClient
            _httpClient.DefaultRequestHeaders.Authorization = null;

            NotifyAuthenticationStateChanged(authState);
        }

        // --- Méthode utilitaire pour lire le token ---
        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();
            var payload = jwt.Split('.')[1];

            // Corrige le padding Base64
            var jsonBytes = ParseBase64WithoutPadding(payload);
            if (jsonBytes == null)
                return claims;

            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
            if (keyValuePairs == null)
                return claims;

            // Ajoute les claims "connus" de .NET
            if (keyValuePairs.TryGetValue(ClaimTypes.NameIdentifier, out var id))
                claims.Add(new Claim(ClaimTypes.NameIdentifier, id.ToString() ?? ""));

            if (keyValuePairs.TryGetValue(ClaimTypes.Name, out var name))
                claims.Add(new Claim(ClaimTypes.Name, name.ToString() ?? ""));

            if (keyValuePairs.TryGetValue(ClaimTypes.Role, out var role))
                claims.Add(new Claim(ClaimTypes.Role, role.ToString() ?? ""));

            // Ajoute notre claim personnalisé
            if (keyValuePairs.TryGetValue("EstablishmentId", out var establishmentId))
                claims.Add(new Claim("EstablishmentId", establishmentId.ToString() ?? ""));

            return claims;
        }

        private byte[]? ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            try
            {
                return Convert.FromBase64String(base64);
            }
            catch (FormatException)
            {
                return null; // Token malformé
            }
        }
    }
}