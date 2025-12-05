namespace EdgePMO.API.Models
{
    public class Testimonial
    {
        public Guid TestimonialId { get; set; } = Guid.NewGuid();
        public Guid CourseId { get; set; }
        public Course Course { get; set; }
        public string StudentName { get; set; }
        public string Comment { get; set; }
        public int Rating { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
