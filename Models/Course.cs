namespace EdgePMO.API.Models
{
    public class Course
    {
        public Guid CourseId { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = null!;
        public string? Subtitle { get; set; }
        public string Description { get; set; } = null!;
        public string? Overview { get; set; }
        public string? MainObjective { get; set; }
        public int Sessions { get; set; }
        public string? Duration { get; set; }
        public string? Level { get; set; }
        public double Price { get; set; }
        public double? Rating { get; set; }
        public int? Students { get; set; }
        public string? CoursePictureUrl { get; set; }
        public Guid InstructorId { get; set; }
        public Instructor Instructor { get; set; } = null!;
        public string? Category { get; set; }
        public bool Certification { get; set; }
        public List<string> SoftwareUsed { get; set; } = new();
        public List<string> WhatStudentsLearn { get; set; } = new();
        public List<string> WhoShouldAttend { get; set; } = new();
        public List<string> Requirements { get; set; } = new();

        public List<Testimonial> Testimonials { get; set; } = new();
        public List<CourseReview> Reviews { get; set; } = new();
        public List<Certificate> Certificates { get; set; } = new();
        public List<CourseUser> CourseUsers { get; set; } = new();
        public ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
        public List<CourseOutline> CourseOutline { get; set; } = new();
        public bool? IsActive { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
