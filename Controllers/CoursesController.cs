using EdgePMO.API.Contracts;
using EdgePMO.API.Dtos;
using EdgePMO.API.Dtos.Courses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;

namespace EdgePMO.API.Controllers
{
    [Route("api/v1.0/[controller]")]
    [ApiController]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseServices _courseServices;
        private readonly ICourseReviewServices _courseReviewServices;

        public CoursesController(ICourseServices courseServices, ICourseReviewServices courseReviewServices)
        {
            _courseServices = courseServices;
            _courseReviewServices = courseReviewServices;
        }

        // Deliberately anonymous-accessible (public course list/details), but still reads
        // the caller's identity when a token IS attached — requirement 4.4 needs to know
        // "is this an admin" and "has this user purchased the hidden course" without
        // requiring login for the normal public-browsing case.
        private (Guid? userId, bool isAdmin) GetCallerContext()
        {
            string? userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid? userId = Guid.TryParse(userIdClaim, out Guid parsed) ? parsed : null;
            bool isAdmin = User.FindFirst(ClaimTypes.Role)?.Value == "admin";
            return (userId, isAdmin);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            (Guid? userId, bool isAdmin) = GetCallerContext();
            Response? courses = await _courseServices.GetAllAsync(userId, isAdmin);
            return StatusCode((int)courses.Code, courses);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            (Guid? userId, bool isAdmin) = GetCallerContext();
            Response? courseResponse = await _courseServices.GetByIdAsync(id, userId, isAdmin);
            return StatusCode((int)courseResponse.Code, courseResponse);
        }

