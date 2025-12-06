using System.ComponentModel.DataAnnotations;

namespace EdgePMO.API.Dtos
{
    public record TemplateUpdateDto
    {
        [Required]
        public Guid Id { get; init; }

        [Required]
        [StringLength(200)]
        public string Name { get; init; } = null!;

        [StringLength(2000)]
        public string? Description { get; init; }

        [Range(0, double.MaxValue)]
        public decimal Price { get; init; }

        [StringLength(100)]
        public string? Category { get; init; }

        [StringLength(1000)]
        public string? ImageUrl { get; init; }

        public bool IsActive { get; init; } = true;
    }
}
