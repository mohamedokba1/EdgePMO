using System.ComponentModel.DataAnnotations;

namespace EdgePMO.API.Dtos
{
    public record CreateKnowledgeHubDto
    {
        [Required]
        public string Title { get; set; }
        [Required]
        public string Excerpt { get; set; }
        [Required]
        public string Author { get; set; }
        [Required]
        public DateTime PublishDate { get; set; }
        [Required]
        public string CoverImageUrl { get; set; }
        [Required]
        public string DocumentUrl { get; set; }
        [Required]
        public List<CreateSectionDto> Sections { get; set; } = new();
    }
}
