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

        public TemplatesServices(EdgepmoDbContext context)
        {
            _context = context;
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
            Template? t = await _context.Templates
                .AsNoTracking()
                .Include(x => x.Purchases)
                .Include(x => x.UserTemplates)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (t == null)
            {
                response.IsSuccess = false;
                response.Message = "Template not found.";
                response.Code = HttpStatusCode.BadRequest;
                return response;
            }

            response.IsSuccess = true;
            response.Message = "Template retrieved.";
            response.Code = HttpStatusCode.OK;
            response.Result.Add("template", JsonSerializer.SerializeToNode(t) ?? JsonValue.Create(new { }));
            return response;
        }

        public async Task<Response> CreateAsync(TemplateCreateDto dto)
        {
            Response response = new Response();

            Template? template = new Template
            {
                Id = Guid.NewGuid(),
                Name = dto.Name.Trim(),
                Description = dto.Description?.Trim(),
                Price = dto.Price,
                Category = dto.Category,
                ImageUrl = dto.ImageUrl,
                IsActive = dto.IsActive,
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

            existing.Name = dto.Name.Trim();
            existing.Description = dto.Description?.Trim();
            existing.Price = dto.Price;
            existing.Category = dto.Category;
            existing.ImageUrl = dto.ImageUrl;
            existing.IsActive = dto.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;

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
                response.Code = HttpStatusCode.NotFound;
                return response;
            }

            existing.IsActive = false;
            await _context.SaveChangesAsync();

            response.IsSuccess = true;
            response.Message = "Template deleted.";
            response.Code = HttpStatusCode.NoContent;
            return response;
        }
    }
}
