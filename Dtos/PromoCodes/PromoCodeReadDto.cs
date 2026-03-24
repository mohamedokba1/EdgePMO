namespace EdgePMO.API.Dtos.PromoCodes
{
    public record PromoCodeReadDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public decimal DiscountValue { get; set; }
        public bool IsPercentage { get; set; }
        public Guid? CourseId { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int MaxUsage { get; set; }
        public int CurrentUsage { get; set; }
        public bool IsActive { get; set; }
    }
}
