namespace EdgePMO.API.Dtos
{
    public class ConsultationRequestReadDto
    {
        public string Company { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public string Email { get; set; } = null!;
        public Guid Id { get; set; }
        public bool? IsConsultant { get; set; }
        public string Message { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Phone { get; set; }
    }
}