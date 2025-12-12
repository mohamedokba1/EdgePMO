namespace EdgePMO.API.Models
{
    public class CourseOutline
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid CourseId { get; set; }
        public Course Course { get; set; }
        public string Title { get; set; }
        public List<string> Items { get; set; } = new();
        public int Order { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
