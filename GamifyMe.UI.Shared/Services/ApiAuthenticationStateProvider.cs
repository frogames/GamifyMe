using GamifyMe.Shared.Dtos; // <-- AJOUTE ÇA
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json; // <-- AJOUTE ÇA
using System.Security.Claims;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using System;

namespace GamifyMe.UI.Shared.Services
{
    public class ApiAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly HttpClient _httpClient;
        private readonly TokenStorageService _tokenStorage;

        // 1. NOUVELLE PROPRIÉTÉ pour stocker les infos
        public InfoBarDto? CurrentUserInfo { get; private set; }

        public ApiAuthenticationStateProvider(HttpClient httpClient, TokenStorageService tokenStorage)
        {
            _httpClient = httpClient;
            _tokenStorage = tokenStorage;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _tokenStorage.GetTokenAsync();

            if (string.IsNullOrWhiteSpace(token))
            {
                CurrentUserInfo = null;
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            // 1. ON MET LE TOKEN D'ABORD
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // 2. ON APPELLE L'API ENSUITE
            await FetchUserInfoAsync();

            return new AuthenticationState(new ClaimsPrincipal(
                new Identity(ParseClaimsFromJwt(token), "jwtAuthType")));
        }

        public async Task MarkUserAsAuthenticated(string token)
        {
            // 👇 LA CORRECTION EST D'AJOUTER CETTE LIGNE 👇
            // On met le token dans les en-têtes AVANT d'appeler l'API
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var authenticatedUser = new ClaimsPrincipal(
                new ClaimsIdentity(ParseClaimsFromJwt(token), "jwtAuthType"));

            var authState = Task.FromResult(new AuthenticationState(authenticatedUser));

            // Cet appel va maintenant réussir
            await FetchUserInfoAsync();

            NotifyAuthenticationStateChanged(authState);
        }

        public async Task Logout()
        {
            CurrentUserInfo = null; // Vider les infos
            await _tokenStorage.RemoveTokenAsync();
            MarkUserAsLoggedOut();
        }

        public void MarkUserAsLoggedOut()
        {
            var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
            var authState = Task.FromResult(new AuthenticationState(anonymousUser));

            _httpClient.DefaultRequestHeaders.Authorization = null;

            NotifyAuthenticationStateChanged(authState);
        }

        // 4. NOUVELLE MÉTHODE pour charger les infos
        private async Task FetchUserInfoAsync()
        {
            try
            {
                CurrentUserInfo = await _httpClient.GetFromJsonAsync<InfoBarDto>("api/Users/info-bar");
            }
            catch (Exception ex)
            {
                // On ne déconnecte PAS l'utilisateur, on log juste l'erreur
                // et on laisse la barre d'info vide.
                Console.WriteLine($"ERREUR (FetchUserInfoAsync): {ex.Message}");
                CurrentUserInfo = null; // La barre d'info sera vide, mais tu restes connecté.
            }
        }

        // ... (après FetchUserInfoAsync)

        // ==================================================================
        // AJOUTE LES MÉTHODES UTILITAIRES MANQUANTES
        // ==================================================================
        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();
            var payload = jwt.Split('.')[1];

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




    // ... (tes méthodes ParseClaimsFromJwt et ParseBase64WithoutPadding restent inchangées) ...

    // 5. CLASSE INTERNE CORRIGÉE (j'avais fait une erreur avant)
    // On doit définir une classe qui hérite de ClaimsIdentity pour que .User.Identity?.Name fonctionne
    private class Identity : ClaimsIdentity
        {
            public Identity(IEnumerable<Claim> claims, string authType) : base(claims, authType)
            {
            }
        }

    }
}