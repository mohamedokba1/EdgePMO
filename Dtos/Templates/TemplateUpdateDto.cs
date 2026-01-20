using System.ComponentModel.DataAnnotations;

namespace EdgePMO.API.Dtos
{
    public record TemplateUpdateDto
    {
        [Required]
        public Guid Id { get; init; }

        [StringLength(200)]
        public string? Name { get; init; } = null!;

        [StringLength(2000)]
        public string? Description { get; init; }

        [Range(0, double.MaxValue)]
        public decimal? Price { get; init; }

        [StringLength(100)]
        public string? Category { get; init; }

        public Guid? CoverImageId { get; init; }

        public Guid? FilePathId { get; init; }

        [StringLength(1000)]
        public string? Type { get; init; }

        [StringLength(1000)]
        public string? Format { get; init; }

        [StringLength(1000)]
        public string? Size { get; init; }

        public bool? IsActive { get; init; }
    }
}