namespace EdgePMO.API.Dtos
{
    public class CreateCourseReviewDto
    {
        public Guid CourseId { get; set; }
        public Guid UserId { get; set; }
        public string? Name { get; set; }
        public double Rating { get; set; }
        public string Text { get; set; } = null!;
    }
}
