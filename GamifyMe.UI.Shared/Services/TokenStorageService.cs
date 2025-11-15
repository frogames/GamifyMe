using Microsoft.JSInterop;
using System.Threading.Tasks;

// Assure-toi que le namespace est bien .UI.Shared
namespace GamifyMe.UI.Shared.Services
{
    public class TokenStorageService
    {
        private readonly IJSRuntime _jsRuntime;
        private const string TOKEN_KEY = "authToken";

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