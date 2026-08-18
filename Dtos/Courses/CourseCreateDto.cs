using EdgePMO.API.Dtos.Courses;
using System.ComponentModel.DataAnnotations;

namespace EdgePMO.API.Dtos
{
    public record CourseCreateDto
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = null!;

        [StringLength(2000)]
        public string? Description { get; set; }

        [StringLength(2000)]
        public string? Subtitle { get; set; }

        [StringLength(10000)]
        public string? MainObjective { get; set; }

        [StringLength(2000)]
        public string? Level { get; set; }

        [Required(ErrorMessage = "No. of sessions can not be null or empty")]
        public int Sessions { get; set; }

        [StringLength(100)]
        public string? Category { get; set; }

        [Required(ErrorMessage = "Course picture ID is required.")]
        public Guid? CoursePictureId { get; set; }

        [StringLength(10000)]
        public string? Overview { get; set; }

        [MaxLength(20)]
        public List<string> WhatStudentsLearn { get; set; } = new();

        [MaxLength(10)]
        public List<string> SoftwareUsed { get; set; } = new();

        [MaxLength(15)]
        public List<string> Requirements { get; set; } = new();

        [MaxLength(20)]
        public List<string> WhoShouldAttend { get; set; } = new();

        [Required]
        [MinLength(1)]
        public List<CourseContentDto> Content { get; set; } = new();

        [Range(0, double.MaxValue)]
        [Required]
        public double Price { get; set; }

        // Match ReadDto
        public string? Duration { get; set; }

        public bool Certification { get; set; } = false;

        [Required]
        public Guid InstructorId { get; set; }

        public bool IsPublic { get; set; } = true;

        public bool ShowStudentsCount { get; set; } = true;

        [Range(0, double.MaxValue)]
        public double? OriginalPrice { get; set; }
    }
}