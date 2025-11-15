using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace GamifyMe.Web.Client.Services
{
    // Ce service gère l'accès direct au LocalStorage du navigateur
    public class TokenStorageService
    {
        private readonly IJSRuntime _jsRuntime;
        private const string TOKEN_KEY = "authToken"; // Clé de stockage

        public TokenStorageService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task SetTokenAsync(string token)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", TOKEN_KEY, token);
        }

        public async Task<string?> GetTokenAsync()
        {
            return await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", TOKEN_KEY);
        }

        public async Task RemoveTokenAsync()
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", TOKEN_KEY);
        }
    }
}