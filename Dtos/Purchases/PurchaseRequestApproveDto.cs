using System.ComponentModel.DataAnnotations;

namespace EdgePMO.API.Dtos
{
    /// <summary>
    /// Optional body for approving a WhatsApp-negotiated purchase request. The admin
    /// has already received payment outside the system by the time they click
    /// Approve — this captures what was actually charged so the resulting Purchase
    /// row carries a real Amount instead of the field's zero default.
    /// </summary>
    public class PurchaseRequestApproveDto
    {
        // Optional — omit to use the course/template's current listed price.
        // Present when the admin negotiated a different amount over WhatsApp.
        [Range(0, double.MaxValue, ErrorMessage = "Amount can not be negative.")]
        public decimal? Amount { get; set; }

        [StringLength(10)]
        public string? Currency { get; set; }
    }
}
