namespace EdgePMO.API.Dtos
{
    public class CourseVideoReadDto
    {
        public string? Description { get; init; }
        public int DurationMinutes { get; init; }
        public Guid Id { get; init; }
        public int Order { get; init; }
        public string Title { get; init; } = null!;
        public string Url { get; init; } = null!;
    }
}