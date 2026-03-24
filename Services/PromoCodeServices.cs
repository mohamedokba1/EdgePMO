using AutoMapper;
using EdgePMO.API.Contracts;
using EdgePMO.API.Dtos;
using EdgePMO.API.Dtos.PromoCodes;
using EdgePMO.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace EdgePMO.API.Services
{
    public class PromoCodeServices : IPromoCodeServices
    {
        private readonly EdgepmoDbContext _context;
        private readonly IMapper _mapper;

        public PromoCodeServices(EdgepmoDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Response> GetAllAsync()
        {
            Response response = new Response();

            List<PromoCode> promos = await _context.Set<PromoCode>()
                                            .AsNoTracking()
                                            .OrderByDescending(p => p.CreatedAt)
                                            .ToListAsync();

            response.IsSuccess = true;
            response.Message = "Promo codes retrieved.";
            response.Code = HttpStatusCode.OK;
            response.Result.Add("promos", JsonSerializer.SerializeToNode(_mapper.Map<IEnumerable<PromoCodeReadDto>>(promos)) ?? JsonValue.Create(Array.Empty<object>()));
            return response;
        }

        public async Task<Response> GetByIdAsync(Guid id)
        {
            Response response = new Response();

            PromoCode? promo = await _context.Set<PromoCode>()
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync(p => p.Id == id);

            if (promo == null)
            {
                response.IsSuccess = false;
                response.Message = "Promo code not found.";
                response.Code = HttpStatusCode.NotFound;
                return response;
            }

            response.IsSuccess = true;
            response.Message = "Promo code retrieved.";
            response.Code = HttpStatusCode.OK;
            response.Result.Add("promo", JsonSerializer.SerializeToNode(_mapper.Map<PromoCodeReadDto>(promo)) ?? JsonValue.Create(new { }));
            return response;
        }

        public async Task<Response> CreateAsync(PromoCodeCreateDto dto)
        {
            Response response = new Response();

            string code = dto.Code.Trim().ToUpperInvariant();

            bool exists = await _context.Set<PromoCode>().AnyAsync(p => p.Code == code);
            if (exists)
            {
                response.IsSuccess = false;
                response.Message = "A promo code with this name already exists.";
                response.Code = HttpStatusCode.BadRequest;
                return response;
            }

            PromoCode? promo = new PromoCode
            {
                Id = Guid.NewGuid(),
                Code = code,
                DiscountValue = dto.DiscountValue,
                IsPercentage = dto.IsPercentage,
                CourseId = dto.CourseId,
                ExpiryDate = dto.ExpiryDate.ToUniversalTime(),
                MaxUsage = dto.MaxUsage,
                CurrentUsage = 0,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.Set<PromoCode>().Add(promo);
            await _context.SaveChangesAsync();

            response.IsSuccess = true;
            response.Message = "Promo code created.";
            response.Code = HttpStatusCode.Created;
            response.Result.Add("promoId", JsonValue.Create(promo.Id.ToString()));
            return response;
        }

        public async Task<Response> UpdateAsync(PromoCodeUpdateDto dto)
        {
            Response response = new Response();

            PromoCode? existing = await _context.Set<PromoCode>().FindAsync(dto.Id);
            if (existing == null)
            {
                response.IsSuccess = false;
                response.Message = "Promo code not found.";
                response.Code = HttpStatusCode.NotFound;
                return response;
            }

            if (!string.IsNullOrWhiteSpace(dto.Code))
            {
                string newCode = dto.Code.Trim().ToUpperInvariant();
                if (!newCode.Equals(existing.Code, StringComparison.OrdinalIgnoreCase))
                {
                    bool codeTaken = await _context.Set<PromoCode>().AnyAsync(p => p.Code == newCode && p.Id != existing.Id);
                    if (codeTaken)
                    {
                        response.IsSuccess = false;
                        response.Message = "Promo code string already in use.";
                        response.Code = HttpStatusCode.BadRequest;
                        return response;
                    }
                    existing.Code = newCode;
                }
            }

            if (dto.DiscountValue.HasValue)
                existing.DiscountValue = dto.DiscountValue.Value;

            if (dto.IsPercentage.HasValue)
                existing.IsPercentage = dto.IsPercentage.Value;

            if (dto.CourseId.HasValue)
                existing.CourseId = dto.CourseId;

            if (dto.ExpiryDate.HasValue)
                existing.ExpiryDate = dto.ExpiryDate.Value.ToUniversalTime();

            if (dto.MaxUsage.HasValue)
                existing.MaxUsage = dto.MaxUsage.Value;

            if (dto.IsActive.HasValue)
                existing.IsActive = dto.IsActive.Value;

            await _context.SaveChangesAsync();

            response.IsSuccess = true;
            response.Message = "Promo code updated.";
            response.Code = HttpStatusCode.OK;
            return response;
        }

        public async Task<Response> DeleteAsync(Guid id)
        {
            Response response = new Response();

            PromoCode? existing = await _context.Set<PromoCode>().FindAsync(id);
            if (existing == null)
            {
                response.IsSuccess = false;
                response.Message = "Promo code not found.";
                response.Code = HttpStatusCode.NotFound;
                return response;
            }

            // Hard delete for Promo Codes is usually preferred if they haven't been used, 
            // but following your pattern, we will deactivate it.
            existing.IsActive = false;
            await _context.SaveChangesAsync();

            response.IsSuccess = true;
            response.Message = "Promo code deactivated.";
            response.Code = HttpStatusCode.OK;
            return response;
        }

        public async Task<Response> ValidateCodeAsync(string code, Guid? courseId)
        {
            Response response = new Response();

            if (string.IsNullOrWhiteSpace(code))
            {
                response.IsSuccess = false;
                response.Message = "Promo code is required.";
                response.Code = HttpStatusCode.BadRequest;
                return response;
            }

            string normalizedCode = code.Trim().ToUpperInvariant();

            PromoCode? promo = await _context.Set<PromoCode>()
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync(p => p.Code == normalizedCode);

            if (promo == null)
            {
                response.IsSuccess = false;
                response.Message = "Invalid promo code.";
                response.Code = HttpStatusCode.NotFound;
                return response;
            }

            if (!promo.IsActive)
            {
                response.IsSuccess = false;
                response.Message = "This promo code is no longer active.";
                response.Code = HttpStatusCode.BadRequest;
                return response;
            }

            if (promo.ExpiryDate < DateTime.UtcNow)
            {
                response.IsSuccess = false;
                response.Message = "This promo code has expired.";
                response.Code = HttpStatusCode.BadRequest;
                return response;
            }

            if (promo.CurrentUsage >= promo.MaxUsage)
            {
                response.IsSuccess = false;
                response.Message = "This promo code has reached its maximum usage limit.";
                response.Code = HttpStatusCode.BadRequest;
                return response;
            }


            if (promo.CourseId.HasValue && promo.CourseId != courseId)
            {
                response.IsSuccess = false;
                response.Message = "This promo code is not valid for the selected item.";
                response.Code = HttpStatusCode.BadRequest;
                return response;
            }

            response.IsSuccess = true;
            response.Message = "Promo code applied successfully.";
            response.Code = HttpStatusCode.OK;

            response.Result.Add("discount", JsonValue.Create(promo.DiscountValue));
            response.Result.Add("isPercentage", JsonValue.Create(promo.IsPercentage));

            return response;
        }
    }
}
