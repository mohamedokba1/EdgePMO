using System.ComponentModel.DataAnnotations;

namespace EdgePMO.API.Dtos
{
    public record UpdateKnowledgeHubDto
    {
        [Required(ErrorMessage = "Article Id can not be null or empty")]
        public Guid Id { get; set; }
        public string? Title { get; set; } = null!;
        public string? Excerpt { get; set; } = null!;
        public string? Author { get; set; } = null!;
        public DateTime? PublishDate { get; set; }
        public string? CoverImageUrl { get; set; } = null!;
        public string? DocumentUrl { get; set; }
        public bool? IsActive { get; set; }
        public List<CreateSectionDto>? Sections { get; set; } = new();
    }
}
