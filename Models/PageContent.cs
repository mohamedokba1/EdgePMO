namespace EdgePMO.API.Models
{
    public class PageContent
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Slug { get; set; } = null!; // unique, e.g. "home", "about", "pricing"
        public string Name { get; set; } = null!; // human friendly name
        public string Data { get; set; } = "{}"; // JSON payload for the page structure/blocks
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
