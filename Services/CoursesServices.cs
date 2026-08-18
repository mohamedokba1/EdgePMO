using AutoMapper;
using EdgePMO.API.Contracts;
using EdgePMO.API.Dtos;
using EdgePMO.API.Dtos.Courses;
using EdgePMO.API.Models;
using EdgePMO.API.Settings;
using Microsoft.EntityFrameworkCore;
using System;
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

        /// <summary>
        /// Requirement 4.2 — the course's overall Duration field is auto-derived from
        /// the sum of every attached video's DurationMinutes, so it's never manually
        /// re-typed by an admin and never drifts from the actual session content.
        /// </summary>
        private static string FormatCourseDuration(double totalMinutes)
        {
            if (totalMinutes <= 0) return "0 hours";

            double hours = totalMinutes / 60.0;
            string hoursText = hours % 1 == 0
                ? ((int)hours).ToString()
                : Math.Round(hours, 1).ToString();

            return $"{hoursText} hours";
        }

        private async Task RecalculateCourseDurationAsync(Guid courseId)
        {
            double totalMinutes = await _context.CourseVideos
                .Where(v => v.CourseOutline.CourseId == courseId)
                .SumAsync(v => (double?)v.DurationMinutes) ?? 0;

            Course? course = await _context.Courses.FindAsync(courseId);
            if (course == null) return;

            course.Duration = FormatCourseDuration(totalMinutes);
            await _context.SaveChangesAsync();
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
            await RecalculateCourseDurationAsync(dto.CourseId);

            Response? courseResponse = await GetByIdAsync(dto.CourseId, currentUserId: null, isAdmin: true);

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

            // New courses go to the back of the display order (requirement 4.3).
            int nextSortOrder = await _context.Courses.AnyAsync()
                ? await _context.Courses.MaxAsync(c => c.SortOrder) + 1
                : 0;

            // Requirement 4.2 — Duration is derived from the sessions being created,
            // not taken from dto.Duration.
            double totalMinutes = dto.Content.Sum(c => c.Videos?.Sum(v => v.DurationMinutes) ?? 0);

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
                Duration = FormatCourseDuration(totalMinutes),
                InstructorId = dto.InstructorId,
                Price = dto.Price,
                IsActive = true,
                UpdatedAt = DateTime.UtcNow,
                SortOrder = nextSortOrder,
                IsPublic = dto?.IsPublic ?? true,
                ShowStudentsCount = dto?.ShowStudentsCount ?? true,
                OriginalPrice = dto?.OriginalPrice > 0 ? dto.OriginalPrice : null,
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

            Response? courseResponse = await GetByIdAsync(course.CourseId, currentUserId: null, isAdmin: true);
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

            CourseVideo? existing = await _context.CourseVideos
                .Include(v => v.CourseOutline)
                .FirstOrDefaultAsync(v => v.Id == courseVideoId);
            if (existing == null)
            {
                response.IsSuccess = false;
                response.Message = "Video not found.";
                response.Code = HttpStatusCode.BadRequest;
                return response;
            }

            string? videoUrl = existing.Url;
            Guid courseId = existing.CourseOutline.CourseId;

            _context.CourseVideos.Remove(existing);
            await _context.SaveChangesAsync();
            await RecalculateCourseDurationAsync(courseId);

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

        public async Task<Response> GetAllAsync(Guid? currentUserId, bool isAdmin)
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
                .OrderBy(c => c.SortOrder)
                .ToListAsync();

            // Requirement 4.4 — admins see everything; everyone else only sees IsPublic
            // courses, plus any hidden course they've actually purchased.
            if (!isAdmin)
            {
                courses = courses
                    .Where(c => c.IsPublic || (currentUserId.HasValue && c.CourseUsers.Any(cu => cu.UserId == currentUserId.Value)))
                    .ToList();
            }

            response.IsSuccess = true;
            response.Message = "Courses retrieved successfully.";
            response.Code = HttpStatusCode.OK;
            response.Result.Add("courses", JsonSerializer.SerializeToNode(_mapper.Map<IEnumerable<CourseReadDto>>(courses)) ?? JsonValue.Create(Array.Empty<object>()));
            return response;
        }

        public async Task<Response> GetByIdAsync(Guid id, Guid? currentUserId, bool isAdmin)
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

            // Requirement 4.4 — a hidden course is not reachable by direct URL either,
            // unless the requester purchased it or is an admin. Responds identically to
            // "not found" so a hidden course's existence isn't leaked to a random visitor.
            bool hasAccess = isAdmin
                || course.IsPublic
                || (currentUserId.HasValue && course.CourseUsers.Any(cu => cu.UserId == currentUserId.Value));

            if (!hasAccess)
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
            // Duration (requirement 4.2) is derived below from the actual session
            // videos whenever Content is part of this update — dto.Duration is no
            // longer taken at face value so it can't drift from the real content.
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
            if (dto.IsPublic.HasValue) course.IsPublic = dto.IsPublic.Value;
            if (dto.ShowStudentsCount.HasValue) course.ShowStudentsCount = dto.ShowStudentsCount.Value;
            // OriginalPrice: 0 is the "clear the discount" sentinel — a real original
            // price of 0 makes no sense, and PATCH DTOs can't otherwise distinguish
            // "not provided" from "explicitly null" once deserialized.
            if (dto.OriginalPrice.HasValue) course.OriginalPrice = dto.OriginalPrice.Value > 0 ? dto.OriginalPrice.Value : null;

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
                                        dbVideo.DurationMinutes = vDto.DurationMinutes ?? dbVideo.DurationMinutes;
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
                                        DurationMinutes = vDto.DurationMinutes ?? 0,
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
                            foreach (CourseVideoUpdateDto v in outlineDto.Videos)
                            {
                                string? path = v.Url.HasValue ? await GetFilePathAsync(v.Url.Value) : null;
                                if (path == null) continue;

                                CourseVideo? newVideo = new CourseVideo
                                {
                                    Id = Guid.NewGuid(),
                                    CourseOutlineId = newOutlineId,
                                    Title = v.Title,
                                    Description = v.Description,
                                    Url = path,
                                    Order = v.Order ?? 1,
                                    DurationMinutes = v.DurationMinutes ?? 0,
                                    CreatedAt = DateTime.UtcNow
                                };
                                _context.CourseVideos.Add(newVideo);
                            }
                        }

                        if (outlineDto.Documents != null)
                        {
                            foreach (var d in outlineDto.Documents)
                            {
                                string? path = d.Url.HasValue ? await GetFilePathAsync(d.Url.Value) : null;
                                if (path == null) continue;

                                CourseDocument? newDoc = new CourseDocument
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

            // Requirement 4.2 — recomputed from the now-committed video set rather than
            // tracked incrementally above; the add/update/remove branching for videos,
            // outlines, and documents is intricate enough that re-querying the saved
            // state is far less error-prone than trying to keep a running total in sync
            // with every branch. A no-op when dto.Content wasn't part of this update.
            await RecalculateCourseDurationAsync(course.CourseId);

            response.IsSuccess = true;
            response.Code = HttpStatusCode.OK;
            response.Message = "Course updated successfully.";
            response.Result = (await GetByIdAsync(course.CourseId, currentUserId: null, isAdmin: true)).Result;

            return response;
        }

        /// <summary>Requirement 4.3 — persists the admin's drag-and-drop course order.</summary>
        public async Task<Response> ReorderAsync(List<Guid> orderedCourseIds)
        {
            Response response = new Response();

            List<Course> courses = await _context.Courses
                .Where(c => orderedCourseIds.Contains(c.CourseId))
                .ToListAsync();

            if (courses.Count != orderedCourseIds.Count)
            {
                response.IsSuccess = false;
                response.Message = "One or more course IDs were not found.";
                response.Code = HttpStatusCode.BadRequest;
                return response;
            }

            for (int i = 0; i < orderedCourseIds.Count; i++)
            {
                Course course = courses.First(c => c.CourseId == orderedCourseIds[i]);
                course.SortOrder = i;
            }

            await _context.SaveChangesAsync();

            response.IsSuccess = true;
            response.Message = "Course order updated.";
            response.Code = HttpStatusCode.OK;
            return response;
        }

        public Task<Response> UpdateCourseVideoAsync(CourseVideoUpdateDto dto)
        {
            throw new NotImplementedException();
        }

        public async Task<Response> UpdateUserProgressAsync(Guid userId, Guid courseId, double progress)
        {
            Response response = new Response();

            CourseUser? enrollment = await _context.CourseUsers
                                                   .FirstOrDefaultAsync(cu => cu.UserId == userId && cu.CourseId == courseId);

            if (enrollment == null)
            {
                return new Response { IsSuccess = false, Message = "Enrollment not found.", Code = HttpStatusCode.NotFound };
            }

            enrollment.Progress = progress;
            await _context.SaveChangesAsync();

            response.IsSuccess = true;
            response.Message = "Progress synced successfully.";
            response.Code = HttpStatusCode.OK;
            response.Result.Add("newProgress", enrollment.Progress);

            return response;
        }

        /// <summary>Requirement 3.5 — upserts how far a user has watched into a video.</summary>
        public async Task<Response> UpdateVideoWatchProgressAsync(Guid userId, Guid videoId, double watchedSeconds)
        {
            Response response = new Response();

            bool videoExists = await _context.CourseVideos.AnyAsync(v => v.Id == videoId);
            if (!videoExists)
            {
                return new Response { IsSuccess = false, Message = "Video not found.", Code = HttpStatusCode.NotFound };
            }

            VideoWatchProgress? existing = await _context.VideoWatchProgresses
                .FirstOrDefaultAsync(p => p.CourseVideoId == videoId && p.UserId == userId);

            if (existing == null)
            {
                existing = new VideoWatchProgress
                {
                    CourseVideoId = videoId,
                    UserId = userId,
                    WatchedSeconds = watchedSeconds,
                    ViewCount = 1,
                };
                _context.VideoWatchProgresses.Add(existing);
            }
            else
            {
                // Furthest point reached, not the latest — a rewind shouldn't lose progress.
                existing.WatchedSeconds = Math.Max(existing.WatchedSeconds, watchedSeconds);
                existing.LastWatchedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            response.IsSuccess = true;
            response.Message = "Watch progress updated.";
            response.Code = HttpStatusCode.OK;
            return response;
        }

        /// <summary>Requirement 5.2 — per-video view counts and watch-time for admins.</summary>
        public async Task<Response> GetVideoAnalyticsAsync(Guid courseId)
        {
            Response response = new Response();

            List<CourseVideo> videos = await _context.CourseVideos
                .AsNoTracking()
                .Where(v => v.CourseOutline.CourseId == courseId)
                .Include(v => v.CourseOutline)
                .ToListAsync();

            if (!videos.Any())
            {
                response.IsSuccess = true;
                response.Message = "No videos found for this course.";
                response.Code = HttpStatusCode.OK;
                response.Result.Add("videos", JsonSerializer.SerializeToNode(Array.Empty<object>()));
                return response;
            }

            List<Guid> videoIds = videos.Select(v => v.Id).ToList();
            List<VideoWatchProgress> allProgress = await _context.VideoWatchProgresses
                .AsNoTracking()
                .Where(p => videoIds.Contains(p.CourseVideoId))
                .ToListAsync();

            var analytics = videos.Select(v =>
            {
                List<VideoWatchProgress> videoProgress = allProgress.Where(p => p.CourseVideoId == v.Id).ToList();
                double durationSeconds = v.DurationMinutes * 60.0;
                double avgCompletionPercent = durationSeconds > 0 && videoProgress.Any()
                    ? Math.Round(videoProgress.Average(p => Math.Min(p.WatchedSeconds / durationSeconds, 1.0)) * 100, 1)
                    : 0;

                return new
                {
                    VideoId = v.Id,
                    Title = v.Title,
                    SessionTitle = v.CourseOutline.Title,
                    ViewCount = videoProgress.Sum(p => p.ViewCount),
                    UniqueViewers = videoProgress.Count,
                    TotalWatchedMinutes = Math.Round(videoProgress.Sum(p => p.WatchedSeconds) / 60.0, 1),
                    AvgCompletionPercent = avgCompletionPercent,
                };
            })
            .OrderByDescending(a => a.ViewCount)
            .ToList();

            response.IsSuccess = true;
            response.Message = "Video analytics retrieved.";
            response.Code = HttpStatusCode.OK;
            response.Result.Add("videos", JsonSerializer.SerializeToNode(analytics) ?? JsonValue.Create(Array.Empty<object>()));
            return response;
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