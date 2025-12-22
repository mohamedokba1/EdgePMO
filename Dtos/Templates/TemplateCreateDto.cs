using System.ComponentModel.DataAnnotations;

namespace EdgePMO.API.Dtos
{
    public record TemplateCreateDto
    {
        [Required]
        [StringLength(200)]
        public string Name { get; init; } = null!;

        [StringLength(2000)]
        [Required]
        public string? Description { get; init; }

        [Range(0, double.MaxValue)]
        [Required]
        public decimal Price { get; init; }

        [StringLength(100)]
        [Required]
        public string? Category { get; init; }

        [StringLength(1000)]
        [Required]
        public string? CoverImageUrl { get; init; }

        [StringLength(1000)]
        [Required]
        public string? FilePath { get; init; }

        [StringLength(1000)]
        [Required]
        public string? Type { get; init; }

        [Required]
        [StringLength(1000)]
        public string? Size { get; init; }

        [Required]
        [StringLength(1000)]
        public string? Format { get; init; }
    }
}
