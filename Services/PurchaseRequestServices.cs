using AutoMapper;
using EdgePMO.API.Contracts;
using EdgePMO.API.Dtos;
using EdgePMO.API.Models;
using EdgePMO.API.Settings;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore.Storage;

namespace EdgePMO.API.Services
{
    public class PurchaseRequestServices : IPurchaseRequestServices
    {
        private readonly EdgepmoDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;

        public PurchaseRequestServices(EdgepmoDbContext context, IEmailService emailService, IMapper mapper)
        {
            _context = context;
            _emailService = emailService;
            _mapper = mapper;
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

            if (!string.IsNullOrWhiteSpace(dto.IdempotencyKey))
            {
                PurchaseRequest? existing = await _context.Set<PurchaseRequest>()
                                                          .AsNoTracking()
                                                          .FirstOrDefaultAsync(r => r.IdempotencyKey == dto.IdempotencyKey && r.UserId == requestorId);

                if (existing != null)
                {
                    response.IsSuccess = true;
                    response.Message = "Duplicate request detected.";
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

        public async Task<Response> GetByIdAsync(Guid id)
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
                response.Code = HttpStatusCode.BadRequest;
                return response;
            }

            PurchaseRequestResponseDto? dto = _mapper.Map<PurchaseRequestResponseDto>(pr);
            response.IsSuccess = true;
            response.Message = "Purchase request retrieved.";
            response.Code = HttpStatusCode.OK;
            response.Result.Add("request", JsonSerializer.SerializeToNode(dto) ?? JsonValue.Create(new { }));
            return response;
        }

        public async Task<Response> GetForUserAsync(Guid userId)
        {
            Response response = new Response();

            List<PurchaseRequest>? list = await _context.Set<PurchaseRequest>()
                .AsNoTracking()
                .Include(pr => pr.Course)
                .Include(pr => pr.Template)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.RequestedAt)
                .ToListAsync();

            List<PurchaseRequestResponseDto>? dtoList = list.Select(pr => _mapper.Map<PurchaseRequestResponseDto>(pr)).ToList();

            response.IsSuccess = true;
            response.Message = "User purchase requests retrieved.";
            response.Code = HttpStatusCode.OK;
            response.Result.Add("requests", JsonSerializer.SerializeToNode(dtoList) ?? JsonValue.Create(Array.Empty<object>()));
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

            List<PurchaseRequestResponseDto>? dtoList = list.Select(pr => _mapper.Map<PurchaseRequestResponseDto>(pr)).ToList();

            response.IsSuccess = true;
            response.Message = "Purchase requests retrieved.";
            response.Code = HttpStatusCode.OK;
            response.Result.Add("requests", JsonSerializer.SerializeToNode(dtoList));
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

            if (!string.Equals(pr.Status, PurchaseRequestStatus.Pending.ToString().ToLowerInvariant(), StringComparison.OrdinalIgnoreCase))
            {
                response.IsSuccess = false;
                response.Message = "Purchase request already processed.";
                response.Code = HttpStatusCode.BadRequest;
                return response;
            }

            // Use transaction to ensure atomicity
            await using IDbContextTransaction? tx = await _context.Database.BeginTransactionAsync();
            try
            {
                User? adminUser = await _context.Users.FindAsync(adminId);
                if (adminUser == null || !(adminUser.IsAdmin ?? false))
                {
                    await tx.RollbackAsync();
                    response.IsSuccess = false;
                    response.Message = "Admin user not found or not authorized.";
                    response.Code = HttpStatusCode.Unauthorized;
                    return response;
                }
                Purchase? purchase = new Purchase
                {
                    Id = Guid.NewGuid(),
                    UserId = pr.UserId,
                    TemplateId = pr.TemplateId,
                    CourseId = pr.CourseId,
                    PurchaseType = pr.TemplateId.HasValue ? "template" : "course",
                    PaymentMethod = "manual",
                    TransactionId = $"manual-{Guid.NewGuid()}",
                    Status = "completed",
                    PurchasedAt = DateTime.UtcNow,
                    Notes = $"Approved by admin {adminUser.Email}"
                };

                _context.Purchases.Add(purchase);

                if (purchase.TemplateId.HasValue)
                {
                    UserTemplate? ut = new UserTemplate
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
                        CourseUser? cu = new CourseUser
                        {
                            CourseId = purchase.CourseId.Value,
                            UserId = purchase.UserId,
                            EnrolledAt = DateTime.UtcNow,
                            Progress = 0.0
                        };
                        _context.CourseUsers.Add(cu);
                    }
                }

                pr.Status = PurchaseRequestStatus.Approved.ToString().ToLowerInvariant();
                pr.AdminId = adminId;
                pr.DecisionAt = DateTime.UtcNow;
                pr.DecisionAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                try
                {
                    User? user = await _context.Users.FindAsync(pr.UserId);
                    if (user != null && !string.IsNullOrWhiteSpace(user.Email))
                    {
                        await SendApprovalEmailAsync(user, pr);
                    }
                }
                catch
                {
                    response.IsSuccess = true;
                    response.Message = "Purchase request approved and access granted. but the approval email not sent for the user, please check the your mail provider";
                    response.Code = HttpStatusCode.OK;
                    return response;
                }

                response.IsSuccess = true;
                response.Message = "Purchase request approved and access granted.";
                response.Code = HttpStatusCode.OK;
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

        public async Task<Response> RejectAsync(Guid requestId, Guid adminId, List<string> reasons)
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

            if (!string.Equals(pr.Status, PurchaseRequestStatus.Pending.ToString().ToLowerInvariant(), StringComparison.OrdinalIgnoreCase))
            {
                response.IsSuccess = false;
                response.Message = "Purchase request already processed.";
                response.Code = HttpStatusCode.BadRequest;
                return response;
            }

            pr.Status = PurchaseRequestStatus.Rejected.ToString().ToLowerInvariant();
            pr.AdminId = adminId;
            pr.DecisionAt = DateTime.UtcNow;
            pr.Notes = (pr.Notes ?? string.Empty) + $" | Rejection reason: {string.Join(", ",reasons)}";

            await _context.SaveChangesAsync();

            try
            {
                User? user = await _context.Users.FindAsync(pr.UserId);
                if (user != null && !string.IsNullOrWhiteSpace(user.Email))
                {
                    await SendRejectionEmailAsync(user, pr, string.Join(", ", reasons));
                }
            }
            catch
            {
                response.IsSuccess = true;
                response.Message = "Purchase request rejected. but the rejection email not sent for the user, please check the your mail provider";
                response.Code = HttpStatusCode.OK;
                return response;
            }

            response.IsSuccess = true;
            response.Message = "Purchase request rejected.";
            response.Code = HttpStatusCode.OK;
            return response;
        }


