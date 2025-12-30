namespace EdgePMO.API.Dtos
{
    public record InstructorReadDto
    {
        public Guid InstructorId { get; init; }
        public string InstructorName { get; init; } = null!;
        public string? Profile { get; init; }
        public string? ProfileImageUrl { get; init; }
        public string? Title { get; init; }
    }
}
