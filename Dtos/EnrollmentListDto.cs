using System.ComponentModel.DataAnnotations;

namespace EdgePMO.API.Dtos
{
    public class EnrollmentListDto
    {
        [Required]
        public List<string> Emails { get; init; }
    }
}
