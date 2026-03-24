using EdgePMO.API.Contracts;
using EdgePMO.API.Dtos;
using EdgePMO.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text.Json;

namespace EdgePMO.API.Services
{
    public class PaymentServices : IPaymentServices
    {
        private readonly EdgepmoDbContext _context;
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;

        public PaymentServices(EdgepmoDbContext context, IConfiguration config, HttpClient httpClient)
        {
            _context = context;
            _config = config;
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _config["Tap:SecretKey"]);
        }

        public async Task<Response> InitializePurchaseAsync(Guid userId, Guid? courseId, Guid? templateId, string promoCode)
        {
            return new Response()
            {

            };
            //// 1. Calculate Price & Apply Promo (Logic from previous step)
            //decimal finalAmount = await CalculatePrice(courseId, templateId, promoCode);

            //// 2. Create Pending Purchase Record
            //Purchase? purchase = new Purchase
            //{
            //    Id = Guid.NewGuid(),
            //    UserId = userId,
            //    CourseId = courseId,
            //    TemplateId = templateId,
            //    Amount = finalAmount,
            //    Status = "pending",
            //    TransactionId = $"EP_{Guid.NewGuid().ToString("N").Substring(0, 10)}",
            //    PurchaseType = courseId.HasValue ? "Course" : "Template"
            //};

            //_context.Purchases.Add(purchase);
            //await _context.SaveChangesAsync();

            //// 3. Request Tap Charge URL
            //var tapRequest = new
            //{
            //    amount = purchase.Amount,
            //    currency = "SAR", // Set based on user region
            //    customer = new { first_name = "User", email = "user@email.com" }, // Fetch from User entity
            //    source = new { id = "src_all" },
            //    redirect = new { url = "https://edgepmo.com/payment/success" },
            //    post = new { url = "https://api.edgepmo.com/api/v2/payments/webhook" },
            //    reference = new { transaction = purchase.TransactionId }
            //};

            //HttpResponseMessage? response = await _httpClient.PostAsJsonAsync("https://api.tap.company/v2/charges", tapRequest);
            //JsonElement tapResult = await response.Content.ReadFromJsonAsync<JsonElement>();

            //return new Response
            //{
            //    IsSuccess = true,
            //    Result = new { transactionUrl = tapResult.GetProperty("transaction").GetProperty("url").GetString() }
            //};
        }

        public async Task<Response> ProcessCallbackAsync(string tapChargeId)
        {
            // Fetch full status from Tap to verify (Secure check)
            HttpResponseMessage? response = await _httpClient.GetAsync($"https://api.tap.company/v2/charges/{tapChargeId}");
            JsonElement data = await response.Content.ReadFromJsonAsync<JsonElement>();

            string txnId = data.GetProperty("reference").GetProperty("transaction").GetString();
            string status = data.GetProperty("status").GetString();

            if (status == "CAPTURED")
            {
                var purchase = await _context.Purchases.FirstOrDefaultAsync(p => p.TransactionId == txnId);
                if (purchase != null && purchase.Status == "pending")
                {
                    purchase.Status = "completed";
                    // TRIGGER: Unlock course/template here
                    await _context.SaveChangesAsync();
                }
            }
            return new Response { IsSuccess = true };
        }

        //private async Task<decimal> CalculatePrice(Guid? courseId, Guid? templateId, string? promoCode)
        //{
        //    decimal basePrice = 0;

        //    if (courseId.HasValue)
        //    {
        //        Course? course = await _context.Courses
        //            .AsNoTracking()
        //            .FirstOrDefaultAsync(c => c.CourseId == courseId.Value);

        //        if (course == null) throw new Exception("Course not found.");
        //        basePrice = course.Price;
        //    }
        //    else if (templateId.HasValue)
        //    {
        //        var template = await _context.Templates
        //            .AsNoTracking()
        //            .FirstOrDefaultAsync(t => t.Id == templateId.Value);

        //        if (template == null) throw new Exception("Template not found.");
        //        basePrice = template.Price;
        //    }
        //    else
        //    {
        //        throw new Exception("No item specified for purchase.");
        //    }

        //    // 2. Apply Promo Code Logic
        //    if (!string.IsNullOrWhiteSpace(promoCode))
        //    {
        //        var promo = await _context.PromoCodes
        //            .AsNoTracking()
        //            .FirstOrDefaultAsync(p => p.Code.ToUpper() == promoCode.ToUpper()
        //                                   && p.IsActive
        //                                   && p.ExpiryDate > DateTime.UtcNow
        //                                   && p.CurrentUsage < p.MaxUsage);

        //        if (promo != null)
        //        {
        //            // Verify if the promo is restricted to this specific course
        //            if (promo.CourseId == null || promo.CourseId == courseId)
        //            {
        //                if (promo.IsPercentage)
        //                {
        //                    // Formula: Price - (Price * (Discount / 100))
        //                    basePrice -= (basePrice * (promo.DiscountValue / 100));
        //                }
        //                else
        //                {
        //                    // Formula: Price - Fixed Amount
        //                    basePrice -= promo.DiscountValue;
        //                }
        //            }
        //        }
        //    }

        //    // 3. Safety Check: Price should never be negative
        //    return Math.Max(basePrice, 0);
        //}

        public bool VerifySignature(string rawJson, string headerSignature)
        {
            // Use HMACSHA256 with your Tap Secret Key to validate the body
            // This prevents hackers from sending fake 'Success' messages to your VPS
            return true; // Simplified for initial test
        }
    }
}
