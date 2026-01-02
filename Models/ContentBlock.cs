namespace EdgePMO.API.Models
{
    public class ContentBlock
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid SectionId { get; set; }
        public KnowledgeHubSection Section { get; set; } = null!;
        public string Type { get; set; } = null!;
        public string Content { get; set; } = null!;
        public int Order { get; set; }
    }
}
