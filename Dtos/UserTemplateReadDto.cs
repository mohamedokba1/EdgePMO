namespace EdgePMO.API.Dtos
{
    public class UserTemplateReadDto
    {
        public Guid TemplateId { get; init; }
        public string Name { get; init; } = null!;
        public string? ImageUrl { get; init; }
        public decimal Price { get; init; }
        public string? Category { get; init; }
        public bool IsActive { get; init; }
        public DateTime PurchasedAt { get; init; }
        public DateTime? DownloadedAt { get; init; }
        public bool IsFavorite { get; init; }
    }
}
