using AutoMapper;
using EdgePMO.API.Contracts;
using EdgePMO.API.Dtos;
using EdgePMO.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace EdgePMO.API.Services
{
    public class CoursesServices : ICourseServices
    {
        private readonly EdgepmoDbContext _context;
        private readonly IContentServices _contentServices;
        private readonly IMapper _mapper;

        public CoursesServices(EdgepmoDbContext context, IContentServices contentServices, IMapper mapper)
        {
            _context = context;
            _contentServices = contentServices;
            _mapper = mapper;
        }

        public async Task<Response> GetAllAsync()
        {
            Response response = new Response();

            List<Course>? courses = await _context.Courses
                .AsNoTracking()
                .Include(c => c.Instructor)
                .Include(c => c.Testimonials)
                .Include(c => c.Certificates)
                .Include(c => c.CourseVideos)
                .Include(c => c.CourseUsers)
                    .ThenInclude(cu => cu.User)
                .ToListAsync();

            response.IsSuccess = true;
            response.Message = "Courses retrieved successfully.";
            response.Code = HttpStatusCode.OK;
            response.Result.Add("courses", JsonSerializer.SerializeToNode(_mapper.Map<IEnumerable<CourseReadDto>>(courses)) ?? JsonValue.Create(Array.Empty<object>()));
            return response;
        }

        public async Task<Response> GetByIdAsync(Guid id)
        {
            Response response = new Response();

            Course? course = await _context.Courses
                .AsNoTracking()
                .Include(c => c.Instructor)
                .Include(c => c.Testimonials)
                .Include(c => c.Certificates)
                .Include(c => c.CourseVideos)
                .Include(c => c.CourseUsers)
                    .ThenInclude(cu => cu.User)
                .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null)
            {
                response.IsSuccess = false;
                response.Message = "Course not found.";
                response.Code = HttpStatusCode.NotFound;
                return response;
            }

            CourseReadDto? dto = _mapper.Map<CourseReadDto>(course);

            response.IsSuccess = true;
            response.Message = "Course retrieved successfully.";
            response.Code = HttpStatusCode.OK;
            response.Result.Add("course", JsonSerializer.SerializeToNode(dto) ?? JsonValue.Create(new { }));
            return response;
        }

        public async Task<Response> CreateAsync(CourseCreateDto dto)
        {
            Response response = new Response();

            bool instructorExists = await _context.Instructors.AnyAsync(i => i.InstructorId == dto.InstructorId);
            if (!instructorExists)
            {
                response.IsSuccess = false;
                response.Message = "Instructor not found.";
                response.Code = HttpStatusCode.BadRequest;
                return response;
            }
            Course? course = new Course
            {
                CourseId = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                CoursePictureUrl = dto.CoursePictureUrl,
                Overview = dto.Overview,
                WhatStudentsLearn = dto.WhatStudentsLearn,
                SessionsBreakdown = dto.SessionsBreakdown,
                InstructorId = dto.InstructorId,
                Price = dto.Price
            };
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            Response? courseResponse = await GetByIdAsync(course.CourseId);
            response.IsSuccess = true;
            response.Message = "Course created successfully.";
            response.Code = HttpStatusCode.Created;
            response.Result = courseResponse.Result;
            return response;
        }

        public async Task<Response> UpdateAsync(CourseUpdateDto dto)
        {
            Response response = new Response();
            Course? existing = await _context.Courses.FindAsync(dto.CourseId);
            if (existing == null)
            {
                response.IsSuccess = false;
                response.Message = "Course not found.";
                response.Code = HttpStatusCode.NotFound;
                return response;
            }

            if (dto.InstructorId != existing.InstructorId)
            {
                bool instructorExists = await _context.Instructors.AnyAsync(i => i.InstructorId == dto.InstructorId);
                if (!instructorExists)
                {
                    response.IsSuccess = false;
                    response.Message = "Instructor not found.";
                    response.Code = HttpStatusCode.BadRequest;
                    return response;
                }
            }

            _context.Entry(existing).CurrentValues.SetValues(dto);
            await _context.SaveChangesAsync();

            Response updatedCourse = await GetByIdAsync(existing.CourseId);
            response.IsSuccess = true;
            response.Message = "Course updated successfully.";
            response.Code = HttpStatusCode.OK;
            response.Result = updatedCourse.Result;
            return response;
        }

        public async Task<Response> DeleteAsync(Guid id)
        {
            Response response = new Response();
            Course? existing = await _context.Courses.FindAsync(id);
            if (existing == null)
            {
                response.IsSuccess = false;
                response.Message = "Course not found.";
                response.Code = HttpStatusCode.NotFound;
                return response;
            }

            _context.Courses.Remove(existing);
            await _context.SaveChangesAsync();

            response.IsSuccess = true;
            response.Message = "Course deleted successfully.";
            response.Code = HttpStatusCode.NoContent;
            return response;
        }

        public async Task<Response> AttachCourseVideoAsync(CourseVideoCreateDto dto)
        {
            Response response = new Response();

            Course? course = await _context.Courses.FindAsync(dto.CourseId);
            if (course == null)
            {
                response.IsSuccess = false;
                response.Message = "Course not found.";
                response.Code = HttpStatusCode.BadRequest;
                return response;
            }

            string fileName = Path.GetFileName(dto.VideoUrl ?? string.Empty);
            if (string.IsNullOrWhiteSpace(fileName))
            {
                response.IsSuccess = false;
                response.Message = "Invalid file name or URL.";
                response.Code = HttpStatusCode.BadRequest;
                return response;
            }

            string ext = Path.GetExtension(fileName).ToLowerInvariant();
            if (ext != ".mp4")
            {
                response.IsSuccess = false;
                response.Message = "Only .mp4 files are allowed.";
                response.Code = HttpStatusCode.BadRequest;
                return response;
            }

            Response listResp = await _contentServices.ListAssetsAsync();
            if (!listResp.IsSuccess)
            {
                response.IsSuccess = false;
                response.Message = "Unable to verify uploaded assets.";
                response.Code = HttpStatusCode.InternalServerError;
                return response;
            }

            string? matchedRelative = null;
            if (listResp.Result.TryGetPropertyValue("files", out JsonNode? filesNode) && filesNode is JsonArray filesArray)
            {
                foreach (JsonNode? n in filesArray)
                {
                    string? entry = (n as JsonValue)?.GetValue<string>();
                    if (string.IsNullOrWhiteSpace(entry))
                        continue;

                    if (Path.GetFileName(entry).Equals(fileName, StringComparison.OrdinalIgnoreCase))
                    {
                        matchedRelative = entry.Replace("\\", "/");
                        break;
                    }
                }
            }

            if (matchedRelative == null)
            {
                response.IsSuccess = false;
                response.Message = "Uploaded file not found in assets.";
                response.Code = HttpStatusCode.BadRequest;
                return response;
            }

            CourseVideo video = new CourseVideo
            {
                CourseId = dto.CourseId,
                Title = dto.Title?.Trim(),
                Description = dto.Description?.Trim(),
                VideoUrl = matchedRelative,
                DurationSeconds = dto.DurationSeconds,
                Order = dto.Order
            };

            _context.CourseVideos.Add(video);
            await _context.SaveChangesAsync();

            Response? courseResponse = await GetByIdAsync(dto.CourseId);

            response.IsSuccess = true;
            response.Message = "Uploaded video attached to course.";
            response.Code = HttpStatusCode.Created;
            response.Result = courseResponse.Result;
            return response;
        }

        public async Task<Response> GetEnrolledUsersAsync(Guid courseId)
        {
            Response response = new Response();

            Course? course = await _context.Courses
                .AsNoTracking()
                .Include(c => c.CourseUsers)
                    .ThenInclude(cu => cu.User)
                .FirstOrDefaultAsync(c => c.CourseId == courseId);

            if (course == null)
            {
                response.IsSuccess = false;
                response.Message = "Course not found.";
                response.Code = HttpStatusCode.NotFound;
                return response;
            }

            var users = course.CourseUsers
                .OrderByDescending(cu => cu.EnrolledAt)
                .Select(cu => new
                {
                    id = cu.User.Id,
                    userName = $"{cu.User.FirstName} {cu.User.LastName}",
                    email = cu.User.Email,
                    isActive = cu.User.IsActive ?? true,
                    enrolledAt = cu.EnrolledAt,
                    progress = cu.Progress
                })
                .ToArray();

            response.IsSuccess = true;
            response.Message = "Enrolled users retrieved.";
            response.Code = HttpStatusCode.OK;
            response.Result.Add("users", JsonSerializer.SerializeToNode(users) ?? JsonValue.Create(Array.Empty<object>()));
            return response;
        }

        public async Task<Response> EnrollUsersByEmailsAsync(Guid courseId, IEnumerable<string> emails)
        {
            Response response = new Response();

            Course? course = await _context.Courses.FindAsync(courseId);
            if (course == null)
            {
                response.IsSuccess = false;
                response.Message = "Course not found.";
                response.Code = HttpStatusCode.NotFound;
                return response;
            }

            List<string>? normalizedEmails = emails
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Select(e => e.Trim().ToLowerInvariant())
                .Distinct()
                .ToList();

            if (normalizedEmails.Count == 0)
            {
                response.IsSuccess = false;
                response.Message = "No valid emails provided.";
                response.Code = HttpStatusCode.BadRequest;
                return response;
            }

            List<User>? users = await _context.Users
                                              .Where(u => normalizedEmails.Contains(u.Email.ToLower()))
                                              .ToListAsync();

            HashSet<string>? foundEmails = users.Select(u => u.Email.Trim().ToLowerInvariant()).ToHashSet();

            List<string>? notFound = normalizedEmails.Except(foundEmails).ToList();

            List<object>? enrolled = new List<object>();
            List<string>? alreadyEnrolled = new List<string>();

            foreach (User? user in users)
            {
                bool exists = await _context.CourseUsers.AnyAsync(cu => cu.CourseId == courseId && cu.UserId == user.Id);
                if (exists)
                {
                    alreadyEnrolled.Add(user.Email);
                    continue;
                }

                CourseUser? cu = new CourseUser
                {
                    CourseId = courseId,
                    UserId = user.Id,
                    EnrolledAt = DateTime.UtcNow,
                    Progress = 0.0
                };

                _context.CourseUsers.Add(cu);
                enrolled.Add(new { userId = user.Id, email = user.Email });
            }

            await _context.SaveChangesAsync();

            response.IsSuccess = true;
            response.Message = "Enrollment processed.";
            response.Code = HttpStatusCode.OK;
            response.Result.Add("enrolled", JsonSerializer.SerializeToNode(enrolled) ?? JsonValue.Create(Array.Empty<object>()));
            response.Result.Add("alreadyEnrolled", JsonSerializer.SerializeToNode(alreadyEnrolled) ?? JsonValue.Create(Array.Empty<object>()));
            response.Result.Add("notFound", JsonSerializer.SerializeToNode(notFound) ?? JsonValue.Create(Array.Empty<object>()));
            return response;
        }

        public async Task<Response> UnenrollUsersByEmailsAsync(Guid courseId, IEnumerable<string> emails)
        {
            Response response = new Response();

            Course? course = await _context.Courses.FindAsync(courseId);
            if (course == null)
            {
                response.IsSuccess = false;
                response.Message = "Course not found.";
                response.Code = HttpStatusCode.NotFound;
                return response;
            }

            List<string>? normalizedEmails = emails
                                            .Where(e => !string.IsNullOrWhiteSpace(e))
                                            .Select(e => e.Trim().ToLowerInvariant())
                                            .Distinct()
                                            .ToList();

            if (normalizedEmails.Count == 0)
            {
                response.IsSuccess = false;
                response.Message = "No valid emails provided.";
                response.Code = HttpStatusCode.BadRequest;
                return response;
            }

            List<User>? users = await _context.Users
                .Where(u => normalizedEmails.Contains(u.Email.ToLower()))
                .ToListAsync();

            HashSet<string>? foundEmails = users.Select(u => u.Email.Trim().ToLowerInvariant()).ToHashSet();

            List<string>? notFound = normalizedEmails.Except(foundEmails).ToList();

            List<object>? unenrolled = new List<object>();
            List<string>? notEnrolled = new List<string>();

            List<Guid>? userIds = users.Select(u => u.Id).ToList();
            List<CourseUser>? enrollments = await _context.CourseUsers
                .Where(cu => cu.CourseId == courseId && userIds.Contains(cu.UserId))
                .ToListAsync();

            HashSet<Guid>? enrolledUserIds = enrollments.Select(e => e.UserId).ToHashSet();

            foreach (User user in users)
            {
                if (enrolledUserIds.Contains(user.Id))
                {
                    CourseUser? ent = enrollments.First(e => e.UserId == user.Id && e.CourseId == courseId);
                    _context.CourseUsers.Remove(ent);
                    unenrolled.Add(new { userId = user.Id, email = user.Email });
                }
                else
                {
                    notEnrolled.Add(user.Email);
                }
            }

            await _context.SaveChangesAsync();

            response.IsSuccess = true;
            response.Message = "Unenrollment processed.";
            response.Code = HttpStatusCode.OK;
            response.Result.Add("unenrolled", JsonSerializer.SerializeToNode(unenrolled) ?? JsonValue.Create(Array.Empty<object>()));
            response.Result.Add("notEnrolled", JsonSerializer.SerializeToNode(notEnrolled) ?? JsonValue.Create(Array.Empty<object>()));
            response.Result.Add("notFound", JsonSerializer.SerializeToNode(notFound) ?? JsonValue.Create(Array.Empty<object>()));
            return response;
        }

        public async Task<Response> IsUsersEnrolledAsync(Guid courseId, IEnumerable<string> emails)
        {
            Response response = new Response();

            if (emails == null)
            {
                response.IsSuccess = false;
                response.Message = "Emails list is required.";
                response.Code = HttpStatusCode.BadRequest;
                return response;
            }

            List<string>? normalizedEmails = emails
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Select(e => e.Trim().ToLowerInvariant())
                .Distinct()
                .ToList();

            if (normalizedEmails.Count == 0)
            {
                response.IsSuccess = false;
                response.Message = "No valid emails provided.";
                response.Code = HttpStatusCode.BadRequest;
                return response;
            }

            bool courseExists = await _context.Courses.AnyAsync(c => c.CourseId == courseId);
            if (!courseExists)
            {
                response.IsSuccess = false;
                response.Message = "Course not found.";
                response.Code = HttpStatusCode.NotFound;
                return response;
            }

            var users = await _context.Users
                .AsNoTracking()
                .Where(u => normalizedEmails.Contains(u.Email.ToLower()))
                .Select(u => new { u.Id, u.Email })
                .ToListAsync();

            HashSet<string>? foundEmails = users.Select(u => u.Email.Trim().ToLowerInvariant()).ToHashSet();
            List<string>? notFound = normalizedEmails.Except(foundEmails).ToList();
            Dictionary<Guid, string>? userIdMap = users.ToDictionary(u => u.Id, u => u.Email.Trim());
            List<Guid>? userIds = users.Select(u => u.Id).ToList();

            List<Guid>? enrollments = await _context.CourseUsers
                .AsNoTracking()
                .Where(cu => cu.CourseId == courseId && userIds.Contains(cu.UserId))
                .Select(cu => cu.UserId)
                .ToListAsync();

            HashSet<Guid>? enrolledUserIds = new HashSet<Guid>(enrollments);


            var results = users.Select(u => new
            {
                email = u.Email,
                enrolled = enrolledUserIds.Contains(u.Id),
                userId = u.Id
            })
            .ToList();

            response.IsSuccess = true;
            response.Message = "Enrollment check completed.";
            response.Code = HttpStatusCode.OK;
            response.Result.Add("results", JsonSerializer.SerializeToNode(results) ?? JsonValue.Create(Array.Empty<object>()));
            response.Result.Add("notFound", JsonSerializer.SerializeToNode(notFound) ?? JsonValue.Create(Array.Empty<object>()));
            return response;
        }

        public async Task<Response> DeleteCourseVideoAsync(Guid courseVideoId)
        {
            Response response = new Response();

            CourseVideo? existing = await _context.CourseVideos.FindAsync(courseVideoId);
            if (existing == null)
            {
                response.IsSuccess = false;
                response.Message = "Video not found.";
                response.Code = HttpStatusCode.BadRequest;
                return response;
            }

            string? videoUrl = existing.VideoUrl;

            _context.CourseVideos.Remove(existing);
            await _context.SaveChangesAsync();

            response.IsSuccess = true;
            response.Message = "Course video deleted.";
            response.Code = HttpStatusCode.NoContent;
            return response;
        }
    }
}
