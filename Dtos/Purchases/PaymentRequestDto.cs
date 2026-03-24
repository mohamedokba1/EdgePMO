namespace EdgePMO.API.Dtos
{
    public class PaymentRequestDto
    {
        public Guid? CourseId { get; set; }
        public Guid? TemplateId { get; set; }
        public string? PromoCode { get; set; }
    }
}
