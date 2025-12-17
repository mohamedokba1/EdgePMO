using System.ComponentModel.DataAnnotations;

namespace EdgePMO.API.Dtos
{
    public record CourseVideoCreateDto
    {
        //[Required]
        public Guid CourseId { get; init; }

        [Required(ErrorMessage = "Outline Id can not be null or empty")]
        public Guid OutlineId { get; init; }

        [Required]
        [StringLength(500)]
        public string Title { get; init; } = null!;

        [StringLength(2000)]
        public string? Description { get; init; }

        [Required]
        public string Url { get; init; } = null!;

        public int DurationSeconds { get; init; }

        [Required]
        [Range(1, 100)]
        public int Order { get; init; }
    }
}
