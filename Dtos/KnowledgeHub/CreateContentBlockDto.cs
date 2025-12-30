namespace EdgePMO.API.Dtos
{
    public record CreateContentBlockDto
    {
        public string Type { get; set; } = null!;
        public string Content { get; set; } = null!;
        public int Order { get; set; }
    }
}
