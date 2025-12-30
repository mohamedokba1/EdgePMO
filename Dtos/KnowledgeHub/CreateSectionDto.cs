namespace EdgePMO.API.Dtos
{
    public class CreateSectionDto
    {
        public string Heading { get; set; } = null!;
        public int Order { get; set; }
        public List<CreateContentBlockDto> Blocks { get; set; } = new();
    }
}
