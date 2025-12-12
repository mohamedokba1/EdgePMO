using System.ComponentModel.DataAnnotations;

namespace EdgePMO.API.Dtos.Courses
{
    public record CourseOutlinesDto
    {
        [Required(ErrorMessage = "Title of outline is required")]
        public string Title { get; init; }
        public List<string> Items { get; init; }

        [Required]
        public int Order { get; init; }
    }
}
