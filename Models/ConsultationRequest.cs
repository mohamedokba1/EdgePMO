namespace EdgePMO.API.Models
{
    public class ConsultationRequest
    {
        public string Company { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public string Email { get; set; }
        public Guid Id { get; set; }
        public bool IsConsultant { get; set; }
        public string Message { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
    }
}