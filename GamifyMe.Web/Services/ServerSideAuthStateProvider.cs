using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace GamifyMe.Web.Services
{
    // Ce provider est utilisé pour le pré-rendu côté serveur (SSR).
    // Il indique toujours à l'application que l'utilisateur n'est PAS authentifié.
    // L'authentification réelle sera gérée par le client (WASM) après le chargement.
    public class ServerSideAuthStateProvider : AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var anonymousIdentity = new ClaimsIdentity();
            var anonymousUser = new ClaimsPrincipal(anonymousIdentity);
            return Task.FromResult(new AuthenticationState(anonymousUser));
        }
    }
}