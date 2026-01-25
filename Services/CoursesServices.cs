using AutoMapper;
using EdgePMO.API.Contracts;
using EdgePMO.API.Dtos;
using EdgePMO.API.Dtos.Courses;
using EdgePMO.API.Models;
using EdgePMO.API.Settings;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace EdgePMO.API.Services
{
    public class CoursesServices : ICourseServices
    {
        private readonly IContentServices _contentServices;
        private readonly EdgepmoDbContext _context;
        private readonly ICourseContentServices _courseContentServices;
        private readonly IMapper _mapper;

        public CoursesServices(EdgepmoDbContext context, IContentServices contentServices, IMapper mapper, ICourseContentServices courseContentServices)
        {
            _context = context;
            _contentServices = contentServices;
            _mapper = mapper;
            _courseContentServices = courseContentServices;
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
            MediaFile? mediaFile = await _context.MediaFiles.FindAsync(dto.Url);
            if (mediaFile == null || mediaFile.FilePath == null)
            {
                response.IsSuccess = false;
                response.Message = "Uploaded file not found.";
                response.Code = HttpStatusCode.BadRequest;
                return response;
            }

            string fileName = Path.GetFileName(mediaFile.FilePath ?? string.Empty);
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

            Response listResp = await _contentServices.ListCoursesAssetsAsync();
            if (!listResp.IsSuccess)
            {
                response.IsSuccess = false;
                response.Message = "Unable to verify uploaded assets.";
                response.Code = HttpStatusCode.InternalServerError;
                return response;
            }

            string? matchedRelative = null;
            if (listResp.Result.TryGetPropertyValue("courses", out JsonNode? filesNode) && filesNode is JsonArray filesArray)
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
                CourseOutlineId = dto.OutlineId,
                Title = dto.Title?.Trim(),
                Description = dto.Description?.Trim(),
                Url = matchedRelative,
                DurationMinutes = dto.DurationMinutes,
                Order = dto.Order,
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

            MediaFile? mediaFile = await _context.MediaFiles.Where(m => m.Id == dto.CoursePictureId).FirstOrDefaultAsync();
            if (mediaFile is null)
            {
                response.IsSuccess = false;
                response.Message = "Course picture not found.";
                response.Code = HttpStatusCode.BadRequest;
                return response;
            }

            Course? course = new Course
            {
                CourseId = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                Subtitle = dto.Subtitle,
                MainObjective = dto?.MainObjective,
                CoursePictureUrl = mediaFile.FilePath,
                Overview = dto?.Overview,
                WhatStudentsLearn = dto?.WhatStudentsLearn,
                SoftwareUsed = dto?.SoftwareUsed,
                Requirements = dto?.Requirements,
                WhoShouldAttend = dto?.WhoShouldAttend,
                Level = dto?.Level,
                Sessions = dto.Sessions,
                Category = dto?.Category,
                Certification = dto.Certification,
                Duration = dto.Duration?.ToString() ?? null,
                InstructorId = dto.InstructorId,
                Price = dto.Price,
                IsActive = true,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            foreach (CourseContentDto? contentDto in dto.Content)
            {
                Response createOutlineResponse = await _courseContentServices.CreateCourseOutline(contentDto, course.CourseId);

                if (!createOutlineResponse.IsSuccess)
                {
                    return createOutlineResponse;
                }
                var testId = createOutlineResponse.Result["outlineId"].ToString();
                Guid outlineId = testId.ToGuidOrDefault(Guid.Empty);

                if (outlineId.Equals(Guid.Empty))
                {
                    await DeleteAsync(course.CourseId);
                    response.IsSuccess = false;
                    response.Message = "Failed to create course outline.";
                    response.Code = HttpStatusCode.InternalServerError;
                    return response;
                }

                foreach (CourseVideoCreateDto courseCreateDto in contentDto.Videos)
                {
                    Response attachVideoResponse = await _courseContentServices.CreateCourseVideo(courseCreateDto, outlineId);
                    if (!attachVideoResponse.IsSuccess)
                    {
                        return attachVideoResponse;
                    }
                }

                foreach (CourseDocumentCreateDto courseDocumentDto in contentDto.Documents)
                {
                    Response attachDocumentResponse = await _courseContentServices.CreateCourseDocuemnt(courseDocumentDto, outlineId);
                    if (!attachDocumentResponse.IsSuccess)
                    {
                        return attachDocumentResponse;
                    }
                }
            }

            Response? courseResponse = await GetByIdAsync(course.CourseId);
            response.IsSuccess = true;
            response.Message = "Course created successfully.";
            response.Code = HttpStatusCode.Created;
            response.Result = courseResponse.Result;
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

            string? videoUrl = existing.Url;

            _context.CourseVideos.Remove(existing);
            await _context.SaveChangesAsync();

            response.IsSuccess = true;
            response.Message = "Course video deleted.";
            response.Code = HttpStatusCode.NoContent;
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

            if (!course.Students.HasValue)
                course.Students = 0;

            course.Students += enrolled.Count;
            await _context.SaveChangesAsync();

            response.IsSuccess = true;
            response.Message = "Enrollment processed.";
            response.Code = HttpStatusCode.OK;
            response.Result.Add("enrolled", JsonSerializer.SerializeToNode(enrolled) ?? JsonValue.Create(Array.Empty<object>()));
            response.Result.Add("alreadyEnrolled", JsonSerializer.SerializeToNode(alreadyEnrolled) ?? JsonValue.Create(Array.Empty<object>()));
            response.Result.Add("notFound", JsonSerializer.SerializeToNode(notFound) ?? JsonValue.Create(Array.Empty<object>()));
            return response;
        }

        public async Task<Response> GetAllAsync()
        {
            Response response = new Response();

            List<Course>? courses = await _context.Courses
                .AsNoTracking()
                .Include(c => c.Instructor)
                .Include(c => c.Testimonials)
                .Include(c => c.Certificates)
                .Include(c => c.Reviews)
                    .ThenInclude(cr => cr.User)
                .Include(c => c.CourseOutline)
                    .ThenInclude(co => co.Videos)
                 .Include(c => c.CourseOutline)
                    .ThenInclude(co => co.Documents)
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
                .Include(c => c.Reviews)
                    .ThenInclude(cr => cr.User)
                .Include(c => c.CourseOutline)
                    .ThenInclude(co => co.Videos)
                 .Include(c => c.CourseOutline)
                    .ThenInclude(co => co.Documents)
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

        /*
        public async Task<Response> UpdateAsync(CourseUpdateDto dto)
        {
            Response response = new Response();
            Course? existing = await _context.Courses
                .Include(c => c.Instructor)
                .Include(c => c.Testimonials)
                .Include(c => c.Certificates)
                .Include(c => c.Reviews)
                    .ThenInclude(cr => cr.User)
                .Include(c => c.CourseOutline)
                    .ThenInclude(co => co.Videos)
                 .Include(c => c.CourseOutline)
                    .ThenInclude(co => co.Documents)
                .Include(c => c.CourseUsers)
                    .ThenInclude(cu => cu.User)
                .FirstOrDefaultAsync(c => c.CourseId == dto.CourseId);

            if (existing == null)
            {
                response.IsSuccess = false;
                response.Message = "Course not found.";
                response.Code = HttpStatusCode.NotFound;
                return response;
            }

            if (!string.IsNullOrWhiteSpace(dto.Name))
                existing.Name = dto.Name;

            if (!string.IsNullOrWhiteSpace(dto.Description))
                existing.Description = dto.Description;

            if (dto.Price.HasValue)
                existing.Price = dto.Price.Value;

            if (!string.IsNullOrEmpty(dto.Duration))
                existing.Duration = dto.Duration;

            if (dto.IsActive.HasValue)
                existing.IsActive = dto.IsActive.Value;

            if (dto.CoursePictureId.HasValue)
            {
                MediaFile? mediaFile = await _context.MediaFiles.FindAsync(dto.CoursePictureId.Value);
                if (mediaFile != null)
                {
                    existing.CoursePictureUrl = mediaFile.FilePath?.Replace("\\", "/")?.Trim();
                }
            }

            if (!string.IsNullOrWhiteSpace(dto.Overview))
                existing.Overview = dto.Overview;

            if (!string.IsNullOrWhiteSpace(dto.Subtitle))
                existing.Subtitle = dto.Subtitle;

            if (!string.IsNullOrWhiteSpace(dto.MainObjective))
                existing.MainObjective = dto.MainObjective;

            if (dto.Sessions.HasValue)
                existing.Sessions = dto.Sessions.Value;

            if (!string.IsNullOrWhiteSpace(dto.Level))
                existing.Level = dto.Level;

            if (dto.Rating.HasValue)
                existing.Rating = dto.Rating.Value;

            if (dto.Students.HasValue)
                existing.Students = dto.Students.Value;

            if (dto.InstructorId.HasValue)
                existing.InstructorId = dto.InstructorId.Value;

            if (!string.IsNullOrWhiteSpace(dto.Category))
                existing.Category = dto.Category;

            if (dto.Certification.HasValue)
                existing.Certification = dto.Certification.Value;

            if (dto.SoftwareUsed != null && dto.SoftwareUsed.Count > 0)
                existing.SoftwareUsed = dto.SoftwareUsed;

            if (dto.WhatStudentsLearn != null && dto.WhatStudentsLearn.Count > 0)
                existing.WhatStudentsLearn = dto.WhatStudentsLearn;

            if (dto.WhoShouldAttend != null && dto.WhoShouldAttend.Count > 0)
                existing.WhoShouldAttend = dto.WhoShouldAttend;

            if (dto.Requirements != null && dto.Requirements.Count > 0)
                existing.Requirements = dto.Requirements;

            if (dto.Content != null)
            {
                Dictionary<Guid, CourseOutline>? dbOutlines = existing.CourseOutline.ToDictionary(o => o.Id, o => o);

                foreach (CourseContentUpdateDto outlineDto in dto.Content)
                {
                    CourseOutline? outline;
                    if (outlineDto.Id != Guid.Empty && outlineDto.Id.HasValue && dbOutlines.TryGetValue(outlineDto.Id.Value, out outline))
                    {
                        if (!string.IsNullOrEmpty(outlineDto.Title))
                            outline.Title = outlineDto.Title;

                        if (outlineDto.Order.HasValue)
                            outline.Order = outlineDto.Order.Value;

                        Dictionary<Guid, CourseVideo>? dbVideos = outline.Videos.ToDictionary(v => v.Id, v => v);
                        List<CourseVideoUpdateDto>? dtoVideos = outlineDto.Videos ?? new List<CourseVideoUpdateDto>();

                        // Update or add videos
                        foreach (CourseVideoUpdateDto videoDto in dtoVideos)
                        {
                            if (dbVideos.TryGetValue(videoDto.Id.Value, out var dbVideo))
                            {
                                if (dbVideo.Title != videoDto.Title)
                                    dbVideo.Title = videoDto.Title;

                                if (dbVideo.Description != videoDto.Description)
                                    dbVideo.Description = videoDto.Description;

                                if (videoDto.DurationMinutes.HasValue)
                                    dbVideo.DurationMinutes = videoDto.DurationMinutes.Value;

                                if (videoDto.Order.HasValue)
                                    dbVideo.Order = videoDto.Order.Value;

                                if (videoDto.Url.HasValue)
                                {
                                    var mediaFile = await _context.MediaFiles.FindAsync(videoDto.Url.Value);
                                    if (mediaFile != null)
                                        dbVideo.Url = mediaFile.FilePath?.Replace("\\", "/")?.Trim();
                                }
                            }
                            else
                            {
                                CourseVideo newCourseVideo = new CourseVideo();
                                newCourseVideo.Id = Guid.NewGuid();
                                newCourseVideo.Title = videoDto.Title;
                                newCourseVideo.Description = videoDto.Description;
                                newCourseVideo.DurationMinutes = videoDto.DurationMinutes.HasValue ? videoDto.DurationMinutes.Value : 0;
                                newCourseVideo.Order = videoDto.Order.HasValue ? videoDto.Order.Value : 1;
                                newCourseVideo.CreatedAt = DateTime.UtcNow;
                                newCourseVideo.CourseOutlineId = outline.Id;

                                MediaFile? mediaFile = await _context.MediaFiles.FindAsync(videoDto?.Url);
                                if (mediaFile != null)
                                {
                                    newCourseVideo.Url = mediaFile.FilePath?.Replace("\\", "/")?.Trim();
                                }

                                outline.Videos.Add(newCourseVideo);
                            }
                        }

                        //outline.Videos.RemoveAll(v => v.Id != null || v.Id != Guid.Empty);

                        // --- Documents ---
                        Dictionary<Guid, CourseDocument>? dbDocs = outline.Documents.ToDictionary(d => d.CourseDocumentId, d => d);
                        List<CourseDocumentUpdateDto>? dtoDocs = outlineDto.Documents ?? new List<CourseDocumentUpdateDto>();

                        foreach (CourseDocumentUpdateDto docDto in dtoDocs)
                        {
                            if (docDto is { } && docDto.Id.HasValue && dbDocs.TryGetValue(docDto.Id.Value, out CourseDocument? dbDoc))
                            {
                                MediaFile? mediaFile = await _context.MediaFiles.FindAsync(docDto.Url);
                                if (mediaFile != null)
                                {
                                    dbDoc.DocumentUrl = mediaFile.FilePath?.Replace("\\", "/")?.Trim();
                                }
                                dbDoc.Title = docDto.Title;
                                dbDoc.Description = docDto.Description;
                            }
                            else
                            {
                                CourseDocument newDoc = new CourseDocument();
                                MediaFile? mediaFile = await _context.MediaFiles.FindAsync(docDto.Url);
                                if (mediaFile != null)
                                {
                                    newDoc.DocumentUrl = mediaFile.FilePath?.Replace("\\", "/")?.Trim();
                                }
                                newDoc.Title = docDto.Title;
                                newDoc.Description = docDto.Description;
                                newDoc.CreatedAt = DateTime.UtcNow;
                                newDoc.CourseDocumentId = Guid.NewGuid();
                                newDoc.CourseOutlineId = outline.Id;
                                // Add new document
                                outline.Documents.Add(newDoc);
                            }
                        }
                        //HashSet<Guid?>? dtoDocIds = dtoDocs.Select(d => d.Id).ToHashSet();
                        //outline.Documents.RemoveAll(d => d.CourseDocumentId != Guid.Empty && !dtoDocIds.Contains(d.CourseDocumentId));
                    }
                }
            }
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            Response updatedCourse = await GetByIdAsync(existing.CourseId);
            response.IsSuccess = true;
            response.Message = "Course updated successfully.";
            response.Code = HttpStatusCode.OK;
            response.Result = updatedCourse.Result;
            return response;
        }
        */

        public async Task<Response> UpdateAsync(CourseUpdateDto dto)
        {
            Response? response = new Response();

            Course? course = await _context.Courses
                .Include(c => c.CourseOutline)
                    .ThenInclude(o => o.Videos)
                .Include(c => c.CourseOutline)
                    .ThenInclude(o => o.Documents)
                .FirstOrDefaultAsync(c => c.CourseId == dto.CourseId);

            if (course == null)
            {
                return new Response { IsSuccess = false, Code = HttpStatusCode.NotFound, Message = "Course not found." };
            }
            _context.Entry(course).State = EntityState.Modified;

            if (!string.IsNullOrWhiteSpace(dto.Name)) course.Name = dto.Name;
            if (!string.IsNullOrWhiteSpace(dto.Description)) course.Description = dto.Description;
            if (dto.Price.HasValue) course.Price = dto.Price.Value;
            if (dto.IsActive.HasValue) course.IsActive = dto.IsActive.Value;
            if (!string.IsNullOrWhiteSpace(dto.Duration)) course.Duration = dto.Duration;
            if (!string.IsNullOrWhiteSpace(dto.Overview)) course.Overview = dto.Overview;
            if (!string.IsNullOrWhiteSpace(dto.Subtitle)) course.Subtitle = dto.Subtitle;
            if (!string.IsNullOrWhiteSpace(dto.MainObjective)) course.MainObjective = dto.MainObjective;
            if (dto.Sessions.HasValue) course.Sessions = dto.Sessions.Value;
            if (!string.IsNullOrWhiteSpace(dto.Level)) course.Level = dto.Level;
            if (dto.Rating.HasValue) course.Rating = dto.Rating.Value;
            if (dto.Students.HasValue) course.Students = dto.Students.Value;
            if (dto.InstructorId.HasValue) course.InstructorId = dto.InstructorId.Value;
            if (!string.IsNullOrWhiteSpace(dto.Category)) course.Category = dto.Category;
            if (dto.Certification.HasValue) course.Certification = dto.Certification.Value;
            if (dto.SoftwareUsed != null) course.SoftwareUsed = dto.SoftwareUsed;
            if (dto.WhatStudentsLearn != null) course.WhatStudentsLearn = dto.WhatStudentsLearn;
            if (dto.WhoShouldAttend != null) course.WhoShouldAttend = dto.WhoShouldAttend;
            if (dto.Requirements != null) course.Requirements = dto.Requirements;

            course.UpdatedAt = DateTime.UtcNow;

            if (dto.Content != null)
            {
                var dtoOutlineIds = dto.Content.Where(o => o.Id.HasValue).Select(o => o.Id!.Value).ToHashSet();
                var outlinesToRemove = course.CourseOutline.Where(o => !dtoOutlineIds.Contains(o.Id)).ToList();
                foreach (var outlineToRemove in outlinesToRemove)
                {
                    _context.CourseOutlines.Remove(outlineToRemove);
                }

                foreach (var outlineDto in dto.Content)
                {
                    if (outlineDto.Id.HasValue)
                    {
                        var outline = course.CourseOutline.FirstOrDefault(o => o.Id == outlineDto.Id.Value);
                        if (outline == null) continue;

                        _context.Entry(outline).State = EntityState.Modified;
                        outline.Title = outlineDto.Title ?? outline.Title;
                        outline.Order = outlineDto.Order ?? outline.Order;

                        // --- VIDEOS ---
                        var dtoVideoIds = outlineDto.Videos?.Where(v => v.Id.HasValue).Select(v => v.Id!.Value).ToHashSet() ?? new HashSet<Guid>();
                        var videosToRemove = outline.Videos.Where(v => !dtoVideoIds.Contains(v.Id)).ToList();
                        foreach (var vRem in videosToRemove) _context.CourseVideos.Remove(vRem);

                        if (outlineDto.Videos != null)
                        {
                            foreach (var vDto in outlineDto.Videos)
                            {
                                string? videoPath = vDto.Url.HasValue ? await GetFilePathAsync(vDto.Url.Value) : null;
                                if (videoPath == null) continue;

                                if (vDto.Id.HasValue)
                                {
                                    var dbVideo = outline.Videos.FirstOrDefault(v => v.Id == vDto.Id.Value);
                                    if (dbVideo != null)
                                    {
                                        _context.Entry(dbVideo).State = EntityState.Modified;
                                        dbVideo.Title = vDto.Title;
                                        dbVideo.Description = vDto.Description;
                                        dbVideo.Order = vDto.Order ?? dbVideo.Order;
                                        dbVideo.Url = videoPath;
                                    }
                                }
                                else
                                {
                                    var newVideo = new CourseVideo
                                    {
                                        Id = Guid.NewGuid(),
                                        CourseOutlineId = outline.Id,
                                        Title = vDto.Title,
                                        Description = vDto.Description,
                                        Url = videoPath,
                                        Order = vDto.Order ?? 1,
                                        CreatedAt = DateTime.UtcNow
                                    };
                                    _context.CourseVideos.Add(newVideo);
                                }
                            }
                        }

                        // --- DOCUMENTS ---
                        var dtoDocIds = outlineDto.Documents?.Where(d => d.Id.HasValue).Select(d => d.Id!.Value).ToHashSet() ?? new HashSet<Guid>();
                        var docsToRemove = outline.Documents.Where(d => !dtoDocIds.Contains(d.CourseDocumentId)).ToList();
                        foreach (var dRem in docsToRemove) _context.CourseDocuments.Remove(dRem);

                        if (outlineDto.Documents != null)
                        {
                            foreach (var dDto in outlineDto.Documents)
                            {
                                string? docPath = dDto.Url.HasValue ? await GetFilePathAsync(dDto.Url.Value) : null;
                                if (docPath == null) continue;

                                if (dDto.Id.HasValue)
                                {
                                    var dbDoc = outline.Documents.FirstOrDefault(d => d.CourseDocumentId == dDto.Id.Value);
                                    if (dbDoc != null)
                                    {
                                        _context.Entry(dbDoc).State = EntityState.Modified;
                                        dbDoc.Title = dDto.Title;
                                        dbDoc.Description = dDto.Description;
                                        dbDoc.DocumentUrl = docPath;
                                    }
                                }
                                else
                                {
                                    CourseDocument? newDoc = new CourseDocument
                                    {
                                        CourseDocumentId = Guid.NewGuid(),
                                        CourseOutlineId = outline.Id,
                                        Title = dDto.Title,
                                        Description = dDto.Description,
                                        DocumentUrl = docPath,
                                        CreatedAt = DateTime.UtcNow
                                    };
                                    _context.CourseDocuments.Add(newDoc);
                                }
                            }
                        }
                    }
                    else
                    {
                        Guid newOutlineId = Guid.NewGuid();
                        var newOutline = new CourseOutline
                        {
                            Id = newOutlineId,
                            CourseId = course.CourseId,
                            Title = outlineDto.Title,
                            Order = outlineDto.Order ?? 1,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.CourseOutlines.Add(newOutline);

                        if (outlineDto.Videos != null)
                        {
                            foreach (var v in outlineDto.Videos)
                            {
                                var path = v.Url.HasValue ? await GetFilePathAsync(v.Url.Value) : null;
                                if (path == null) continue;

                                var newVideo = new CourseVideo
                                {
                                    Id = Guid.NewGuid(),
                                    CourseOutlineId = newOutlineId,
                                    Title = v.Title,
                                    Description = v.Description,
                                    Url = path,
                                    Order = v.Order ?? 1,
                                    CreatedAt = DateTime.UtcNow
                                };
                                _context.CourseVideos.Add(newVideo);
                            }
                        }

                        if (outlineDto.Documents != null)
                        {
                            foreach (var d in outlineDto.Documents)
                            {
                                var path = d.Url.HasValue ? await GetFilePathAsync(d.Url.Value) : null;
                                if (path == null) continue;

                                var newDoc = new CourseDocument
                                {
                                    CourseDocumentId = Guid.NewGuid(),
                                    CourseOutlineId = newOutlineId,
                                    Title = d.Title,
                                    Description = d.Description,
                                    DocumentUrl = path,
                                    CreatedAt = DateTime.UtcNow
                                };
                                _context.CourseDocuments.Add(newDoc);
                            }
                        }
                    }
                }
            }
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                _context.ChangeTracker.Clear();
                return new Response { IsSuccess = false, Code = HttpStatusCode.Conflict, Message = "Concurrency error: The record was modified by another process." };
            }
            catch (Exception ex)
            {
                return new Response { IsSuccess = false, Code = HttpStatusCode.InternalServerError, Message = ex.Message };
            }

            response.IsSuccess = true;
            response.Code = HttpStatusCode.OK;
            response.Message = "Course updated successfully.";
            response.Result = (await GetByIdAsync(course.CourseId)).Result;

            return response;
        }

        public Task<Response> UpdateCourseVideoAsync(CourseVideoUpdateDto dto)
        {
            throw new NotImplementedException();
        }

        private async Task<string?> GetFilePathAsync(Guid mediaId)
        {
            var media = await _context.MediaFiles
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == mediaId);

            return media?.FilePath?.Replace("\\", "/")?.Trim();
        }

        private string? GetFilePathFromMediaFileId(Guid? mediaFileId)
        {
            if (!mediaFileId.HasValue)
                return null;
            MediaFile? mediaFile = _context.MediaFiles.Find(mediaFileId.Value);
            return mediaFile?.FilePath?.Replace("\\", "/").Trim() ?? null;
        }
    }
}