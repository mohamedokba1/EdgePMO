namespace EdgePMO.API.Dtos
{
    public record UserReadDto
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
