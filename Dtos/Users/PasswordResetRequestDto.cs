using System.ComponentModel.DataAnnotations;

namespace EdgePMO.API.Dtos
{
    public record PasswordResetRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
