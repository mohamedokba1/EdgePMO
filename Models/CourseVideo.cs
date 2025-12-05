namespace EdgePMO.API.Models
{
    public class CourseVideo
    {
        public Guid CourseVideoId { get; set; } = Guid.NewGuid();
        public Guid CourseId { get; set; }
        public Course Course { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string VideoUrl { get; set; }
        public int DurationSeconds { get; set; }
        public int Order { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
