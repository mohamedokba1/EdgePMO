using System.ComponentModel.DataAnnotations;

namespace EdgePMO.API.Dtos
{
    public record VerifyRequestDto
    {
        [Required]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; }
    }
}
