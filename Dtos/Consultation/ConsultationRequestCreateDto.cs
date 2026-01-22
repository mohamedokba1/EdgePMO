using System.ComponentModel.DataAnnotations;

namespace EdgePMO.API.Dtos
{
    public class ConsultationRequestCreateDto
    {
        [StringLength(200)]
        public string? Company { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; } = null!;

        public bool? IsConsultant { get; set; }

        [Required]
        [StringLength(2000)]
        public string Message { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;

        [StringLength(15)]
        public string? Phone { get; set; }
    }
}