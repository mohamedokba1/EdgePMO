namespace EdgePMO.API.Dtos
{
    public record SectionDto
    {
        public Guid Id { get; set; }
        public string Heading { get; set; } = null!;
        public int Order { get; set; }
        public List<ContentBlockDto> Blocks { get; set; } = new();
    }
}
