namespace EdgePMO.API.Dtos
{
    public class CourseVideoReadDto
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = null!;
        public string? Description { get; init; }
        public string Url { get; init; } = null!;
        public int DurationSeconds { get; init; }
        public int Order { get; init; }
    }
}
