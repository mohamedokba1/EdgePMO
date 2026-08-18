namespace EdgePMO.API.Models
{
    /// <summary>
    /// One row per (user, video) — upserted as the student watches. Backs both
    /// requirement 3.5 (real watched-minutes, more granular than the single course-level
    /// Progress percentage) and 5.2 (per-video view counts and engagement for admins).
    /// </summary>
    public class VideoWatchProgress
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid CourseVideoId { get; set; }
        public CourseVideo CourseVideo { get; set; } = null!;
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        // Furthest point reached in the video, in seconds — not cumulative play time,
        // since a re-watched section shouldn't double-count. Matches how the player
        // already derives progress from video.currentTime.
        public double WatchedSeconds { get; set; }

        // Incremented once per distinct playback session (see 3.6's claim-playback-session
        // for what "a session" means here) — this is what 5.2's "views per video" counts.
        public int ViewCount { get; set; } = 1;

        public DateTime FirstWatchedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastWatchedAt { get; set; } = DateTime.UtcNow;
    }
}
