namespace EdgePMO.API.Dtos
{
    public record KnowledgeHubDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Excerpt { get; set; }
        public string Author { get; set; }
        public DateTime PublishDate { get; set; }
        public string CoverImageUrl { get; set; }
        public string? DocumentUrl { get; set; }
        public List<SectionDto> Sections { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}
