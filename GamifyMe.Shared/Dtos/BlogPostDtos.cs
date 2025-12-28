namespace GamifyMe.Shared.Dtos
{
    public class BlogPostDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string? CoverImageUrl { get; set; }
        public string Author { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }
        public bool IsPublished { get; set; }
    }

    public class CreateBlogPostDto
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string? CoverImageUrl { get; set; }
        public string Author { get; set; } = "Meritopass";
        public bool IsPublished { get; set; } = false;
    }

    public class UpdateBlogPostDto
    {
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty; // Allow updating slug manually if needed
        public string Content { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string? CoverImageUrl { get; set; }
        public bool IsPublished { get; set; }
    }
}
