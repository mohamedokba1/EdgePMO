using System.ComponentModel.DataAnnotations;

namespace EdgePMO.API.Dtos.Courses
{
    public class ProgressUpdateDto
    {
        [Required(ErrorMessage = "Course CertificateId can not be null or empty")]
        public Guid CourseId { get; set; }

        [Required(ErrorMessage = "Progress value can not be null or empty")]
        public double Progress { get; set; }
    }
}
