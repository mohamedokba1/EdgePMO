namespace EdgePMO.API.Models
{
    public class CourseVideo
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid CourseOutlineId { get; set; }
        public CourseOutline CourseOutline { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string Url { get; set; }
        public int DurationMinutes { get; set; }
        public int Order { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
