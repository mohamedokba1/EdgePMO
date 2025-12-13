namespace EdgePMO.API.Models
{
    public class CourseDocument
    {
        public Guid CourseDocumentId { get; set; } = Guid.NewGuid();
        public Guid CourseOutlineId { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string DocumentUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public CourseOutline CourseOutline { get; set; }
    }
}
