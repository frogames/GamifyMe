using GamifyMe.Shared.Dtos;
using System.Net.Http.Json;

namespace GamifyMe.UI.Shared.Services
{
    public class BlogService
    {
        private readonly HttpClient _http;

        public BlogService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<BlogPostDto>> GetPublishedPostsAsync()
        {
            return await _http.GetFromJsonAsync<List<BlogPostDto>>("api/blog") ?? new List<BlogPostDto>();
        }

        public async Task<BlogPostDto?> GetPostBySlugAsync(string slug)
        {
            try
            {
                return await _http.GetFromJsonAsync<BlogPostDto>($"api/blog/{slug}");
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<List<BlogPostDto>> GetAllPostsAdminAsync()
        {
            return await _http.GetFromJsonAsync<List<BlogPostDto>>("api/blog/admin") ?? new List<BlogPostDto>();
        }

        public async Task<BlogPostDto> CreatePostAsync(CreateBlogPostDto post)
        {
            var response = await _http.PostAsJsonAsync("api/blog/admin", post);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<BlogPostDto>() ?? throw new InvalidOperationException("Empty response");
        }

        public async Task<BlogPostDto> UpdatePostAsync(Guid id, UpdateBlogPostDto post)
        {
            var response = await _http.PutAsJsonAsync($"api/blog/admin/{id}", post);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<BlogPostDto>() ?? throw new InvalidOperationException("Empty response");
        }

        public async Task DeletePostAsync(Guid id)
        {
            var response = await _http.DeleteAsync($"api/blog/admin/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}
