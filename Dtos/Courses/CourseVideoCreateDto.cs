using System.ComponentModel.DataAnnotations;

namespace EdgePMO.API.Dtos
{
    public record CourseVideoCreateDto
    {
        [Required]
        public Guid CourseId { get; init; }

        [Required]
        [StringLength(300)]
        public string Title { get; init; } = null!;

        [StringLength(2000)]
        public string? Description { get; init; }

        [Required]
        public string VideoUrl { get; init; } = null!;

        public int DurationSeconds { get; init; }
        [Required]
        [Range(1, 100)]
        public int Order { get; init; }
    }
}
