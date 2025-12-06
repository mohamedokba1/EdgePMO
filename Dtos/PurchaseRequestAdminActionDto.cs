using System.ComponentModel.DataAnnotations;

namespace EdgePMO.API.Dtos
{
    public record PurchaseRequestAdminActionDto
    {
        [Required]
        public string Status { get; init; } = null!;

        [StringLength(2000)]
        public string? Notes { get; init; }
    }
}
