using System.ComponentModel.DataAnnotations;

namespace EdgePMO.API.Dtos
{
    public class CreateCourseReviewDto
    {
        [Required]
        public Guid CourseId { get; set; }
        [Required]
        public Guid UserId { get; set; }
        public string? Header { get; set; }
        public double? Rating { get; set; }
        public string? Content { get; set; }
    }
}
