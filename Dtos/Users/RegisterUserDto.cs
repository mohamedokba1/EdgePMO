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
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Phone number can only contain numeric digits (country code is separate).")]
        public string Phone1 { get; set; }

        [StringLength(15, ErrorMessage = "Phone cannot exceed 15 characters.")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Phone number can only contain numeric digits (country code is separate).")]
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


        //[RegularExpression(@"^\+[1-9]\d{0,2}$", ErrorMessage = "Country code must start with '+' followed by 1 to 3 digits (e.g. +20,  +1, +44, +966).")]
        //public string CountryCode { get; set; }


        //public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        //{
        //    bool hasPhone = !string.IsNullOrWhiteSpace(Phone1) || !string.IsNullOrWhiteSpace(Phone2);
        //    bool hasCountryCode = !string.IsNullOrWhiteSpace(CountryCode);

        //    if (hasPhone && !hasCountryCode)
        //        yield return new ValidationResult(
        //            "Country code is required when a phone number is provided.",
        //            new[] { nameof(CountryCode) });

        //    if (!hasPhone && hasCountryCode)
        //        yield return new ValidationResult(
        //            "A phone number is required when a country code is provided.",
        //            new[] { nameof(Phone1) });

        //    if (hasPhone && hasCountryCode)
        //    {
        //        // Enforce combined E.164 length: country code + number must not exceed 15 digits total
        //        var digits = CountryCode.TrimStart('+');

        //        if (!string.IsNullOrWhiteSpace(Phone1) && digits.Length + Phone1.Length > 15)
        //            yield return new ValidationResult(
        //                $"The combination of country code and Phone1 must not exceed 15 digits in total (E.164 standard).",
        //                new[] { nameof(Phone1) });

        //        if (!string.IsNullOrWhiteSpace(Phone2) && digits.Length + Phone2.Length > 15)
        //            yield return new ValidationResult(
        //                $"The combination of country code and Phone2 must not exceed 15 digits in total (E.164 standard).",
        //                new[] { nameof(Phone2) });
        //    }
        //}
    }
}