using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace GamifyMe.Web
{
    // Ce service "factice" est UNIQUEMENT pour le rendu côté serveur (SSR).
    // Son seul but est d'exister pour que le serveur ne plante pas au démarrage.
    // Il dit simplement "l'utilisateur n'est pas connecté".
    public class ServerSideAuthStateProvider : AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
            return Task.FromResult(new AuthenticationState(anonymousUser));
        }
    }
}