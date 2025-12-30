using System.ComponentModel.DataAnnotations;

namespace EdgePMO.API.Dtos
{
    public record PurchaseRequestRejectionActionDto
    {

        [StringLength(4000)]
        [Required(ErrorMessage = "Rejection reasons list can't be null or empty")]
        public List<string>? RejectionReasons { get; init; }
    }
}
