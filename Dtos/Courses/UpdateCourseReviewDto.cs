using System.ComponentModel.DataAnnotations;

namespace EdgePMO.API.Dtos.Courses
{
    public class UpdateCourseReviewDto
    {
        [Required]
        public Guid Id { get; set; }
        public string? Header { get; set; }
        public double? Rating { get; set; }
        public string? Content { get; set; }
    }
}
