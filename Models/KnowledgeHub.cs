namespace EdgePMO.API.Models
{
    public class KnowledgeHub
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = null!;
        public string Excerpt { get; set; } = null!;
        public string Author { get; set; } = null!;
        public DateTime PublishDate { get; set; }
        public string CoverImageUrl { get; set; } = null!;
        public string? DocumentUrl { get; set; }
        public List<KnowledgeHubSection> Sections { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
    }
}
