namespace EdgePMO.API.Models
{
    public class CourseReview
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid CourseId { get; set; }
        public Course? Course { get; set; }
        public Guid UserId { get; set; }
        public User? User { get; set; }
        public string? Header { get; set; }
        public double Rating { get; set; }
        public string Content { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
