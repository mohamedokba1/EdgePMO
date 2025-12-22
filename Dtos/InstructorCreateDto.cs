using System;
using System.ComponentModel.DataAnnotations;

namespace EdgePMO.API.Dtos
{
    public record InstructorCreateDto
    {
        [Required]
        [StringLength(200)]
        public string InstructorName { get; init; } = null!;

        [StringLength(2000)]
        public string? Profile { get; init; }

        [StringLength(500)]
        public string? ProfileImageUrl { get; init; }
    }
}