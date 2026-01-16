using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;

namespace EdgePMO.API.Dtos
{
    public record PageContentCreateDto
    {
        [Required]
        [StringLength(200)]
        public string Slug { get; init; } = null!;

        [Required]
        [StringLength(200)]
        public string Name { get; init; } = null!;

        [Required]
        public JsonObject Data { get; init; } = new JsonObject();

        public bool IsActive { get; init; } = true;
    }
}
