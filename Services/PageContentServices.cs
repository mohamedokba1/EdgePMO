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
    public class PageContentServices : IPageContentServices
    {
        private readonly EdgepmoDbContext _context;
        private readonly IMapper _mapper;

        public PageContentServices(EdgepmoDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Response> GetAllAsync()
        {
            Response response = new Response();

            List<PageContent>? pages = await _context.Set<PageContent>()
                                            .AsNoTracking()
                                            .OrderBy(p => p.Name)
                                            .ToListAsync();

            response.IsSuccess = true;
            response.Message = "Pages retrieved.";
            response.Code = HttpStatusCode.OK;
            response.Result.Add("pages", JsonSerializer.SerializeToNode(_mapper.Map<IEnumerable<PageContentReadDto>>(pages)) ?? JsonValue.Create(Array.Empty<object>()));
            return response;
        }

        public async Task<Response> GetByIdAsync(Guid id)
        {
            Response response = new Response();

            PageContent? page = await _context.Set<PageContent>()
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync(p => p.Id == id);

            if (page == null)
            {
                response.IsSuccess = false;
                response.Message = "Page not found.";
                response.Code = HttpStatusCode.NotFound;
                return response;
            }

            response.IsSuccess = true;
            response.Message = "Page retrieved.";
            response.Code = HttpStatusCode.OK;
            response.Result.Add("page", JsonSerializer.SerializeToNode(_mapper.Map<PageContentReadDto>(page)) ?? JsonValue.Create(new { }));
            return response;
        }

        public async Task<Response> GetBySlugAsync(string slug)
        {
            Response response = new Response();

            if (string.IsNullOrWhiteSpace(slug))
            {
                response.IsSuccess = false;
                response.Message = "Slug is required.";
                response.Code = HttpStatusCode.BadRequest;
                return response;
            }

            PageContent? page = await _context.Set<PageContent>()
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(p => p.Slug.Equals(slug.ToLower()) && p.IsActive);

            if (page == null)
            {
                response.IsSuccess = false;
                response.Message = "Page not found.";
                response.Code = HttpStatusCode.NotFound;
                return response;
            }

            response.IsSuccess = true;
            response.Message = "Page retrieved.";
            response.Code = HttpStatusCode.OK;
            response.Result.Add("page", JsonSerializer.SerializeToNode(_mapper.Map<PageContentReadDto>(page)) ?? JsonValue.Create(new { }));
            return response;
        }

        public async Task<Response> CreateAsync(PageContentCreateDto dto)
        {
            Response response = new Response();

            string slug = dto.Slug.Trim().ToLowerInvariant();

            bool exists = await _context.Set<PageContent>().AnyAsync(p => p.Slug == slug);
            if (exists)
            {
                response.IsSuccess = false;
                response.Message = "A page with the same slug already exists.";
                response.Code = HttpStatusCode.BadRequest;
                return response;
            }

            var page = new PageContent
            {
                Id = Guid.NewGuid(),
                Slug = slug,
                Name = dto.Name.Trim(),
                Data = dto.Data.ToString(),
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Set<PageContent>().Add(page);
            await _context.SaveChangesAsync();

            response.IsSuccess = true;
            response.Message = "Page created.";
            response.Code = HttpStatusCode.Created;
            response.Result.Add("pageId", JsonValue.Create(page.Id.ToString()));
            return response;
        }

        public async Task<Response> UpdateAsync(PageContentUpdateDto dto)
        {
            Response response = new Response();

            var existing = await _context.Set<PageContent>().FindAsync(dto.Id);
            if (existing == null)
            {
                response.IsSuccess = false;
                response.Message = "Page not found.";
                response.Code = HttpStatusCode.BadRequest;
                return response;
            }

            if (!string.IsNullOrWhiteSpace(dto.Slug))
            {
                string newSlug = dto.Slug!.Trim().ToLowerInvariant();
                if (!newSlug.Equals(existing.Slug, StringComparison.OrdinalIgnoreCase))
                {
                    bool slugTaken = await _context.Set<PageContent>().AnyAsync(p => p.Slug == newSlug && p.Id != existing.Id);
                    if (slugTaken)
                    {
                        response.IsSuccess = false;
                        response.Message = "Slug already in use by another page.";
                        response.Code = HttpStatusCode.BadRequest;
                        return response;
                    }
                    existing.Slug = newSlug;
                }
            }

            if (!string.IsNullOrWhiteSpace(dto.Name))
                existing.Name = dto.Name!.Trim();

            if (dto.Data is not null)
                existing.Data = dto.Data.ToString();

            if (dto.IsActive.HasValue)
                existing.IsActive = dto.IsActive.Value;

            existing.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            response.IsSuccess = true;
            response.Message = "Page updated.";
            response.Code = HttpStatusCode.OK;
            return response;
        }

        public async Task<Response> DeleteAsync(Guid id)
        {
            Response response = new Response();

            PageContent? existing = await _context.Set<PageContent>().FindAsync(id);
            if (existing == null)
            {
                response.IsSuccess = false;
                response.Message = "Page not found.";
                response.Code = HttpStatusCode.BadRequest;
                return response;
            }

            existing.IsActive = false;
            existing.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            response.IsSuccess = true;
            response.Message = "Page deactivated";
            response.Code = HttpStatusCode.OK;
            return response;
        }
    }
}
