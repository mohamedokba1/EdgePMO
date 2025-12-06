using System.ComponentModel.DataAnnotations;

namespace EdgePMO.API.Dtos
{
    public record PurchaseRequestCreateDto
    {
        // UserId is taken from authenticated user (controller), not from payload
        public Guid? TemplateId { get; init; }
        public Guid? CourseId { get; init; }

        [Range(0, double.MaxValue)]
        public decimal? RequestedAmount { get; init; }

        [StringLength(10)]
        public string? Currency { get; init; } = "EGP";

        public string? IdempotencyKey { get; init; }

        [StringLength(2000)]
        public string? Notes { get; init; }
    }
}
