using System.ComponentModel.DataAnnotations;

namespace EdgePMO.API.Dtos.Courses
{
    public class CourseDocumentCreateDto
    {
        [StringLength(2000)]
        public string? Description { get; init; }

        [Required]
        [StringLength(500)]
        public string Title { get; init; } = null!;

        [Required]
        public Guid Url { get; init; }
    }
}