        public async Task SendApprovalEmailAsync(User user, PurchaseRequest pr)
        {
            string htmlBody = $@"
                            <!DOCTYPE html>
                            <html lang='en'>
                            <head>
                                <meta charset='UTF-8'>
                                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                                <title>Purchase Request Approved</title>
                                <style>
                                    * {{ margin: 0; padding: 0; box-sizing: border-box; }}
                                    body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; background-color: #f5f5f5; padding: 20px; }}
                                    .container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1); }}
                                    .header {{ background: linear-gradient(135deg, #10b981 0%, #059669 100%); padding: 40px 20px; text-align: center; color: white; }}
                                    .header h1 {{ font-size: 28px; font-weight: 600; margin-bottom: 8px; }}
                                    .content {{ padding: 40px 30px; }}
                                    .greeting {{ font-size: 16px; color: #333; margin-bottom: 24px; line-height: 1.6; }}
                                    .status-box {{ background-color: #ecfdf5; border-left: 4px solid #10b981; padding: 20px; margin: 24px 0; border-radius: 4px; }}
                                    .status-box .label {{ font-size: 12px; color: #059669; font-weight: 600; text-transform: uppercase; }}
                                    .status-box .value {{ font-size: 14px; color: #333; margin-top: 8px; font-family: 'Courier New', monospace; }}
                                    .details {{ background-color: #f9fafb; padding: 20px; border-radius: 4px; margin: 24px 0; }}
                                    .detail-row {{ display: flex; justify-content: space-between; padding: 12px 0; border-bottom: 1px solid #e5e7eb; font-size: 14px; }}
                                    .detail-row .label {{ color: #6b7280; font-weight: 500; }}
                                    .detail-row .value {{ color: #333; font-weight: 600; }}
                                    .message {{ font-size: 14px; color: #555; line-height: 1.6; margin: 24px 0; }}
                                    .button {{ display: inline-block; background: linear-gradient(135deg, #10b981 0%, #059669 100%); color: white; padding: 12px 32px; border-radius: 6px; text-decoration: none; font-weight: 600; }}
                                    .footer {{ background-color: #f9fafb; padding: 24px; text-align: center; border-top: 1px solid #e5e7eb; font-size: 12px; color: #6b7280; }}
                                </style>
                            </head>
                            <body>
                                <div class='container'>
                                    <div class='header'>
                                        <h1>✓ Request Approved</h1>
                                        <p>Your purchase request has been approved</p>
                                    </div>
                                    <div class='content'>
                                        <div class='greeting'>Hi <strong>{user.FirstName}</strong>,</div>
                                        <p class='message'>Great news! Your purchase request has been successfully approved by our admin team. You now have access to proceed with your purchase.</p>
                                        <div class='details'>
                                            <div class='detail-row'>
                                                <span class='label'>Status</span>
                                                <span class='value' style='color: #10b981;'>✓ Approved</span>
                                            </div>
                                            <div class='detail-row'>
                                                <span class='label'>Approved On</span>
                                                <span class='value'>{DateTime.Now:MMMM dd, yyyy}</span>
                                            </div>
                                        </div>
                                        <p class='message'>If you have any questions, please contact our support team.</p>
                                    </div>
                                    <div class='footer'>
                                        <p>© 2025 EdgePMO. All rights reserved.</p>
                                    </div>
                                </div>
                            </body>
                            </html>";

            await _emailService.SendEmailAsync(user.Email, "Your Purchase Request Has Been Approved", htmlBody, true);
        }

        public async Task SendRejectionEmailAsync(User user, PurchaseRequest pr, string rejectionReasons)
        {
            string htmlBody = $@"
                    <!DOCTYPE html>
                    <html lang='en'>
                    <head>
                        <meta charset='UTF-8'>
                        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                        <title>Purchase Request Rejected</title>
                        <style>
                            * {{ margin: 0; padding: 0; box-sizing: border-box; }}
                            body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; background-color: #f5f5f5; padding: 20px; }}
                            .container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1); }}
                            .header {{ background: linear-gradient(135deg, #ef4444 0%, #dc2626 100%); padding: 40px 20px; text-align: center; color: white; }}
                            .header h1 {{ font-size: 28px; font-weight: 600; margin-bottom: 8px; }}
                            .content {{ padding: 40px 30px; }}
                            .greeting {{ font-size: 16px; color: #333; margin-bottom: 24px; line-height: 1.6; }}
                            .status-box {{ background-color: #fef2f2; border-left: 4px solid #ef4444; padding: 20px; margin: 24px 0; border-radius: 4px; }}
                            .status-box .label {{ font-size: 12px; color: #dc2626; font-weight: 600; text-transform: uppercase; }}
                            .reason-box {{ background-color: #f9fafb; padding: 20px; border-radius: 4px; margin: 24px 0; border: 1px solid #e5e7eb; }}
                            .reason-box .label {{ font-size: 12px; color: #6b7280; font-weight: 600; margin-bottom: 8px; }}
                            .reason-box .content {{ font-size: 14px; color: #333; line-height: 1.6; }}
                            .details {{ background-color: #f9fafb; padding: 20px; border-radius: 4px; margin: 24px 0; }}
                            .detail-row {{ display: flex; justify-content: space-between; padding: 12px 0; border-bottom: 1px solid #e5e7eb; font-size: 14px; }}
                            .detail-row .label {{ color: #6b7280; font-weight: 500; }}
                            .detail-row .value {{ color: #333; font-weight: 600; }}
                            .message {{ font-size: 14px; color: #555; line-height: 1.6; margin: 24px 0; }}
                            .footer {{ background-color: #f9fafb; padding: 24px; text-align: center; border-top: 1px solid #e5e7eb; font-size: 12px; color: #6b7280; }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>
                                <h1>✕ Request Rejected</h1>
                                <p>Your purchase request requires revision</p>
                            </div>
                            <div class='content'>
                                <div class='greeting'>Hi <strong>{user.FirstName}</strong>,</div>
                                <p class='message'>Thank you for submitting your purchase request. After review, we are unable to approve it at this time.</p>
                                <div class='status-box'>
                                    <div class='label'>Request ID</div>
                                    <div class='value'>{pr.Id}</div>
                                </div>
                                <div class='details'>
                                    <div class='detail-row'>
                                        <span class='label'>Status</span>
                                        <span class='value' style='color: #ef4444;'>✕ Rejected</span>
                                    </div>
                                    <div class='detail-row'>
                                        <span class='label'>Rejected On</span>
                                        <span class='value'>{DateTime.Now:MMMM dd, yyyy}</span>
                                    </div>
                                </div>
                                <div class='reason-box'>
                                    <div class='label'>Reason for Rejection</div>
                                    <div class='content'>{rejectionReasons}</div>
                                </div>
                                <p class='message'>Please review the rejection reason and resubmit your request with the necessary corrections.</p>
                            </div>
                            <div class='footer'>
                                <p>© 2025 EdgePMO. All rights reserved.</p>
                            </div>
                        </div>
                    </body>
                    </html>";

            await _emailService.SendEmailAsync(user.Email, "Your Purchase Request Requires Revision", htmlBody, true);
        }

    }
}
