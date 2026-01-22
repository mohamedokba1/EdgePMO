using System.ComponentModel.DataAnnotations;

namespace EdgePMO.API.Dtos
{
    public class ConsultationRequestUpdateDto
    {
        [StringLength(200)]
        public string? Company { get; set; }

        [EmailAddress]
        [StringLength(200)]
        public string? Email { get; set; }

        [Required]
        public Guid Id { get; set; }

        public bool? IsConsultant { get; set; }

        [StringLength(2000)]
        public string? Message { get; set; }

        [StringLength(100)]
        public string? Name { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }
    }
}