using System.ComponentModel.DataAnnotations;

namespace GamifyMe.Shared.Models
{
    public class BlogPost
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(250)]
        public string Slug { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Summary { get; set; } = string.Empty;

        public string? CoverImageUrl { get; set; }

        public string Author { get; set; } = "Meritopass";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public DateTime? PublishedAt { get; set; }

        public bool IsPublished { get; set; } = false;
    }
}
