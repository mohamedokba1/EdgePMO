using System.ComponentModel.DataAnnotations;

namespace EdgePMO.API.Dtos.Courses
{
    public class CourseDocumentCreateDto
    {
        [Required]
        [StringLength(500)]
        public string Title { get; init; } = null!;

        [StringLength(2000)]
        public string? Description { get; init; }

        [Required]
        public string Url { get; init; } = null!;
    }
}
