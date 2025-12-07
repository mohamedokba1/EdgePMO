namespace EdgePMO.API.Dtos
{
    public class TemplateReadDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = null!;
        public string? Description { get; init; }
        public decimal Price { get; init; }
        public string? Category { get; init; }
        public string? CoverImageUrl { get; init; }
        public bool IsActive { get; init; }
        public string? FilePath { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }

        public List<TemplateUsersReadDto> UsersPurchased { get; init; } = new();
    }
}
