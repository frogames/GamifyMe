using GamifyMe.Api.Services;
using GamifyMe.Shared.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GamifyMe.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BlogController : ControllerBase
    {
        private readonly IBlogService _blogService;

        public BlogController(IBlogService blogService)
        {
            _blogService = blogService;
        }

        [HttpGet]
        public async Task<ActionResult<List<BlogPostDto>>> GetPublishedPosts()
        {
            var posts = await _blogService.GetPublishedPostsAsync();
            return Ok(posts);
        }

        [HttpGet("{slug}")]
        public async Task<ActionResult<BlogPostDto>> GetPostBySlug(string slug)
        {
            var post = await _blogService.GetPostBySlugAsync(slug);
            if (post == null) return NotFound();
            return Ok(post);
        }

        [HttpGet("admin")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<List<BlogPostDto>>> GetAllPostsAdmin()
        {
            var posts = await _blogService.GetAllPostsAdminAsync();
            return Ok(posts);
        }

        [HttpPost("admin")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<BlogPostDto>> CreatePost(CreateBlogPostDto dto)
        {
            var created = await _blogService.CreatePostAsync(dto);
            return CreatedAtAction(nameof(GetPostBySlug), new { slug = created.Slug }, created);
        }

        [HttpPut("admin/{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<BlogPostDto>> UpdatePost(Guid id, UpdateBlogPostDto dto)
        {
            var updated = await _blogService.UpdatePostAsync(id, dto);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpDelete("admin/{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> DeletePost(Guid id)
        {
            var success = await _blogService.DeletePostAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
