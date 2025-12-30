namespace EdgePMO.API.Dtos
{
    public record UpdatePurchaseStatusDto
    {
        public string Status { get; init; } = null!;
    }
}
