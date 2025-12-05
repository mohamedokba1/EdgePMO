using System.ComponentModel.DataAnnotations;

namespace EdgePMO.API.Dtos
{
    public record InstructorUpdateDto
    {
        [Required]
        public Guid InstructorId { get; init; }

        [Required]
        [StringLength(200)]
        public string InstructorName { get; init; } = null!;

        [StringLength(2000)]
        public string? Profile { get; init; }
    }
}