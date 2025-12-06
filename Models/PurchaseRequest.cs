namespace EdgePMO.API.Models
{
    public class PurchaseRequest
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public Guid? TemplateId { get; set; }
        public Guid? CourseId { get; set; }
        public decimal? RequestedAmount { get; set; }
        public string Currency { get; set; } = "EGP";
        public string Notes { get; set; }
        public string Status { get; set; } = "Pending";
        public Guid? AdminId { get; set; }
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DecisionAt { get; set; }
        public string? IdempotencyKey { get; set; }

        public User User { get; set; }
        public Template Template { get; set; }
        public Course Course { get; set; }
    }
}
