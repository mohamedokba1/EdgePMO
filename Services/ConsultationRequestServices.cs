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
    public class ConsultationRequestServices : IConsultationRequestServices
    {
        private readonly EdgepmoDbContext _context;
        private readonly IMapper _mapper;

        public ConsultationRequestServices(EdgepmoDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Response> CreateAsync(ConsultationRequestCreateDto dto)
        {
            Response response = new Response();
            ConsultationRequest? entity = new ConsultationRequest
            {
                Id = Guid.NewGuid(),
                Name = dto.Name.Trim(),
                Email = dto.Email?.Trim(),
                Phone = dto.Phone?.Trim(),
                Message = dto.Message?.Trim(),
                Company = dto.Company?.Trim(),
                IsConsultant = dto.IsConsultant is not null ? dto.IsConsultant.Value : false,
                CreatedAt = DateTime.UtcNow
            };
            _context.Set<ConsultationRequest>().Add(entity);
            await _context.SaveChangesAsync();

            response.IsSuccess = true;
            response.Message = "Consultation request created.";
            response.Code = HttpStatusCode.Created;
            response.Result.Add("id", JsonValue.Create(entity.Id.ToString()));
            return response;
        }

        public async Task<Response> DeleteAsync(Guid id)
        {
            Response response = new Response();
            ConsultationRequest? entity = await _context.Set<ConsultationRequest>().FindAsync(id);

            if (entity == null)
            {
                response.IsSuccess = false;
                response.Message = "Consultation request not found.";
                response.Code = HttpStatusCode.BadRequest;
                return response;
            }
            _context.Set<ConsultationRequest>().Remove(entity);
            await _context.SaveChangesAsync();

            response.IsSuccess = true;
            response.Message = "Consultation request deleted.";
            response.Code = HttpStatusCode.NoContent;
            return response;
        }

        public async Task<Response> GetAllAsync()
        {
            Response response = new Response();
            List<ConsultationRequest>? list = await _context.Set<ConsultationRequest>()
                                                            .AsNoTracking()
                                                            .OrderByDescending(x => x.CreatedAt)
                                                            .ToListAsync();

            List<ConsultationRequestReadDto>? dtoList = list.Select(x => _mapper.Map<ConsultationRequestReadDto>(x)).ToList();

            response.IsSuccess = true;
            response.Message = "Consultation requests retrieved.";
            response.Code = HttpStatusCode.OK;
            response.Result.Add("requests", JsonSerializer.SerializeToNode(dtoList) ?? JsonValue.Create(Array.Empty<object>()));
            return response;
        }

        public async Task<Response> GetByIdAsync(Guid id)
        {
            Response response = new Response();
            ConsultationRequest? entity = await _context.Set<ConsultationRequest>()
                                                        .AsNoTracking()
                                                        .FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
            {
                response.IsSuccess = false;
                response.Message = "Consultation request not found.";
                response.Code = HttpStatusCode.BadRequest;
                return response;
            }
            ConsultationRequestReadDto? dto = _mapper.Map<ConsultationRequestReadDto>(entity);

            response.IsSuccess = true;
            response.Message = "Consultation request retrieved.";
            response.Code = HttpStatusCode.OK;
            response.Result.Add("request", JsonSerializer.SerializeToNode(dto) ?? JsonValue.Create(new { }));
            return response;
        }

        public async Task<Response> UpdateAsync(Guid id, ConsultationRequestUpdateDto dto)
        {
            Response response = new Response();
            ConsultationRequest? entity = await _context.Set<ConsultationRequest>().FindAsync(id);
            if (entity == null)
            {
                response.IsSuccess = false;
                response.Message = "Consultation request not found.";
                response.Code = HttpStatusCode.NotFound;
                return response;
            }

            if (!string.IsNullOrWhiteSpace(dto.Name))
                entity.Name = dto.Name.Trim();
            if (!string.IsNullOrWhiteSpace(dto.Email))
                entity.Email = dto.Email.Trim();
            if (!string.IsNullOrWhiteSpace(dto.Phone))
                entity.Phone = dto.Phone.Trim();
            if (!string.IsNullOrWhiteSpace(dto.Message))
                entity.Message = dto.Message.Trim();
            if (!string.IsNullOrWhiteSpace(dto.Company))
                entity.Company = dto.Company.Trim();
            if (dto.IsConsultant.HasValue)
                entity.IsConsultant = dto.IsConsultant.Value;

            await _context.SaveChangesAsync();

            response.IsSuccess = true;
            response.Message = "Consultation request updated.";
            response.Code = HttpStatusCode.OK;
            return response;
        }
    }
}