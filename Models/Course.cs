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

        // Requirement 4.3 — admin-controlled display order on the public course list.
        // Lower sorts first. New courses default to the back of the list (see
        // CoursesServices.CreateAsync, which sets this to max+1 at creation time).
        public int SortOrder { get; set; } = 0;

        // Requirement 4.4 — per-course visibility. True (default): listed and reachable
        // by anyone. False: hidden from the public list and blocked by direct URL unless
        // the requesting user has actually purchased/enrolled in it.
        public bool IsPublic { get; set; } = true;

        // Requirement 4.5 — admin can hide the registered-student count on the public
        // course card/details page without affecting the underlying enrollment data.
        public bool ShowStudentsCount { get; set; } = true;

        // Requirement 4.1 (narrowed scope — see phase decision) — a simple "was/now"
        // discount shown to all users. Null/unset means no discount; when set, it must
        // be greater than Price for the discount to display (enforced at the DTO level).
        public double? OriginalPrice { get; set; }
    }
}
