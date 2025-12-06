using EdgePMO.API.Contracts;
using EdgePMO.API.Dtos;
using EdgePMO.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace EdgePMO.API.Services
{
    public class PurchaseRequestServices : IPurchaseRequestServices
    {
        private readonly EdgepmoDbContext _context;
        private readonly IEmailService _emailService;

        public PurchaseRequestServices(EdgepmoDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<Response> CreateRequestAsync(PurchaseRequestCreateDto dto, Guid requestorId)
        {
            Response response = new Response();

            if (dto.TemplateId == null && dto.CourseId == null)
            {
                response.IsSuccess = false;
                response.Message = "Either TemplateId or CourseId is required.";
                response.Code = HttpStatusCode.BadRequest;
                return response;
            }

            if (dto.TemplateId != null)
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

            if (dto.CourseId != null)
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

            // Idempotency: if client provided IdempotencyKey, return existing request (for same user) instead of creating duplicate
            if (!string.IsNullOrWhiteSpace(dto.IdempotencyKey))
            {
                var existing = await _context.Set<PurchaseRequest>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.IdempotencyKey == dto.IdempotencyKey && r.UserId == requestorId);

                if (existing != null)
                {
                    response.IsSuccess = true;
                    response.Message = "Duplicate request detected. Returning existing request.";
                    response.Code = HttpStatusCode.OK;
                    response.Result.Add("request", JsonSerializer.SerializeToNode(existing) ?? JsonValue.Create(new { }));
                    return response;
                }
            }

            PurchaseRequest? pr = new PurchaseRequest
            {
                UserId = requestorId,
                TemplateId = dto.TemplateId,
                CourseId = dto.CourseId,
                RequestedAmount = dto.RequestedAmount,
                Currency = dto.Currency,
                Notes = dto.Notes,
                Status = "Pending",
                RequestedAt = DateTime.UtcNow,
                IdempotencyKey = string.IsNullOrWhiteSpace(dto.IdempotencyKey) ? null : dto.IdempotencyKey
            };

            _context.Add(pr);
            await _context.SaveChangesAsync();

            response.IsSuccess = true;
            response.Message = "Purchase request created.";
            response.Code = HttpStatusCode.Created;
            response.Result.Add("request", JsonSerializer.SerializeToNode(pr) ?? JsonValue.Create(new { }));
            return response;
        }

        public async Task<Response> GetByIdAsync(Guid id, Guid? requesterId = null)
        {
            Response response = new Response();

            PurchaseRequest? pr = await _context.Set<PurchaseRequest>()
                        .AsNoTracking()
                        .Include(x => x.User)
                        .Include(x => x.Template)
                        .Include(x => x.Course)
                        .FirstOrDefaultAsync(x => x.Id == id);

            if (pr == null)
            {
                response.IsSuccess = false;
                response.Message = "Purchase request not found.";
                response.Code = HttpStatusCode.NotFound;
                return response;
            }

            if (requesterId.HasValue && pr.UserId != requesterId.Value)
            {
                response.IsSuccess = false;
                response.Message = "Not authorized to view this request.";
                response.Code = HttpStatusCode.Forbidden;
                return response;
            }

            response.IsSuccess = true;
            response.Message = "Purchase request retrieved.";
            response.Code = HttpStatusCode.OK;
            response.Result.Add("request", JsonSerializer.SerializeToNode(pr) ?? JsonValue.Create(new { }));
            return response;
        }

        public async Task<Response> GetForUserAsync(Guid userId)
        {
            Response response = new Response();

            List<PurchaseRequest>? list = await _context.Set<PurchaseRequest>()
                .AsNoTracking()
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.RequestedAt)
                .ToListAsync();

            response.IsSuccess = true;
            response.Message = "User purchase requests retrieved.";
            response.Code = HttpStatusCode.OK;
            response.Result.Add("requests", JsonSerializer.SerializeToNode(list) ?? JsonValue.Create(Array.Empty<object>()));
            return response;
        }

        public async Task<Response> GetAllAsync()
        {
            Response response = new Response();

            List<PurchaseRequest>? list = await _context.Set<PurchaseRequest>()
                .AsNoTracking()
                .Include(r => r.User)
                .Include(r => r.Template)
                .Include(r => r.Course)
                .OrderByDescending(r => r.RequestedAt)
                .ToListAsync();

            response.IsSuccess = true;
            response.Message = "Purchase requests retrieved.";
            response.Code = HttpStatusCode.OK;
            response.Result.Add("requests", JsonSerializer.SerializeToNode(list) ?? JsonValue.Create(Array.Empty<object>()));
            return response;
        }

        public async Task<Response> ApproveAsync(Guid requestId, Guid adminId)
        {
            Response response = new Response();

            PurchaseRequest? pr = await _context.Set<PurchaseRequest>().FirstOrDefaultAsync(r => r.Id == requestId);
            if (pr == null)
            {
                response.IsSuccess = false;
                response.Message = "Purchase request not found.";
                response.Code = HttpStatusCode.NotFound;
                return response;
            }

            //if (!string.Equals(pr.Status, PurchaseRequestStatus.Pending.ToString().ToLowerInvariant(), StringComparison.OrdinalIgnoreCase))
            //{
            //    response.IsSuccess = false;
            //    response.Message = "Purchase request already processed.";
            //    response.Code = HttpStatusCode.BadRequest;
            //    return response;
            //}

            // Use transaction to ensure atomicity
            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                decimal amount = pr.RequestedAmount
                    ?? (pr.TemplateId.HasValue ? (await _context.Templates.FindAsync(pr.TemplateId.Value))?.Price ?? 0m : 0m);

                var purchase = new Purchase
                {
                    Id = Guid.NewGuid(),
                    UserId = pr.UserId,
                    TemplateId = pr.TemplateId,
                    CourseId = pr.CourseId,
                    PurchaseType = pr.TemplateId.HasValue ? "template" : "course",
                    Amount = amount,
                    Currency = pr.Currency ?? "USD",
                    PaymentMethod = "manual",
                    TransactionId = $"manual-{Guid.NewGuid()}",
                    Status = "completed",
                    PurchasedAt = DateTime.UtcNow,
                    Notes = $"Approved by admin {adminId}"
                };

                _context.Purchases.Add(purchase);

                if (purchase.TemplateId.HasValue)
                {
                    var ut = new UserTemplate
                    {
                        Id = Guid.NewGuid(),
                        UserId = purchase.UserId,
                        TemplateId = purchase.TemplateId.Value,
                        PurchaseId = purchase.Id,
                        PurchasedAt = DateTime.UtcNow
                    };
                    _context.UserTemplates.Add(ut);
                }

                if (purchase.CourseId.HasValue)
                {
                    bool already = await _context.CourseUsers.AnyAsync(cu => cu.CourseId == purchase.CourseId && cu.UserId == purchase.UserId);
                    if (!already)
                    {
                        var cu = new CourseUser
                        {
                            CourseId = purchase.CourseId.Value,
                            UserId = purchase.UserId,
                            EnrolledAt = DateTime.UtcNow,
                            Progress = 0.0
                        };
                        _context.CourseUsers.Add(cu);
                    }
                }

                //pr.Status = PurchaseRequestStatus.Accepted.ToString().ToLowerInvariant();
                pr.AdminId = adminId;
                pr.DecisionAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                try
                {
                    var user = await _context.Users.FindAsync(pr.UserId);
                    if (user != null && !string.IsNullOrWhiteSpace(user.Email))
                    {
                        string subject = "Your purchase request has been approved";
                        string body = $"Hello {user.FirstName},\n\nYour purchase request (id: {pr.Id}) has been approved by admin. Access has been granted.\n\nRegards,\nAdmin";
                        await _emailService.SendEmailVerficationAsync(user.Email, subject, body);
                    }
                }
                catch
                {
                }

                response.IsSuccess = true;
                response.Message = "Purchase request approved and access granted.";
                response.Code = HttpStatusCode.OK;
                response.Result.Add("purchase", JsonSerializer.SerializeToNode(purchase) ?? JsonValue.Create(new { }));
                response.Result.Add("request", JsonSerializer.SerializeToNode(pr) ?? JsonValue.Create(new { }));
                return response;
            }
            catch
            {
                await tx.RollbackAsync();
                response.IsSuccess = false;
                response.Message = "Failed to approve purchase request.";
                response.Code = HttpStatusCode.InternalServerError;
                return response;
            }
        }

        public async Task<Response> RejectAsync(Guid requestId, Guid adminId, string reason)
        {
            Response response = new Response();

            var pr = await _context.Set<PurchaseRequest>().FirstOrDefaultAsync(r => r.Id == requestId);
            if (pr == null)
            {
                response.IsSuccess = false;
                response.Message = "Purchase request not found.";
                response.Code = HttpStatusCode.NotFound;
                return response;
            }

            //if (!string.Equals(pr.Status, PurchaseRequestStatus.Pending.ToString().ToLowerInvariant(), StringComparison.OrdinalIgnoreCase))
            //{
            //    response.IsSuccess = false;
            //    response.Message = "Purchase request already processed.";
            //    response.Code = HttpStatusCode.BadRequest;
            //    return response;
            //}

            //pr.Status = PurchaseRequestStatus.Rejected.ToString().ToLowerInvariant();
            pr.AdminId = adminId;
            pr.DecisionAt = DateTime.UtcNow;
            pr.Notes = (pr.Notes ?? string.Empty) + $" | Rejection reason: {reason}";

            await _context.SaveChangesAsync();

            try
            {
                var user = await _context.Users.FindAsync(pr.UserId);
                if (user != null && !string.IsNullOrWhiteSpace(user.Email))
                {
                    string subject = "Your purchase request has been rejected";
                    string body = $"Hello {user.FirstName},\n\nYour purchase request (id: {pr.Id}) has been rejected. Reason: {reason}\n\nRegards,\nAdmin";
                    await _emailService.SendEmailVerficationAsync(user.Email, subject, body);
                }
            }
            catch
            {
            }

            response.IsSuccess = true;
            response.Message = "Purchase request rejected.";
            response.Code = HttpStatusCode.OK;
            response.Result.Add("request", JsonSerializer.SerializeToNode(pr) ?? JsonValue.Create(new { }));
            return response;
        }
    }
}
