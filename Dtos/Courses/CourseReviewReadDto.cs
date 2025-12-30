namespace EdgePMO.API.Dtos
{
    public class CourseReviewReadDto
    {
        public Guid Id { get; set; }
        public string? Email { get; set; }
        public string? Username { get; set; }
        public string? Header { get; set; }
        public double? Rating { get; set; }
        public string? Content { get; set; }
    }
}
