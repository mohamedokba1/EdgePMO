using System.ComponentModel.DataAnnotations;

namespace EdgePMO.API.Dtos.Courses
{
    public class SetReviewHiddenDto
    {
        [Required]
        public bool IsHidden { get; set; }
    }
}
