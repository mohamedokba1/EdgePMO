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
        public string? Level { get; set; }

        [Required(ErrorMessage = "No. of sessions can not be null or empty")]
        public int Sessions { get; set; }

        [StringLength(100)]
        public string? Category { get; set; }

        [StringLength(1000)]
        public string? CoursePictureUrl { get; set; }

        [StringLength(10000)]
        public string? Overview { get; set; }

        [StringLength(10000)]
        public List<string> WhatStudentsLearn { get; set; } = new List<string>();

        [StringLength(10000)]
        public List<string> SoftwareRequirements{ get; set; } = new List<string>();

        [StringLength(5000)]
        public List<string> Requirements { get; set; } = new List<string>();

        [StringLength(5000)]
        public List<string> WhoShouldAttend { get; set; } = new List<string>();

        public List<CourseOutlinesDto> Outlines { get; set; }

        [Range(0, double.MaxValue)]
        [Required]
        public double Price { get; set; }

        [Required]
        public Guid InstructorId { get; set; }
    }
}
