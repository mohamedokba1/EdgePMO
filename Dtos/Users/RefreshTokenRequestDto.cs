using System.ComponentModel.DataAnnotations;

namespace EdgePMO.API.Dtos
{
    public record RefreshTokenRequestDto
    {
        [Required]
        public string RefreshToken { get; init; }
    }
}
