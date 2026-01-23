using System.ComponentModel.DataAnnotations;

namespace EdgePMO.API.Dtos.Courses
{
    public class CourseContentUpdateDto
    {
        public List<CourseDocumentUpdateDto>? Documents { get; set; }

        [Required]
        public Guid Id { get; set; }

        [Range(1, 1000)]
        public int? Order { get; set; } = 1;

        [StringLength(500)]
        public string? Title { get; set; }

        public List<CourseVideoUpdateDto>? Videos { get; set; }
    }
}