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
            // Projected rather than serializing the raw entity — `.Include(p => p.User)`
            // pulls in PasswordHash/PasswordSalt/RefreshToken, which is exactly the bug
            // already found and fixed on the course-reviews endpoint (it also crashed
            // System.Text.Json mid-stream, surfacing to the browser as a bare
            // net::ERR_FAILED). This endpoint has apparently never actually been called
            // by the frontend before now, so the bug was latent — fixing it before
            // wiring the dashboard's revenue reporting up to it.
            Response response = new Response();

            List<object> list = await _context.Purchases
                .AsNoTracking()
                .OrderByDescending(p => p.PurchasedAt)
                .Select(p => new
                {
                    p.Id,
                    p.UserId,
                    UserName = p.User != null ? $"{p.User.FirstName} {p.User.LastName}".Trim() : null,
                    UserEmail = p.User != null ? p.User.Email : null,
                    p.CourseId,
                    CourseName = p.Course != null ? p.Course.Name : null,
                    p.TemplateId,
                    TemplateName = p.Template != null ? p.Template.Name : null,
                    p.PurchaseType,
                    p.Amount,
                    p.Currency,
                    p.PaymentMethod,
                    p.TransactionId,
                    p.Status,
                    p.PurchasedAt,
                    p.RefundedAt,
                    p.Notes,
                })
                .ToListAsync<object>();

            response.IsSuccess = true;
            response.Message = "Purchases retrieved.";
            response.Code = HttpStatusCode.OK;
            response.Result.Add("purchases", JsonSerializer.SerializeToNode(list) ?? JsonValue.Create(Array.Empty<object>()));
            return response;
        }

        public async Task<Response> GetByIdAsync(Guid id)
        {
            Response response = new Response();

            var purchase = await _context.Purchases
                .AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => new
                {
                    p.Id,
                    p.UserId,
                    UserName = p.User != null ? $"{p.User.FirstName} {p.User.LastName}".Trim() : null,
                    UserEmail = p.User != null ? p.User.Email : null,
                    p.CourseId,
                    CourseName = p.Course != null ? p.Course.Name : null,
                    p.TemplateId,
                    TemplateName = p.Template != null ? p.Template.Name : null,
                    p.PurchaseType,
                    p.Amount,
                    p.Currency,
                    p.PaymentMethod,
                    p.TransactionId,
                    p.Status,
                    p.PurchasedAt,
                    p.RefundedAt,
                    p.Notes,
                })
                .FirstOrDefaultAsync();

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
