using System.ComponentModel.DataAnnotations;

namespace EdgePMO.API.Dtos
{
    public record LoginDto
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
