using System.Text.Json;
using System.Text.Json.Nodes;

namespace EdgePMO.API.Dtos
{
    public record PageContentReadDto
    {
        public Guid Id { get; init; }
        public string Slug { get; init; } = null!;
        public string Name { get; init; } = null!;
        public JsonElement Data { get; init; }
        public bool IsActive { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
    }
}
