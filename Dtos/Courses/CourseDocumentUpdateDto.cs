using System.ComponentModel.DataAnnotations;

namespace EdgePMO.API.Dtos.Courses
{
    public class CourseDocumentUpdateDto
    {
        public string? Description { get; init; }
        public Guid? Id { get; init; }
        public int? Order { get; init; }
        public string? Title { get; init; }
        public Guid? Url { get; init; }
    }
}