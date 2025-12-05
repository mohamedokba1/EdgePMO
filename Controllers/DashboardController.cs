//using EdgePMO.API.Contracts;
//using EdgePMO.API.Dtos;
//using EdgePMO.API.Models;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;

//namespace EdgePMO.API.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    [Authorize(Policy = "AdminsOnly")]
//    public class DashboardController : ControllerBase
//    {
//        private readonly ICourseServices _courseServices;
//        private readonly IInstructorServices _instructorServices;
//        private readonly ITestimonialServices _testimonialServices;
//        private readonly IUserServices _userServices;
//        private readonly IWebHostEnvironment _env;

//        private const string UploadsRelative = "uploads";

//        public DashboardController(
//            ICourseServices courseServices,
//            IInstructorServices instructorServices,
//            ITestimonialServices testimonialServices,
//            IUserServices userServices,
//            IWebHostEnvironment env)
//        {
//            _courseServices = courseServices;
//            _instructorServices = instructorServices;
//            _testimonialServices = testimonialServices;
//            _userServices = userServices;
//            _env = env;
//        }

//        // Admin summary
//        [HttpGet("summary")]
//        public async Task<IActionResult> Summary()
//        {
//            int courses = (await _courseServices.GetAllAsync()).Count();
//            int instructors = (await _instructorServices.GetAllAsync()).Count();
//            int testimonials = (await _testimonialServices.GetAllAsync()).Count();
//            int users = (await _userServices.GetAllUsersAsync()).Count();

//            string? uploadsPath = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), UploadsRelative);
//            var assetsCount = Directory.Exists(uploadsPath) ? Directory.EnumerateFiles(uploadsPath, "*", SearchOption.TopDirectoryOnly).Count() : 0;

//            return Ok(new
//            {
//                courses,
//                instructors,
//                testimonials,
//                users,
//                assets = assetsCount
//            });
//        }

//        // --------------------
//        // Course management
//        // --------------------
//        [HttpGet("courses")]
//        public async Task<IActionResult> GetCourses() => Ok(await _courseServices.GetAllAsync());

//        [HttpPost("courses")]
//        public async Task<IActionResult> CreateCourse([FromBody] CourseCreateDto dto)
//        {
//            if (!ModelState.IsValid) return BadRequest(ModelState);

//            // Map DTO -> entity
//            var course = new Course
//            {
//                Name = dto.Name.Trim(),
//                Description = dto.Description?.Trim(),
//                CoursePictureUrl = dto.CoursePictureUrl,
//                Overview = dto.Overview,
//                WhatStudentsLearn = dto.WhatStudentsLearn,
//                SessionsBreakdown = dto.SessionsBreakdown,
//                InstructorId = dto.InstructorId
//            };

//            var created = await _courseServices.CreateAsync(course);
//            return CreatedAtAction(nameof(GetCourse), new { id = created.CourseId }, created);
//        }

//        [HttpGet("courses/{id:guid}")]
//        public async Task<IActionResult> GetCourse(Guid id)
//        {
//            var course = await _courseServices.GetByIdAsync(id);
//            if (course == null) return NotFound();
//            return Ok(course);
//        }

//        [HttpPut("courses/{id:guid}")]
//        public async Task<IActionResult> UpdateCourse(Guid id, [FromBody] CourseCreateDto dto)
//        {
//            if (!ModelState.IsValid) return BadRequest(ModelState);

//            var existing = await _courseServices.GetByIdAsync(id);
//            if (existing == null) return NotFound();

//            existing.Name = dto.Name.Trim();
//            existing.Description = dto.Description?.Trim();
//            existing.CoursePictureUrl = dto.CoursePictureUrl;
//            existing.Overview = dto.Overview;
//            existing.WhatStudentsLearn = dto.WhatStudentsLearn;
//            existing.SessionsBreakdown = dto.SessionsBreakdown;
//            existing.InstructorId = dto.InstructorId;

//            var ok = await _courseServices.UpdateAsync(existing);
//            if (!ok) return NotFound();
//            return NoContent();
//        }

//        [HttpDelete("courses/{id:guid}")]
//        public async Task<IActionResult> DeleteCourse(Guid id)
//        {
//            var ok = await _courseServices.DeleteAsync(id);
//            if (!ok) return NotFound();
//            return NoContent();
//        }

//        // --------------------
//        // Instructor management
//        // --------------------
//        [HttpGet("instructors")]
//        public async Task<IActionResult> GetInstructors() => Ok(await _instructorServices.GetAllAsync());

//        [HttpPost("instructors")]
//        public async Task<IActionResult> CreateInstructor([FromBody] InstructorCreateDto dto)
//        {
//            if (!ModelState.IsValid) return BadRequest(ModelState);

