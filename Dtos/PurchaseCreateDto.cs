using System.ComponentModel.DataAnnotations;

namespace EdgePMO.API.Dtos
{
    public record PurchaseCreateDto
    {
        [Required]
        public Guid UserId { get; init; }

        public Guid? TemplateId { get; init; }
        public Guid? CourseId { get; init; }

        [Required]
        [StringLength(50)]
        public string PurchaseType { get; init; } = null!;

        [Range(0, double.MaxValue)]
        public decimal Amount { get; init; }

        [StringLength(10)]
        public string Currency { get; init; } = "USD";

        [StringLength(100)]
        public string? PaymentMethod { get; init; }

        [StringLength(200)]
        public string? TransactionId { get; init; }

        [StringLength(50)]
        public string Status { get; init; } = "completed";

        [StringLength(2000)]
        public string? Notes { get; init; }
    }
}
