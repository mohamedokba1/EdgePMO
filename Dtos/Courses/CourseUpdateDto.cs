using System.ComponentModel.DataAnnotations;

namespace EdgePMO.API.Dtos
{
    public record CourseUpdateDto
    {
        [Required]
        public Guid CourseId { get; set; }

        [StringLength(200)]
        public string Name { get; set; } = null!;

        [StringLength(2000)]
        public string? Description { get; set; }

        [StringLength(1000)]
        public string? CoursePictureUrl { get; set; }

        [StringLength(4000)]
        public string? Overview { get; set; }

        [StringLength(4000)]
        public string? WhatStudentsLearn { get; set; }

        [StringLength(4000)]
        public string? SessionsBreakdown { get; set; }

        public Guid InstructorId { get; set; }
    }
}
