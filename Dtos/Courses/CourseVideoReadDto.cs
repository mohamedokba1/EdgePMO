namespace EdgePMO.API.Dtos
{
    public class CourseVideoReadDto
    {
        public Guid CourseVideoId { get; init; }
        public string Title { get; init; } = null!;
        public string? Description { get; init; }
        public string VideoUrl { get; init; } = null!;
        public int DurationSeconds { get; init; }
        public int Order { get; init; }
        public DateTime CreatedAt { get; init; }
    }
}
