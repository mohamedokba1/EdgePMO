namespace EdgePMO.API.Models
{
    public class Instructor
    {
        public Guid InstructorId { get; set; } = Guid.NewGuid();
        public string InstructorName { get; set; }
        public string Title { get; set; }
        public string Profile { get; set; }
        public string ProfileImageUrl { get; set; }
        public List<Course> Courses { get; set; } = new();
    }
}
