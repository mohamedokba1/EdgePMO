using AutoMapper;
using EdgePMO.API.Contracts;
using EdgePMO.API.Dtos;
using EdgePMO.API.Models;
using EdgePMO.API.Settings;
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

            if (dto.PublishDate.Kind == DateTimeKind.Unspecified)
            {
                dto.PublishDate = DateTime.SpecifyKind(dto.PublishDate, DateTimeKind.Utc);
            }

            MediaFile? coverImageFile = await _context.MediaFiles.FindAsync(dto.CoverImageId);
            MediaFile? documentFile = await _context.MediaFiles.FindAsync(dto.DocumentId);

            KnowledgeHub? knowledgeHub = new KnowledgeHub
            {
                Title = dto.Title,
                Excerpt = dto.Excerpt,
                Author = dto.Author,
                PublishDate = dto.PublishDate,
                CoverImageUrl = coverImageFile?.FilePath,
                DocumentUrl = documentFile?.FilePath
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
                            ContentBlock newContentBlock = new ContentBlock();
                            newContentBlock.Type = blockDto.Type;
                            newContentBlock.Order = blockDto.Order;
                            newContentBlock.Content = ContentBlockSerializer.Serialize(blockDto.Content);
                            section.Blocks.Add(newContentBlock);
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

        public async Task<Response> DeleteAsync(Guid id)
        {
            Response response = new();
            KnowledgeHub? knowledgeHub = await _context.KnowledgeHubs.FirstOrDefaultAsync(k => k.Id == id);

            if (knowledgeHub == null)
            {
                response.IsSuccess = false;
                response.Message = "Knowledge Hub article not found";
                response.Code = HttpStatusCode.BadRequest;
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
            response.Message = "All knowledge articles retrieved successfully!";
            response.Result.Add("pageNo", pageNumber);
            response.Result.Add("pageSize", pageSize);
            response.Result.Add("totalCount", totalCount);
            response.Result.Add("content", JsonSerializer.SerializeToNode(dtos) ?? JsonValue.Create(Array.Empty<object>));
            response.Code = HttpStatusCode.OK;

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
                response.Message = $"Knowledge Hub article with id = {id} not found";
                response.Code = HttpStatusCode.BadRequest;
                return response;
            }

            response.IsSuccess = true;
            response.Message = "knowledge article retrieved successfully!";
            response.Result.Add("content", JsonSerializer.SerializeToNode(_mapper.Map<KnowledgeHubDto>(knowledgeHub)) ?? JsonValue.Create(new JsonObject()));
            response.Code = HttpStatusCode.OK;

            return response;
        }

        public async Task<Response> UpdateAsync(UpdateKnowledgeHubDto dto)
        {
            Response response = new();
            KnowledgeHub? knowledgeHub = await _context.KnowledgeHubs
                                                .Include(k => k.Sections)
                                                .ThenInclude(s => s.Blocks)
                                                .FirstOrDefaultAsync(k => k.Id == dto.Id);

            if (knowledgeHub == null)
            {
                response.IsSuccess = false;
                response.Message = "Knowledge Hub article not found";
                response.Code = HttpStatusCode.NotFound;
                return response;
            }

            if (dto.PublishDate.HasValue)
            {
                dto.PublishDate = DateTime.SpecifyKind(dto.PublishDate.Value, DateTimeKind.Utc);
                knowledgeHub.PublishDate = dto.PublishDate.Value;
            }
            if (dto.CoverImageId.HasValue)
            {
                MediaFile? coverImageFile = await _context.MediaFiles.FindAsync(dto.CoverImageId.Value);
                knowledgeHub.CoverImageUrl = coverImageFile?.FilePath;
            }

            if (dto.DocumentId.HasValue)
            {
                MediaFile? documentFile = await _context.MediaFiles.FindAsync(dto.DocumentId.Value);
                knowledgeHub.DocumentUrl = documentFile?.FilePath;
            }
            knowledgeHub.Title = !string.IsNullOrEmpty(dto.Title) ? dto.Title : knowledgeHub.Title;
            knowledgeHub.Excerpt = !string.IsNullOrEmpty(dto.Excerpt) ? dto.Excerpt : knowledgeHub.Excerpt;
            knowledgeHub.Author = !string.IsNullOrEmpty(dto.Author) ? dto.Author : knowledgeHub.Author;
            knowledgeHub.IsActive = dto.IsActive.HasValue ? dto.IsActive.Value : knowledgeHub.IsActive;
            knowledgeHub.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            // Add new sections
            if (dto.Sections?.Any() == true)
            {
                await _context.KnowledgeHubSections
                    .Where(s => s.KnowledgeHubId == dto.Id)
                    .ExecuteDeleteAsync();
                _context.ChangeTracker.Clear();

                var hub = await _context.KnowledgeHubs.FindAsync(dto.Id);

                foreach (var sectionDto in dto.Sections)
                {
                    hub!.Sections.Add(new KnowledgeHubSection
                    {
                        Heading = sectionDto.Heading,
                        Order = sectionDto.Order,
                        Blocks = sectionDto.Blocks.Select(b => new ContentBlock
                        {
                            Type = b.Type,
                            Order = b.Order,
                            Content = ContentBlockSerializer.Serialize(b.Content)
                        }).ToList()
                    });
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
    }
}