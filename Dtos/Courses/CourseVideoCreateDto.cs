using System.ComponentModel.DataAnnotations;

namespace EdgePMO.API.Dtos
{
    public record CourseVideoCreateDto
    {
        //[Required]
        public Guid CourseId { get; init; }

        [Required(ErrorMessage = "Outline Id can not be null or empty")]
        public Guid OutlineId { get; init; }

        [Required(ErrorMessage = "Video title can not be null or empty")]
        [StringLength(500)]
        public string Title { get; init; } = null!;

        [StringLength(2000)]
        public string? Description { get; init; }

        [Required(ErrorMessage = "Video url can not be null or empty")]
        public string Url { get; init; } = null!;

        public int DurationSeconds { get; init; }

        [Required]
        [Range(1, 100, ErrorMessage ="Video order can not be null or empty")]
        public int Order { get; init; }
    }
}
