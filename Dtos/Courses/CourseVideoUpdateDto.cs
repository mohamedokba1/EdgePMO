using System.ComponentModel.DataAnnotations;

namespace EdgePMO.API.Dtos
{
    public record CourseVideoUpdateDto
    {
        [Required(ErrorMessage = "Outline Id can not be null or empty")]
        public Guid CourseVideoId { get; init; }

        [StringLength(500)]
        public string? Title { get; init; }

        [StringLength(2000)]
        public string? Description { get; init; }

        public string? Url { get; init; } = null!;

        public int? DurationMinutes { get; init; }

        [Range(1, 100, ErrorMessage ="Video order can not be null or empty")]
        public int? Order { get; init; }
    }
}
