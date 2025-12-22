namespace EdgePMO.API.Dtos
{
    public record TemplateUsersReadDto
    {
        public Guid UserId { get; init; }
        public DateTime PurchasedAt { get; init; }
        public DateTime? DownloadedAt { get; init; }
        public bool IsFavorite { get; init; }
    }
}
