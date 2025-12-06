namespace EdgePMO.API.Models
{
    public class UserTemplate
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid TemplateId { get; set; }
        public Guid? PurchaseId { get; set; }
        public DateTime PurchasedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DownloadedAt { get; set; }
        public bool IsFavorite { get; set; } = false;

        public User User { get; set; }
        public Template Template { get; set; }
        public Purchase Purchase { get; set; }
    }
}
