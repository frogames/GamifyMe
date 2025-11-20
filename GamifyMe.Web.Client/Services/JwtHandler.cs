using System.Net.Http.Headers;
using GamifyMe.UI.Shared.Services;

namespace GamifyMe.Web.Client.Services
{
    public class JwtHandler : DelegatingHandler
    {
        private readonly TokenStorageService _tokenStorage;

        public JwtHandler(TokenStorageService tokenStorage)
        {
            _tokenStorage = tokenStorage;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // 1. On récupère le token stocké
            // Note: Vérifiez si votre méthode s'appelle GetToken() ou GetTokenAsync() dans TokenStorageService
            // Je mets ici un code générique, adaptez si besoin.
            var token = await _tokenStorage.GetTokenAsync();

            // 2. S'il existe, on l'ajoute dans le header "Authorization: Bearer ..."
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            // 3. On laisse passer la requête
            return await base.SendAsync(request, cancellationToken);
        }
    }
}