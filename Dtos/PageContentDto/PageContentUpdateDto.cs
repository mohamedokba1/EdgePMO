using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;

namespace EdgePMO.API.Dtos
{
    public record PageContentUpdateDto
    {
        [Required]
        public Guid Id { get; init; }

        [StringLength(200)]
        public string? Slug { get; init; }

        [StringLength(200)]
        public string? Name { get; init; }

        public JsonObject? Data { get; init; }

        public bool? IsActive { get; init; }
    }
}
