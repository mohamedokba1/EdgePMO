using EdgePMO.API.Models;
using System.ComponentModel.DataAnnotations;

namespace EdgePMO.API.Dtos
{
    public record CourseUpdateDto
    {
        [Required]
        public Guid CourseId { get; set; }

        [StringLength(200)]
        public string? Name { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        [StringLength(10)]
        public string? Duration { get; set; }

        public double? Price { get; set; }

        [StringLength(1000)]
        public string? CoursePictureUrl { get; set; }

        [StringLength(4000)]
        public string? Overview { get; set; }

        [StringLength(4000)]
        public string? Subtitle { get; set; }

        [StringLength(10000)]
        public string? MainObjective { get; set; }

        public int? Sessions { get; set; }

        public string? Level { get; set; }

        public double? Rating { get; set; }

        public int? Students { get; set; }

        public Guid? InstructorId { get; set; }

        public string? Category { get; set; }

        public bool? Certification { get; set; }

        public bool? IsActive { get; set; }

        public List<string>? SoftwareUsed { get; set; } = new();

        public List<string>? WhatStudentsLearn { get; set; } = new();

        public List<string>? WhoShouldAttend { get; set; } = new();

        public List<string>? Requirements { get; set; } = new();
    }
}
