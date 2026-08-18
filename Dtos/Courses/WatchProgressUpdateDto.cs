using System.ComponentModel.DataAnnotations;

namespace EdgePMO.API.Dtos.Courses
{
    /// <summary>Requirement 3.5 — payload for reporting how far a user watched into a video.</summary>
    public class WatchProgressUpdateDto
    {
        [Required(ErrorMessage = "VideoId can not be null or empty")]
        public Guid VideoId { get; set; }

        [Required(ErrorMessage = "WatchedSeconds value can not be null or empty")]
        public double WatchedSeconds { get; set; }
    }
}
