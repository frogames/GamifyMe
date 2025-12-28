using GamifyMe.Api.Data;
using GamifyMe.Shared.Dtos;
using GamifyMe.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace GamifyMe.Api.Services
{
    public class BlogService : IBlogService
    {
        private readonly DataContext _context;

        public BlogService(DataContext context)
        {
            _context = context;
        }

        public async Task<List<BlogPostDto>> GetPublishedPostsAsync()
        {
            return await _context.BlogPosts
                .Where(p => p.IsPublished)
                .OrderByDescending(p => p.PublishedAt)
                .Select(p => new BlogPostDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Slug = p.Slug,
                    Summary = p.Summary,
                    CoverImageUrl = p.CoverImageUrl,
                    Author = p.Author,
                    CreatedAt = p.CreatedAt,
                    PublishedAt = p.PublishedAt,
                    IsPublished = p.IsPublished,
                    Content = p.Content 
                })
                .ToListAsync();
        }

        // Note: For lists, we might not want the full content, but for now it's fine. 
        // Optimized: Maybe remove Content from list later if payload is big.

        public async Task<BlogPostDto?> GetPostBySlugAsync(string slug)
        {
            var p = await _context.BlogPosts
                .FirstOrDefaultAsync(p => p.Slug == slug && p.IsPublished);

            if (p == null) return null;

            return new BlogPostDto
            {
                Id = p.Id,
                Title = p.Title,
                Slug = p.Slug,
                Summary = p.Summary,
                CoverImageUrl = p.CoverImageUrl,
                Author = p.Author,
                CreatedAt = p.CreatedAt,
                PublishedAt = p.PublishedAt,
                IsPublished = p.IsPublished,
                Content = p.Content
            };
        }

        public async Task<List<BlogPost>> GetAllPostsAdminAsync()
        {
            return await _context.BlogPosts
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<BlogPost> CreatePostAsync(CreateBlogPostDto postDto)
        {
            var slug = GenerateSlug(postDto.Title);
            
            // Ensure slug uniqueness
            int count = 1;
            var originalSlug = slug;
            while (await _context.BlogPosts.AnyAsync(p => p.Slug == slug))
            {
                slug = $"{originalSlug}-{count}";
                count++;
            }

            var post = new BlogPost
            {
                Title = postDto.Title,
                Slug = slug,
                Content = postDto.Content,
                Summary = postDto.Summary,
                CoverImageUrl = postDto.CoverImageUrl,
                Author = postDto.Author,
                IsPublished = postDto.IsPublished,
                CreatedAt = DateTime.UtcNow,
                PublishedAt = postDto.IsPublished ? DateTime.UtcNow : null
            };

            _context.BlogPosts.Add(post);
            await _context.SaveChangesAsync();

            return post;
        }

        public async Task<BlogPost?> UpdatePostAsync(Guid id, UpdateBlogPostDto postDto)
        {
            var post = await _context.BlogPosts.FindAsync(id);
            if (post == null) return null;

            post.Title = postDto.Title;
            post.Content = postDto.Content;
            post.Summary = postDto.Summary;
            post.CoverImageUrl = postDto.CoverImageUrl;
            
            // Handle publishing date logic
            if (postDto.IsPublished && !post.IsPublished)
            {
                post.PublishedAt = DateTime.UtcNow;
            }
            else if (!postDto.IsPublished)
            {
                post.PublishedAt = null;
            }
            post.IsPublished = postDto.IsPublished;
            
            post.UpdatedAt = DateTime.UtcNow;

            // Optional: allow slug update? For SEO consistency usually you don't change slug, but if requested...
            if (!string.IsNullOrEmpty(postDto.Slug) && postDto.Slug != post.Slug)
            {
                 // Verify uniqueness if changed
                 if (!await _context.BlogPosts.AnyAsync(p => p.Slug == postDto.Slug && p.Id != id))
                 {
                     post.Slug = postDto.Slug;
                 }
            }

            await _context.SaveChangesAsync();
            return post;
        }

        public async Task<bool> DeletePostAsync(Guid id)
        {
            var post = await _context.BlogPosts.FindAsync(id);
            if (post == null) return false;

            _context.BlogPosts.Remove(post);
            await _context.SaveChangesAsync();
            return true;
        }

        private string GenerateSlug(string title)
        {
            var slug = title.ToLower().Trim();
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\s-]", ""); // Remove invalid chars
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-"); // Convert spaces to hyphens
            slug = slug.Trim('-');
            return slug;
        }
    }
}
