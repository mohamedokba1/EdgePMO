namespace EdgePMO.API.Dtos
{
    /// <summary>
    /// A student entry on a course's StudentsList — unlike UserReadDto, this carries the
    /// per-enrollment data (Progress, EnrolledAt) that lives on the CourseUser join row,
    /// not on the User itself. Fixes requirement 3.1: CourseUser.Progress was written
    /// correctly by sync-progress but never returned by any read endpoint, because
    /// StudentsList was previously typed as the generic UserReadDto.
    /// </summary>
    public record CourseStudentDto
    {
        public Guid Id { get; init; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public double Progress { get; set; }
        public DateTime EnrolledAt { get; set; }
    }
}
