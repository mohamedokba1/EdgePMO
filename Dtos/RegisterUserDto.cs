using System.ComponentModel.DataAnnotations;

namespace EdgePMO.API.Dtos
{
    public record RegisterUserDto
    {
        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 50 characters.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 50 characters.")]
        public string LastName { get; set; }

        [StringLength(15, ErrorMessage = "Phone cannot exceed 15 characters.")]
        [RegularExpression(@"^\+?[0-9]+$", ErrorMessage = "Phone number can only contain numeric digits and an optional '+' at the start.")]
        public string Phone1 { get; set; }

        [StringLength(15, ErrorMessage = "Phone cannot exceed 15 characters.")]
        [RegularExpression(@"^\+?[0-9]+$", ErrorMessage = "Phone number can only contain numeric digits and an optional '+' at the start.")]
        public string Phone2 { get; set; }

        [StringLength(255, ErrorMessage = "Last company cannot exceed 255 characters.")]
        public string LastCompany { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [StringLength(150, ErrorMessage = "Email cannot exceed 150 characters.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]+$",
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character (@$!%*?&).")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Password confirmation is required.")]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string PasswordConfirmation { get; set; }

    }
}
