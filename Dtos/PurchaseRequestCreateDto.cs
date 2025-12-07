using System.ComponentModel.DataAnnotations;

namespace EdgePMO.API.Dtos
{
    public record PurchaseRequestCreateDto
    {
        public Guid? TemplateId { get; init; }
        public Guid? CourseId { get; init; }

        public string? IdempotencyKey { get; init; }

        [StringLength(2000)]
        public string? Notes { get; init; } = "N/A";
    }
}
