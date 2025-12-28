using GamifyMe.Shared.Dtos;
using GamifyMe.Shared.Models;

namespace GamifyMe.Api.Services
{
    public interface IBlogService
    {
        Task<List<BlogPostDto>> GetPublishedPostsAsync();
        Task<BlogPostDto?> GetPostBySlugAsync(string slug);
        Task<List<BlogPost>> GetAllPostsAdminAsync();
        Task<BlogPost> CreatePostAsync(CreateBlogPostDto postDto);
        Task<BlogPost?> UpdatePostAsync(Guid id, UpdateBlogPostDto postDto);
        Task<bool> DeletePostAsync(Guid id);
    }
}
