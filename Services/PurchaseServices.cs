using EdgePMO.API.Contracts;
using EdgePMO.API.Dtos;
using EdgePMO.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace EdgePMO.API.Services
{
    public class PurchaseServices : IPurchaseServices
    {
        private readonly EdgepmoDbContext _context;

        public PurchaseServices(EdgepmoDbContext context)
        {
            _context = context;
        }

        public async Task<Response> GetAllAsync()
        {
            Response response = new Response();
            List<Purchase>? list = await _context.Purchases
                .AsNoTracking()
                .Include(p => p.User)
                .Include(p => p.Template)
                .Include(p => p.Course)
                .OrderByDescending(p => p.PurchasedAt)
                .ToListAsync();

            response.IsSuccess = true;
            response.Message = "Purchases retrieved.";
            response.Code = HttpStatusCode.OK;
            response.Result.Add("purchases", JsonSerializer.SerializeToNode(list) ?? JsonValue.Create(Array.Empty<object>()));
            return response;
        }

        public async Task<Response> GetByIdAsync(Guid id)
        {
            Response response = new Response();
            Purchase? purchase = await _context.Purchases
                .AsNoTracking()
                .Include(p => p.User)
                .Include(p => p.Template)
                .Include(p => p.Course)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (purchase == null)
            {
                response.IsSuccess = false;
                response.Message = "Purchase not found.";
                response.Code = HttpStatusCode.NotFound;
                return response;
            }

            response.IsSuccess = true;
            response.Message = "Purchase retrieved.";
            response.Code = HttpStatusCode.OK;
            response.Result.Add("purchase", JsonSerializer.SerializeToNode(purchase) ?? JsonValue.Create(new { }));
            return response;
        }

        public async Task<Response> CreateAsync(PurchaseCreateDto dto)
        {
            Response response = new Response();

            User? user = await _context.Users.FindAsync(dto.UserId);
            if (user == null)
            {
                response.IsSuccess = false;
                response.Message = "User not found.";
                response.Code = HttpStatusCode.BadRequest;
                return response;
            }

            if (dto.TemplateId.HasValue)
            {
                Template? tpl = await _context.Templates.FindAsync(dto.TemplateId.Value);
                if (tpl == null)
                {
                    response.IsSuccess = false;
                    response.Message = "Template not found.";
                    response.Code = HttpStatusCode.BadRequest;
                    return response;
                }
            }

            if (dto.CourseId.HasValue)
            {
                Course? course = await _context.Courses.FindAsync(dto.CourseId.Value);
                if (course == null)
                {
                    response.IsSuccess = false;
                    response.Message = "Course not found.";
                    response.Code = HttpStatusCode.BadRequest;
                    return response;
                }
            }

            Purchase? purchase = new Purchase
            {
                Id = Guid.NewGuid(),
                UserId = dto.UserId,
                TemplateId = dto.TemplateId,
                CourseId = dto.CourseId,
                PurchaseType = dto.PurchaseType,
                Amount = dto.Amount,
                Currency = dto.Currency,
                PaymentMethod = dto.PaymentMethod,
                TransactionId = dto.TransactionId,
                Status = dto.Status ?? "completed",
                PurchasedAt = DateTime.UtcNow,
                Notes = dto.Notes
            };

            _context.Purchases.Add(purchase);
            await _context.SaveChangesAsync();

            if (purchase.TemplateId.HasValue)
            {
                UserTemplate? userTemplate = new UserTemplate
                {
                    Id = Guid.NewGuid(),
                    UserId = purchase.UserId,
                    TemplateId = purchase.TemplateId.Value,
                    PurchaseId = purchase.Id,
                    PurchasedAt = DateTime.UtcNow
                };
                _context.UserTemplates.Add(userTemplate);
                await _context.SaveChangesAsync();
            }

            response.IsSuccess = true;
            response.Message = "Purchase recorded.";
            response.Code = HttpStatusCode.Created;
            response.Result.Add("purchase", JsonSerializer.SerializeToNode(purchase) ?? JsonValue.Create(new { }));
            return response;
        }

        public async Task<Response> UpdateStatusAsync(Guid id, string status)
        {
            Response response = new Response();
            Purchase? purchase = await _context.Purchases.FindAsync(id);
            if (purchase == null)
            {
                response.IsSuccess = false;
                response.Message = "Purchase not found.";
                response.Code = HttpStatusCode.NotFound;
                return response;
            }

            purchase.Status = status;
            if (status.Equals("refunded", StringComparison.OrdinalIgnoreCase))
                purchase.RefundedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            response.IsSuccess = true;
            response.Message = "Purchase updated.";
            response.Code = HttpStatusCode.OK;
            response.Result.Add("purchase", JsonSerializer.SerializeToNode(purchase) ?? JsonValue.Create(new { }));
            return response;
        }

        public async Task<Response> DeleteAsync(Guid id)
        {
            Response response = new Response();
            Purchase? existing = await _context.Purchases.FindAsync(id);
            if (existing == null)
            {
                response.IsSuccess = false;
                response.Message = "Purchase not found.";
                response.Code = HttpStatusCode.NotFound;
                return response;
            }

            _context.Purchases.Remove(existing);
            await _context.SaveChangesAsync();

            response.IsSuccess = true;
            response.Message = "Purchase deleted.";
            response.Code = HttpStatusCode.NoContent;
            return response;
        }
    }
}
