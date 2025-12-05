namespace EdgePMO.API.Models
{
    public class CourseUser
    {
        public Guid CourseId { get; set; }
        public Course Course { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }
        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
        public double Progress { get; set; } = 0.0;
    }
}