//            var instructor = new Instructor
//            {
//                InstructorName = dto.InstructorName.Trim(),
//                Profile = dto.Profile?.Trim()
//            };

//            var created = await _instructorServices.CreateAsync(instructor);
//            return CreatedAtAction(nameof(GetInstructor), new { id = created.InstructorId }, created);
//        }

//        [HttpGet("instructors/{id:guid}")]
//        public async Task<IActionResult> GetInstructor(Guid id)
//        {
//            var inst = await _instructorServices.GetByIdAsync(id);
//            if (inst == null) return NotFound();
//            return Ok(inst);
//        }

//        [HttpPut("instructors/{id:guid}")]
//        public async Task<IActionResult> UpdateInstructor(Guid id, [FromBody] InstructorUpdateDto dto)
//        {
//            if (!ModelState.IsValid) return BadRequest(ModelState);
//            if (id != dto.InstructorId) return BadRequest("Route id and body InstructorId must match.");

//            var instructor = new Instructor
//            {
//                InstructorId = dto.InstructorId,
//                InstructorName = dto.InstructorName.Trim(),
//                Profile = dto.Profile?.Trim()
//            };

//            var ok = await _instructorServices.UpdateAsync(instructor);
//            if (!ok) return NotFound();
//            return NoContent();
//        }

//        [HttpDelete("instructors/{id:guid}")]
//        public async Task<IActionResult> DeleteInstructor(Guid id)
//        {
//            var ok = await _instructorServices.DeleteAsync(id);
//            if (!ok) return NotFound();
//            return NoContent();
//        }

//        // --------------------
//        // User management (exposes basic CRUD)
//        // --------------------
//        [HttpGet("users")]
//        public async Task<IActionResult> GetUsers() => Ok(await _userServices.GetAllUsersAsync());

//        //[HttpGet("users/{id:guid}")]
//        //public async Task<IActionResult> GetUser(Guid id)
//        //{
//        //    var u = await _userServices.GetById(id);
//        //    if (u == null) return NotFound();
//        //    return Ok(u);
//        //}

//        //[HttpDelete("users/{id:guid}")]
//        //public async Task<IActionResult> DeleteUser(Guid id)
//        //{
//        //    var ok = await _userServices.DeleteAsync(id);
//        //    if (!ok) return NotFound();
//        //    return NoContent();
//        //}

//        // --------------------
//        // Testimonials management
//        // --------------------
//        [HttpGet("testimonials")]
//        public async Task<IActionResult> GetTestimonials() => Ok(await _testimonialServices.GetAllAsync());

//        [HttpDelete("testimonials/{id:guid}")]
//        public async Task<IActionResult> DeleteTestimonial(Guid id)
//        {
//            var ok = await _testimonialServices.DeleteAsync(id);
//            if (!ok) return NotFound();
//            return NoContent();
//        }

//        // --------------------
//        // Asset uploads (wwwroot/uploads)
//        // --------------------
//        [HttpGet("assets")]
//        public IActionResult ListAssets()
//        {
//            var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
//            var uploads = Path.Combine(webRoot, UploadsRelative);
//            if (!Directory.Exists(uploads)) return Ok(Array.Empty<string>());

//            var files = Directory.EnumerateFiles(uploads)
//                .Select(f => $"{Request.Scheme}://{Request.Host}/{UploadsRelative}/{Path.GetFileName(f)}");
//            return Ok(files);
//        }

//        [HttpPost("assets")]
//        [RequestSizeLimit(50_000_000)] // allow up to ~50MB; adjust as needed
//        public async Task<IActionResult> UploadAsset(IFormFile file)
//        {
//            if (file == null || file.Length == 0) return BadRequest("file is required");

//            var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
//            var uploads = Path.Combine(webRoot, UploadsRelative);
//            Directory.CreateDirectory(uploads);

//            var ext = Path.GetExtension(file.FileName);
//            var fileName = $"{Guid.NewGuid()}{ext}";
//            var filePath = Path.Combine(uploads, fileName);

//            // save file
//            await using (var stream = System.IO.File.Create(filePath))
//            {
//                await file.CopyToAsync(stream);
//            }

//            var url = $"{Request.Scheme}://{Request.Host}/{UploadsRelative}/{fileName}";
//            return Created(url, new { url });
//        }

//        [HttpDelete("assets/{fileName}")]
//        public IActionResult DeleteAsset(string fileName)
//        {
//            if (string.IsNullOrWhiteSpace(fileName)) return BadRequest();

//            var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
//            var filePath = Path.Combine(webRoot, UploadsRelative, Path.GetFileName(fileName));
//            if (!System.IO.File.Exists(filePath)) return NotFound();

//            System.IO.File.Delete(filePath);
//            return NoContent();
//        }
//    }
//}