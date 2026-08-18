using System.ComponentModel.DataAnnotations;

namespace EdgePMO.API.Dtos
{
    public class ListOfUsersEmails
    {
        [Required]
        public List<string> Emails { get; init; }

        // Only read by the enroll/grant-access actions — ignored by
        // unenroll/revoke/status-check. There's no online payment this phase, so
        // this is how a real sale gets recorded: the admin has already been paid
        // over WhatsApp by the time they grant access, and this captures what was
        // actually charged (defaults to the course/template's listed price when
        // omitted).
        [Range(0, double.MaxValue, ErrorMessage = "Amount can not be negative.")]
        public decimal? Amount { get; init; }

        [StringLength(10)]
        public string? Currency { get; init; }
    }
}
