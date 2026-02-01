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

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]+$",
         ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character (@$!%*?&).")]
        public string NewPassword { get; set; }

        [Required]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string NewPasswordConfirmation { get; set; }
    }
}
