namespace EdgePMO.API.Models
{
    public class Certificate
    {
        public Guid CertificateId { get; set; } = Guid.NewGuid();
        public Guid CourseId { get; set; }
        public Course Course { get; set; }
        public string CertificateTitle { get; set; }
        public string CertificateDescription { get; set; }
    }
}
