using System.ComponentModel.DataAnnotations;

namespace EdgePMO.API.Dtos
{
    public class PasswordResetDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string VerificationCode { get; set; }

        [Required]
        public string NewPassword { get; set; }

        [Required]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string NewPasswordConfirmation { get; set; }
    }
}
