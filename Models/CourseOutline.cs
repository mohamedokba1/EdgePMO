namespace EdgePMO.API.Models
{
    public class CourseOutline
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid CourseId { get; set; }
        public string Title { get; set; }
        public int Order { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Course Course { get; set; }
        public List<CourseVideo> Videos { get; set; } = new();
        public List<CourseDocument> Documents { get; set; } = new();
    }
}
