using System.ComponentModel.DataAnnotations;

namespace EdgePMO.API.Dtos.Courses
{
    public class CourseContentReadDto
    {
        [Required(ErrorMessage = "The title of outline section can not be null or empty")]
        [StringLength(500)]
        public string Title { get; set; }

        [Required(ErrorMessage = "The order of section can not be null or empty")]
        [Range(1, 1000)]
        public int Order { get; set; } = 1;

        public List<CourseVideoReadDto> Videos { get; set; }
        public List<CourseDocumentReadDto> Documents { get; set; }
    }
}
