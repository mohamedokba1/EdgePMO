namespace EdgePMO.API.Models
{
    public class Course
    {
        public Guid CourseId { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Description { get; set; }
        public string CoursePictureUrl { get; set; }
        public string Overview { get; set; }
        public string WhatStudentsLearn { get; set; }
        public string SessionsBreakdown { get; set; }

        public Guid InstructorId { get; set; }
        public Instructor Instructor { get; set; }

        public List<Testimonial> Testimonials { get; set; } = new();
        public List<Certificate> Certificates { get; set; } = new();
        public List<CourseVideo> CourseVideos { get; set; } = new();
        public List<CourseUser> CourseUsers { get; set; } = new();
        public ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
    }
}
