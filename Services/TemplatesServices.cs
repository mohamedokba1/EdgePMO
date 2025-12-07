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
    public class TemplatesServices : ITemplateServices
    {
        private readonly EdgepmoDbContext _context;
        private readonly IContentServices _contentServices;
        private readonly IMapper _mapper;

        public TemplatesServices(EdgepmoDbContext context, IContentServices contentServices, IMapper mapper)
        {
            _context = context;
            _contentServices = contentServices;
            _mapper = mapper;
        }

        public async Task<Response> GetAllAsync()
        {
            Response response = new Response();
            List<Template>? list = await _context.Templates
                .AsNoTracking()
                .Include(t => t.Purchases)
                .Include(t => t.UserTemplates)
                .ToListAsync();

            response.IsSuccess = true;
            response.Message = "Templates retrieved.";
            response.Code = HttpStatusCode.OK;
            response.Result.Add("templates", JsonSerializer.SerializeToNode(list) ?? JsonValue.Create(Array.Empty<object>()));
            return response;
        }

        public async Task<Response> GetByIdAsync(Guid id)
        {
            Response response = new Response();
            Template? template = await _context.Templates
                .AsNoTracking()
                .Include(x => x.Purchases)
                .Include(x => x.UserTemplates)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (template == null)
            {
                response.IsSuccess = false;
                response.Message = "Template not found.";
                response.Code = HttpStatusCode.BadRequest;
                return response;
            }

            response.IsSuccess = true;
            response.Message = "Template retrieved.";
            response.Code = HttpStatusCode.OK;
            response.Result.Add("template", JsonSerializer.SerializeToNode(template) ?? JsonValue.Create(new { }));
            return response;
        }

        public async Task<Response> CreateAsync(TemplateCreateDto dto)
        {
            Response response = new Response();

            if (!await _contentServices.FileExistsAsync(dto.FilePath) || !await _contentServices.FileExistsAsync(dto.CoverImageUrl))
            {
                response.IsSuccess = false;
                response.Message = "File(s) does not exist.";
                response.Code = HttpStatusCode.BadRequest;
                return response;
            }

            Template? template = new Template
            {
                Id = Guid.NewGuid(),
                Name = dto.Name.Trim(),
                Description = dto.Description?.Trim(),
                Price = dto.Price,
                Category = dto.Category,
                CoverImageUrl = dto.CoverImageUrl,
                IsActive = dto.IsActive,
                FilePath = dto.FilePath,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Templates.Add(template);
            await _context.SaveChangesAsync();

            response.IsSuccess = true;
            response.Message = "Template created.";
            response.Code = HttpStatusCode.Created;
            response.Result.Add("template", JsonSerializer.SerializeToNode(template) ?? JsonValue.Create(new { }));
            return response;
        }

        public async Task<Response> UpdateAsync(TemplateUpdateDto dto)
        {
            Response response = new Response();

            Template? existing = await _context.Templates.FindAsync(dto.Id);
            if (existing == null)
            {
                response.IsSuccess = false;
                response.Message = "Template not found.";
                response.Code = HttpStatusCode.BadRequest;
                return response;
            }

            if (!string.IsNullOrWhiteSpace(dto.FilePath))
            {
                bool fileExists = await _contentServices.FileExistsAsync(dto.FilePath);
                if (!fileExists)
                {
                    response.IsSuccess = false;
                    response.Message = "FilePath does not exist.";
                    response.Code = HttpStatusCode.BadRequest;
                    return response;
                }
            }

            if (!string.IsNullOrWhiteSpace(dto.CoverImageUrl))
            {
                bool imageExists = await _contentServices.FileExistsAsync(dto.CoverImageUrl);
                if (!imageExists)
                {
                    response.IsSuccess = false;
                    response.Message = "CoverImageUrl does not exist.";
                    response.Code = HttpStatusCode.BadRequest;
                    return response;
                }
            }
            _mapper.Map(dto, existing);

            existing.UpdatedAt = DateTime.UtcNow;
            _context.Entry(existing).Property(e => e.UpdatedAt).IsModified = true;

            await _context.SaveChangesAsync();

            response.IsSuccess = true;
            response.Message = "Template updated.";
            response.Code = HttpStatusCode.OK;
            response.Result.Add("template", JsonSerializer.SerializeToNode(existing) ?? JsonValue.Create(new { }));
            return response;
        }

        public async Task<Response> DeleteAsync(Guid id)
        {
            Response response = new Response();

            Template? existing = await _context.Templates.FindAsync(id);
            if (existing == null)
            {
                response.IsSuccess = false;
                response.Message = "Template not found.";
                response.Code = HttpStatusCode.BadRequest;
                return response;
            }

            existing.IsActive = false;
            await _context.SaveChangesAsync();

            response.IsSuccess = true;
            response.Message = "Template deleted.";
            response.Code = HttpStatusCode.NoContent;
            return response;
        }

        public async Task<Response> GrantAccessByEmailsAsync(Guid templateId, IEnumerable<string> emails)
        {
            Response response = new Response();

            Template? template = await _context.Templates.FindAsync(templateId);
            if (template == null)
            {
                response.IsSuccess = false;
                response.Message = "Template not found.";
                response.Code = HttpStatusCode.NotFound;
                return response;
            }

            List<string>? normalized = emails
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Select(e => e.Trim().ToLowerInvariant())
                .Distinct()
                .ToList();

            if (normalized.Count == 0)
            {
                response.IsSuccess = false;
                response.Message = "No valid emails provided.";
                response.Code = HttpStatusCode.BadRequest;
                return response;
            }

            List<User>? users = await _context.Users
                .Where(u => normalized.Contains(u.Email.ToLower()))
                .ToListAsync();

            HashSet<string>? foundEmails = users.Select(u => u.Email.Trim().ToLowerInvariant()).ToHashSet();
            List<string>? notFound = normalized.Except(foundEmails).ToList();

            List<object>? granted = new List<object>();
            List<string>? already = new List<string>();

            foreach (User user in users)
            {
                bool has = await _context.UserTemplates.AnyAsync(ut => ut.UserId == user.Id && ut.TemplateId == templateId);
                if (has)
                {
                    already.Add(user.Email);
                    continue;
                }

                UserTemplate? ut = new UserTemplate
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    TemplateId = templateId,
                    PurchaseId = null,
                    PurchasedAt = DateTime.UtcNow
                };

                _context.UserTemplates.Add(ut);
                granted.Add(new { userId = user.Id, email = user.Email });
            }

            await _context.SaveChangesAsync();

            response.IsSuccess = true;
            response.Message = "Grant access processed.";
            response.Code = HttpStatusCode.OK;
            response.Result.Add("granted", JsonSerializer.SerializeToNode(granted) ?? JsonValue.Create(Array.Empty<object>()));
            response.Result.Add("already", JsonSerializer.SerializeToNode(already) ?? JsonValue.Create(Array.Empty<object>()));
            response.Result.Add("notFound", JsonSerializer.SerializeToNode(notFound) ?? JsonValue.Create(Array.Empty<object>()));
            return response;
        }

        public async Task<Response> RevokeAccessByEmailsAsync(Guid templateId, IEnumerable<string> emails)
        {
            Response response = new Response();

            Template? template = await _context.Templates.FindAsync(templateId);
            if (template == null)
            {
                response.IsSuccess = false;
                response.Message = "Template not found.";
                response.Code = HttpStatusCode.NotFound;
                return response;
            }

            List<string>? normalized = emails
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Select(e => e.Trim().ToLowerInvariant())
                .Distinct()
                .ToList();

            if (normalized.Count == 0)
            {
                response.IsSuccess = false;
                response.Message = "No valid emails provided.";
                response.Code = HttpStatusCode.BadRequest;
                return response;
            }

            List<User>? users = await _context.Users
                .Where(u => normalized.Contains(u.Email.ToLower()))
                .ToListAsync();

            HashSet<string>? foundEmails = users.Select(u => u.Email.Trim().ToLowerInvariant()).ToHashSet();
            List<string>? notFound = normalized.Except(foundEmails).ToList();

            List<object>? revoked = new List<object>();
            List<string>? notEnrolled = new List<string>();

            List<Guid>? userIds = users.Select(u => u.Id).ToList();
            List<UserTemplate>? entries = await _context.UserTemplates
                .Where(ut => ut.TemplateId == templateId && userIds.Contains(ut.UserId))
                .ToListAsync();

            Dictionary<Guid, UserTemplate>? entriesByUser = entries.ToDictionary(e => e.UserId, e => e);

            foreach (User user in users)
            {
                if (entriesByUser.TryGetValue(user.Id, out var ent))
                {
                    _context.UserTemplates.Remove(ent);
                    revoked.Add(new { userId = user.Id, email = user.Email });
                }
                else
                {
                    notEnrolled.Add(user.Email);
                }
            }

            await _context.SaveChangesAsync();

            response.IsSuccess = true;
            response.Message = "Revoke access processed.";
            response.Code = HttpStatusCode.OK;
            response.Result.Add("revoked", JsonSerializer.SerializeToNode(revoked) ?? JsonValue.Create(Array.Empty<object>()));
            response.Result.Add("notEnrolled", JsonSerializer.SerializeToNode(notEnrolled) ?? JsonValue.Create(Array.Empty<object>()));
            response.Result.Add("notFound", JsonSerializer.SerializeToNode(notFound) ?? JsonValue.Create(Array.Empty<object>()));
            return response;
        }
    }
}
