namespace EdgePMO.API.Models
{
    public class Purchase
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid? TemplateId { get; set; }
        public Guid? CourseId { get; set; }
        public string PurchaseType { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public string PaymentMethod { get; set; }
        public string TransactionId { get; set; }
        public string Status { get; set; } = "pending";
        public DateTime PurchasedAt { get; set; } = DateTime.UtcNow;
        public DateTime? RefundedAt { get; set; }
        public string Notes { get; set; }

        public User User { get; set; }
        public Template Template { get; set; }
        public Course Course { get; set; }
    }
}
