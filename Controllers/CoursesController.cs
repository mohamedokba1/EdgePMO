using EdgePMO.API.Contracts;
using EdgePMO.API.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace EdgePMO.API.Controllers
{
    [Route("api/v1.0/[controller]")]
    [ApiController]
    [Authorize(Policy = "Admin")]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseServices _courseServices;

        public CoursesController(ICourseServices courseServices)
        {
            _courseServices = courseServices;
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
        public async Task<IActionResult> CreateCourseVideo(CourseVideoCreateDto dto)
        {
            Response? created = await _courseServices.AttachCourseVideoAsync(dto);
            return StatusCode((int)created.Code, created);
        }

        [HttpDelete("video/{id:guid}")]
        public async Task<IActionResult> DeleteCourseVideo(Guid id)
        {
            Response? deleteResponse = await _courseServices.DeleteCourseVideoAsync(id);
            return StatusCode((int)deleteResponse.Code, deleteResponse);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CourseCreateDto dto)
        {
            Response? created = await _courseServices.CreateAsync(dto);
            return StatusCode((int)created.Code, created);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, CourseUpdateDto courseUpdateDto)
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
        public async Task<IActionResult> Delete(Guid id)
        {
            Response? deleteResponse = await _courseServices.DeleteAsync(id);
            return StatusCode((int)deleteResponse.Code, deleteResponse);
        }

        [HttpGet("{id:guid}/students")]
        public async Task<IActionResult> GetEnrolledUsers(Guid id)
        {
            Response? resp = await _courseServices.GetEnrolledUsersAsync(id);
            return StatusCode((int)resp.Code, resp);
        }

        [HttpPost("{id:guid}/students")]
        public async Task<IActionResult> EnrollUser(Guid id, [FromBody] EnrollmentListDto dto)
        {
            Response? resp = await _courseServices.EnrollUsersByEmailsAsync(id, dto.Emails);
            return StatusCode((int)resp.Code, resp);
        }

        [HttpDelete("{id:guid}/students")]
        public async Task<IActionResult> UnenrollUser(Guid id, [FromBody] EnrollmentListDto dto)
        {
            Response? resp = await _courseServices.UnenrollUsersByEmailsAsync(id, dto.Emails);
            return StatusCode((int)resp.Code, resp);
        }

        [HttpGet("{id:guid}/students/status")]
        public async Task<IActionResult> IsEnrolled(Guid id, [FromBody] EnrollmentListDto dto)
        {
            Response? resp = await _courseServices.IsUsersEnrolledAsync(id, dto.Emails);
            return StatusCode((int)resp.Code, resp);
        }
    }
}
