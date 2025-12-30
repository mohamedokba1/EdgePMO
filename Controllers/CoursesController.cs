using EdgePMO.API.Contracts;
using EdgePMO.API.Dtos;
using EdgePMO.API.Dtos.Courses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

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

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            Response? courses = await _courseServices.GetAllAsync();
            return StatusCode((int)courses.Code, courses);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            Response? courseResponse = await _courseServices.GetByIdAsync(id);
            return StatusCode((int)courseResponse.Code, courseResponse);
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

        [HttpGet("{id:guid}/reviews")]
        [Authorize]
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

        [HttpDelete("reviews/{id:guid}")]
        [Authorize]
        public async Task<IActionResult> DeleteCourseReviewById(Guid id)
        {
            Response? deleteResponse = await _courseReviewServices.DeleteAsync(id);
            return StatusCode((int)deleteResponse.Code, deleteResponse);
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
            Response? resp = await _courseServices.EnrollUsersByEmailsAsync(id, dto.Emails);
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
    }
}
