namespace EdgePMO.API.Dtos
{
    public class CourseReadDto
    {
        public Guid CourseId { get; init; }
        public string Name { get; init; } = null!;
        public string? Description { get; init; }
        public string? CoursePictureUrl { get; init; }
        public string? Overview { get; init; }
        public string? WhatStudentsLearn { get; init; }
        public string? SessionsBreakdown { get; init; }
        public InstructorReadDto? Instructor { get; init; }

        public List<CourseVideoReadDto> CourseVideos { get; init; } = new();
        public List<UserReadDto> Students { get; init; } = new();
    }
}
