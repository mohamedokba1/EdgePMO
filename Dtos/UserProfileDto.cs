using System.ComponentModel.DataAnnotations;

namespace EdgePMO.API.Dtos
{
    public class UserProfileDto
    {
        [Required]
        public Guid Id { get; init; }

        [EmailAddress]
        public string Email { get; set; }
    }
}
