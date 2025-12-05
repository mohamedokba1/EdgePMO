using System.ComponentModel.DataAnnotations;

namespace EdgePMO.API.Dtos
{
    public record TestimonialCreateDto
    {
        [Required]
        public Guid CourseId { get; init; }

        [Required]
        [StringLength(200)]
        public string StudentName { get; init; } = null!;

        [Required]
        [StringLength(5000)]
        public string Comment { get; init; } = null!;

        [Range(1, 5)]
        public int Rating { get; init; } = 5;
    }
}