using System.Text.Json.Serialization;

namespace EdgePMO.API.Dtos
{
    public class UserProfileDto
    {
        [JsonPropertyName("user")]
        public UserReadDto User { get; set; }

        [JsonPropertyName("courses")]
        public IEnumerable<CourseReadDto> Courses { get; set; }

        [JsonPropertyName("templates")]
        public IEnumerable<UserTemplateReadDto> Templates { get; set; }
    }
}
