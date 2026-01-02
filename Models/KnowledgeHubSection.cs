namespace EdgePMO.API.Models
{
    public class KnowledgeHubSection
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid KnowledgeHubId { get; set; }
        public KnowledgeHub KnowledgeHub { get; set; } = null!;
        public string Heading { get; set; } = null!;
        public int Order { get; set; }
        public List<ContentBlock> Blocks { get; set; } = new();
    }
}