        [HttpPatch("reorder")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> Reorder([FromBody] List<Guid> orderedCourseIds)
        {
            Response? response = await _courseServices.ReorderAsync(orderedCourseIds);
            return StatusCode((int)response.Code, response);
        }

        [HttpPost("video")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> CreateCourseVideo(CourseVideoCreateDto dto)
        {
            Response? created = await _courseServices.AttachCourseVideoAsync(dto);
            return StatusCode((int)created.Code, created);
        }

        [HttpPut("video")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> UpdateCourseVideo(CourseVideoUpdateDto dto)
        {
            Response? created = await _courseServices.UpdateCourseVideoAsync(dto);
            return StatusCode((int)created.Code, created);
        }

        [HttpDelete("video/{id:guid}")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> DeleteCourseVideo(Guid id)
        {
            Response? deleteResponse = await _courseServices.DeleteCourseVideoAsync(id);
            return StatusCode((int)deleteResponse.Code, deleteResponse);
        }

        [HttpPost]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> CreateCourse(CourseCreateDto dto)
        {
            Response? created = await _courseServices.CreateAsync(dto);
            return StatusCode((int)created.Code, created);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> UpdateCourse(Guid id, CourseUpdateDto courseUpdateDto)
        {
            if (id != courseUpdateDto.CourseId)
            {
                return StatusCode(400, new Response()
                {
                    IsSuccess = false,
                    Message = "Route id and course.CourseId must match.",
                    Code = HttpStatusCode.BadRequest
                });
            }

            Response? updateResponse = await _courseServices.UpdateAsync(courseUpdateDto);
            return StatusCode((int)updateResponse.Code, updateResponse);
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> DeleteCourse(Guid id)
        {
            Response? deleteResponse = await _courseServices.DeleteAsync(id);
            return StatusCode((int)deleteResponse.Code, deleteResponse);
        }

        // Admin-only moderation view — returns every review including hidden ones.
        // Never called by the customer-facing course-details page (reviews arrive
        // embedded in the course response, already filtered per GetAllAsync/GetByIdAsync
        // below).
        [HttpGet("{id:guid}/reviews")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> GetCourseReviews(Guid id)
        {
            Response? resp = await _courseReviewServices.GetByCourseIdAsync(id);
            return StatusCode((int)resp.Code, resp);
        }

        [HttpGet("reviews/{id:guid}")]
        [Authorize]
        public async Task<IActionResult> GetCourseReviewById(Guid id)
        {
            Response? resp = await _courseReviewServices.GetByIdAsync(id);
            return StatusCode((int)resp.Code, resp);
        }

        [HttpPost("reviews")]
        [Authorize]
        public async Task<IActionResult> CreateCourseReview([FromBody] CreateCourseReviewDto dto)
        {
            Response? resp = await _courseReviewServices.CreateAsync(dto);
            return StatusCode((int)resp.Code, resp);
        }

        [HttpPut("reviews")]
        [Authorize]
        public async Task<IActionResult> UpdateCourseReview([FromBody] UpdateCourseReviewDto dto)
        {
            Response? resp = await _courseReviewServices.UpdateAsync(dto);
            return StatusCode((int)resp.Code, resp);
        }

        // Was [Authorize] only — any logged-in user could delete any review, not
        // just an admin or the review's own author. Tightened to match this being
        // an admin moderation action.
        [HttpDelete("reviews/{id:guid}")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> DeleteCourseReviewById(Guid id)
        {
            Response? deleteResponse = await _courseReviewServices.DeleteAsync(id);
            return StatusCode((int)deleteResponse.Code, deleteResponse);
        }

        [HttpPatch("reviews/{id:guid}/hide")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> SetCourseReviewHidden(Guid id, [FromBody] SetReviewHiddenDto dto)
        {
            Response? resp = await _courseReviewServices.SetHiddenAsync(id, dto.IsHidden);
            return StatusCode((int)resp.Code, resp);
        }

        [HttpGet("{id:guid}/students")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> GetEnrolledUsers(Guid id)
        {
            Response? resp = await _courseServices.GetEnrolledUsersAsync(id);
            return StatusCode((int)resp.Code, resp);
        }

        [HttpPost("{id:guid}/students")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> EnrollUser(Guid id, [FromBody] ListOfUsersEmails dto)
        {
            Response? resp = await _courseServices.EnrollUsersByEmailsAsync(id, dto.Emails, dto.Amount, dto.Currency);
            return StatusCode((int)resp.Code, resp);
        }

        [HttpDelete("{id:guid}/students")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> UnenrollUser(Guid id, [FromBody] ListOfUsersEmails dto)
        {
            Response? resp = await _courseServices.UnenrollUsersByEmailsAsync(id, dto.Emails);
            return StatusCode((int)resp.Code, resp);
        }

        [HttpPost("{id:guid}/students/status")]
        public async Task<IActionResult> IsEnrolled(Guid id, [FromBody] ListOfUsersEmails dto)
        {
            Response? resp = await _courseServices.IsUsersEnrolledAsync(id, dto.Emails);
            return StatusCode((int)resp.Code, resp);
        }

        [HttpPatch("sync-progress")]
        [Authorize]
        public async Task<IActionResult> SyncProgress([FromBody] ProgressUpdateDto dto)
        {
            string? userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

            Guid userId = Guid.Parse(userIdClaim);
            Response? response = await _courseServices.UpdateUserProgressAsync(userId, dto.CourseId, dto.Progress);
            return StatusCode((int)response.Code, response);
        }

        // Requirement 3.5 — real watched-minutes per video, more granular than the
        // single course-level Progress percentage synced above.
        [HttpPatch("videos/watch-progress")]
        [Authorize]
        public async Task<IActionResult> UpdateVideoWatchProgress([FromBody] WatchProgressUpdateDto dto)
        {
            string? userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

            Guid userId = Guid.Parse(userIdClaim);
            Response? response = await _courseServices.UpdateVideoWatchProgressAsync(userId, dto.VideoId, dto.WatchedSeconds);
            return StatusCode((int)response.Code, response);
        }

        // Requirement 5.2 — per-video view counts and watch-time for admins.
        [HttpGet("{id:guid}/video-analytics")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> GetVideoAnalytics(Guid id)
        {
            Response? response = await _courseServices.GetVideoAnalyticsAsync(id);
            return StatusCode((int)response.Code, response);
        }
    }
}
