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

        [StringLength(1000)]
        public string? CoursePictureUrl { get; set; }

        [StringLength(10000)]
        public string? Overview { get; set; }

        [MaxLength(20, ErrorMessage = "Maximum 20 learning points allowed")]
        public List<string> WhatStudentsLearn { get; set; } = new List<string>();

        [MaxLength(10, ErrorMessage = "Maximum 10 software requirements allowed")]
        public List<string> SoftwareRequirements{ get; set; } = new List<string>();

        [MaxLength(15, ErrorMessage = "Maximum 15 requirements allowed")]
        public List<string> Requirements { get; set; } = new List<string>();

        [MaxLength(20, ErrorMessage = "Maximum 20 audience groups allowed")]
        public List<string> WhoShouldAttend { get; set; } = new List<string>();

        [Required]
        [MinLength(1, ErrorMessage = "At least one course section is required")]
        public List<CourseContentDto> Content { get; set; }

        [Range(0, double.MaxValue)]
        [Required]
        public double Price { get; set; }

        public double? Duration { get; set; }

        public bool HasCertificate { get; set; } = false;

        [Required]
        public Guid InstructorId { get; set; }
    }
}
