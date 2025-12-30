using System.Text.Json.Serialization;

namespace EdgePMO.API.Dtos
{
    public record PurchaseRequestResponseDto
    {
        public Guid Id { get; init; }
        public Guid UserId { get; init; }
        public Guid? TemplateId { get; init; }
        public Guid? CourseId { get; init; }
        public string? Notes { get; init; }
        public string Status { get; init; } = null!;
        public Guid? AdminId { get; init; }
        public DateTime RequestedAt { get; init; }
        public DateTime? DecisionAt { get; init; }
        public string? IdempotencyKey { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public TemplateBriefDto? Template { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public CourseBriefDto? Course { get; init; }

    }

    public record TemplateBriefDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = null!;
        public string? Description { get; init; }
        public decimal Price { get; init; }
        public string? Category { get; init; }
        public string? CoverImageUrl { get; init; }
        public string? FilePath { get; init; }
        public bool IsActive { get; init; }
    }

    public record CourseBriefDto
    {
        public Guid CourseId { get; init; }
        public string Name { get; init; } = null!;
        public decimal Price { get; init; }
        public string? Description { get; init; }
        public string? CoursePictureUrl { get; init; }
    }
}
