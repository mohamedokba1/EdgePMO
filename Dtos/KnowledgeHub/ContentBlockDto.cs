namespace EdgePMO.API.Dtos
{
    public class ContentBlockDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = null!;
        public string Content { get; set; } = null!;
        public int Order { get; set; }
    }
}
