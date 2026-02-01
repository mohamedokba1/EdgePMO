namespace EdgePMO.API.Dtos
{
    public record RefreshToken
    {
        public string Token { get; set; }
        public Guid SessionId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
