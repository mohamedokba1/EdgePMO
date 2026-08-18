using EdgePMO.API.Dtos.Courses;

namespace EdgePMO.API.Dtos
{
    public class CourseReadDto
    {
        public Guid CourseId { get; init; }
        public string Name { get; init; } = null!;
        public string? Subtitle { get; init; }
        public string? Description { get; init; }
        public int Sessions { get; init; }
        public string? Duration { get; init; }
        public string? Level { get; init; }
        public double Price { get; init; }
        public double? Rating { get; init; }
        public int? Students { get; init; }
        public string? CoursePictureUrl { get; init; }
        public InstructorReadDto? Instructor { get; init; }
        public string? Category { get; init; }
        public bool Certification { get; init; }
        public List<string> SoftwareUsed { get; init; } = new();
        public string? MainObjective { get; init; }
        public List<string> WhatStudentsLearn { get; init; } = new();
        public List<string> WhoShouldAttend { get; init; } = new();
        public List<string> Requirements { get; init; } = new();
        public List<CourseContentReadDto> Content { get; init; } = new();
        public List<CourseReviewReadDto> Reviews { get; init; } = new();
        public List<CourseStudentDto> StudentsList { get; init; } = new();
        public int SortOrder { get; init; }
        public bool IsPublic { get; init; } = true;
        public bool ShowStudentsCount { get; init; } = true;
        public double? OriginalPrice { get; init; }
    }
}
