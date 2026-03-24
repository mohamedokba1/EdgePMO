using System.ComponentModel.DataAnnotations;

namespace EdgePMO.API.Models
{
    public class PromoCode
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public decimal DiscountValue { get; set; }
        public bool IsPercentage { get; set; }
        public Guid? CourseId { get; set; }
        public Course? Course { get; set; }
        public Guid? TemplateId { get; set; }
        public Template? Template { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int MaxUsage { get; set; }
        public int CurrentUsage { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
