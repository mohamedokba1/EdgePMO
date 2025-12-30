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
    public class KnowledgeHubService : IKnowledgeHubService
    {
        private readonly EdgepmoDbContext _context;
        private readonly IMapper _mapper;

        public KnowledgeHubService(EdgepmoDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Response> CreateAsync(CreateKnowledgeHubDto dto)
        {
            Response response = new();
            KnowledgeHub? knowledgeHub = new KnowledgeHub
            {
                Title = dto.Title,
                Excerpt = dto.Excerpt,
                Author = dto.Author,
                PublishDate = dto.PublishDate,
                CoverImageUrl = dto.CoverImageUrl,
                DocumentUrl = dto.DocumentUrl,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            if (dto.Sections?.Any() == true)
            {
                foreach (CreateSectionDto sectionDto in dto.Sections.OrderBy(s => s.Order))
                {
                    KnowledgeHubSection? section = new KnowledgeHubSection
                    {
                        Heading = sectionDto.Heading,
                        Order = sectionDto.Order
                    };

                    if (sectionDto.Blocks?.Any() == true)
                    {
                        foreach (CreateContentBlockDto blockDto in sectionDto.Blocks.OrderBy(b => b.Order))
                        {
                            section.Blocks.Add(new ContentBlock
                            {
                                Type = blockDto.Type,
                                Content = blockDto.Content,
                                Order = blockDto.Order
                            });
                        }
                    }

                    knowledgeHub.Sections.Add(section);
                }
            }

            await _context.KnowledgeHubs.AddAsync(knowledgeHub);
            int rowsAffected = await _context.SaveChangesAsync();

            if (rowsAffected > 0)
            {
                response.IsSuccess = true;
                response.Message = "Knowledge Hub article created successfully!";
                response.Code = HttpStatusCode.Created;
            }
            else
            {
                response.IsSuccess = false;
                response.Message = "Failed to create Knowledge Hub article";
                response.Code = HttpStatusCode.BadRequest;
            }

            return response;
        }

        public async Task<Response> GetByIdAsync(Guid id)
        {
            Response response = new();

            KnowledgeHub? knowledgeHub = await _context.KnowledgeHubs
                                        .Include(k => k.Sections)
                                        .ThenInclude(s => s.Blocks)
                                        .FirstOrDefaultAsync(k => k.Id == id && k.IsActive);

            if (knowledgeHub == null)
            {
                response.IsSuccess = false;
                response.Message = "Knowledge Hub article not found";
                response.Code = HttpStatusCode.NotFound;
                return response;
            }

            response.IsSuccess = true;
            response.Result.Add("content", JsonSerializer.SerializeToNode(_mapper.Map<KnowledgeHubDto>(knowledgeHub)) ?? JsonValue.Create(new JsonObject()));
            response.Code = HttpStatusCode.OK;

            return response;
        }

        public async Task<Response> GetAllAsync(int pageNumber = 1, int pageSize = 10)
        {
            Response response = new();

            var query = _context.KnowledgeHubs
                .Where(k => k.IsActive)
                .OrderByDescending(k => k.PublishDate)
                .Include(k => k.Sections)
                .ThenInclude(s => s.Blocks);

            int totalCount = await query.CountAsync();

            List<KnowledgeHub>? articles = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            List<KnowledgeHubDto>? dtos = _mapper.Map<List<KnowledgeHubDto>>(articles);

            response.IsSuccess = true;
            response.Result.Add("pageNo", pageNumber);
            response.Result.Add("pageSize", pageSize);
            response.Result.Add("totalCount", totalCount);
            response.Result.Add("content", JsonSerializer.SerializeToNode(dtos) ?? JsonValue.Create(Array.Empty<object>));
            response.Code = HttpStatusCode.OK;
            
            return response;
        }

        public async Task<Response> UpdateAsync(Guid id, UpdateKnowledgeHubDto dto)
        {
            Response response = new();
            KnowledgeHub? knowledgeHub = await _context.KnowledgeHubs
                                                .Include(k => k.Sections)
                                                .ThenInclude(s => s.Blocks)
                                                .FirstOrDefaultAsync(k => k.Id == id);

            if (knowledgeHub == null)
            {
                response.IsSuccess = false;
                response.Message = "Knowledge Hub article not found";
                response.Code = HttpStatusCode.NotFound;
                return response;
            }

            knowledgeHub.Title = !string.IsNullOrEmpty(dto.Title) ? dto.Title : knowledgeHub.Title;
            knowledgeHub.Excerpt = !string.IsNullOrEmpty(dto.Excerpt) ? dto.Excerpt : knowledgeHub.Excerpt;
            knowledgeHub.Author = !string.IsNullOrEmpty(dto.Author) ? dto.Author : knowledgeHub.Author;
            knowledgeHub.PublishDate = dto.PublishDate.HasValue ? dto.PublishDate.Value : knowledgeHub.PublishDate;
            knowledgeHub.CoverImageUrl = !string.IsNullOrEmpty(dto.CoverImageUrl) ? dto.CoverImageUrl : knowledgeHub.CoverImageUrl;
            knowledgeHub.DocumentUrl = !string.IsNullOrEmpty(dto.DocumentUrl) ? dto.DocumentUrl : knowledgeHub.DocumentUrl;
            knowledgeHub.IsActive = dto.IsActive.HasValue ? dto.IsActive.Value : knowledgeHub.IsActive;
            knowledgeHub.UpdatedAt = DateTime.UtcNow;

            // Remove old sections
            _context.KnowledgeHubSections.RemoveRange(knowledgeHub.Sections);

            // Add new sections
            if (dto.Sections?.Any() == true)
            {
                foreach (CreateSectionDto sectionDto in dto.Sections.OrderBy(s => s.Order))
                {
                    KnowledgeHubSection? section = new KnowledgeHubSection
                    {
                        Heading = sectionDto.Heading,
                        Order = sectionDto.Order
                    };

                    if (sectionDto.Blocks?.Any() == true)
                    {
                        foreach (CreateContentBlockDto blockDto in sectionDto.Blocks.OrderBy(b => b.Order))
                        {
                            section.Blocks.Add(new ContentBlock
                            {
                                Type = blockDto.Type,
                                Content = blockDto.Content,
                                Order = blockDto.Order
                            });
                        }
                    }

                    knowledgeHub.Sections.Add(section);
                }
            }

            int rowsAffected = await _context.SaveChangesAsync();

            if (rowsAffected > 0)
            {
                response.IsSuccess = true;
                response.Message = "Knowledge Hub article updated successfully!";
                response.Code = HttpStatusCode.OK;
            }
            else
            {
                response.IsSuccess = false;
                response.Message = "Failed to update Knowledge Hub article";
                response.Code = HttpStatusCode.BadRequest;
            }

            return response;
        }

        public async Task<Response> DeleteAsync(Guid id)
        {
            Response response = new();
            KnowledgeHub? knowledgeHub = await _context.KnowledgeHubs.FirstOrDefaultAsync(k => k.Id == id);

            if (knowledgeHub == null)
            {
                response.IsSuccess = false;
                response.Message = "Knowledge Hub article not found";
                response.Code = HttpStatusCode.NotFound;
                return response;
            }

            _context.KnowledgeHubs.Remove(knowledgeHub);
            int rowsAffected = await _context.SaveChangesAsync();

            if (rowsAffected > 0)
            {
                response.IsSuccess = true;
                response.Message = "Knowledge Hub article deleted successfully!";
                response.Code = HttpStatusCode.OK;
            }
            else
            {
                response.IsSuccess = false;
                response.Message = "Failed to delete Knowledge Hub article";
                response.Code = HttpStatusCode.BadRequest;
            }
            return response;
        }
    }
}
