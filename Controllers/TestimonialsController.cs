using EdgePMO.API.Contracts;
using EdgePMO.API.Dtos;
using EdgePMO.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EdgePMO.API.Controllers
{
    [Route("v1.0/[controller]")]
    [ApiController]
    public class TestimonialsController : ControllerBase
    {
        private readonly ITestimonialServices _testimonialServices;
        private readonly EdgepmoDbContext _dbContext;

        public TestimonialsController(ITestimonialServices testimonialServices, EdgepmoDbContext dbContext)
        {
            _testimonialServices = testimonialServices;
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _testimonialServices.GetAllAsync();
            return Ok(list);
        }

        [HttpGet("course/{courseId:guid}")]
        public async Task<IActionResult> GetByCourse(Guid courseId)
        {
            var courseExists = await _dbContext.Courses.AnyAsync(c => c.CourseId == courseId);
            if (!courseExists)
                return NotFound();

            var testimonials = await _testimonialServices.GetByCourseIdAsync(courseId);
            return Ok(testimonials);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var t = await _testimonialServices.GetByIdAsync(id);
            if (t == null)
                return NotFound();

            return Ok(t);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] TestimonialCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var courseExists = await _dbContext.Courses.AnyAsync(c => c.CourseId == dto.CourseId);
            if (!courseExists)
                return BadRequest(new { CourseId = "Course does not exist." });

            var testimonial = new Testimonial
            {
                CourseId = dto.CourseId,
                StudentName = dto.StudentName.Trim(),
                Comment = dto.Comment.Trim(),
                Rating = dto.Rating
            };

            var created = await _testimonialServices.CreateAsync(testimonial);
            return CreatedAtAction(nameof(Get), new { id = created.TestimonialId }, created);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] TestimonialUpdateDto dto)
        {
            if (id != dto.TestimonialId)
                return BadRequest("Route id and body TestimonialId must match.");

            bool courseExists = await _dbContext.Courses.AnyAsync(c => c.CourseId == dto.CourseId);
            if (!courseExists)
                return BadRequest(new { CourseId = "Course does not exist." });

            Testimonial? testimonial = new Testimonial
            {
                TestimonialId = dto.TestimonialId,
                CourseId = dto.CourseId,
                StudentName = dto.StudentName.Trim(),
                Comment = dto.Comment.Trim(),
                Rating = dto.Rating
            };

            bool updated = await _testimonialServices.UpdateAsync(testimonial);
            if (!updated)
                return NotFound();

            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            bool deleted = await _testimonialServices.DeleteAsync(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}